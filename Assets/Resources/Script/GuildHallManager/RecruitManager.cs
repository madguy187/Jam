using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

namespace StoryManager
{
    public class RecruitController : MonoBehaviour
    {
        public static RecruitController instance { get; private set; }

        [Header("UI References")]
        [SerializeField] private UnitSlot[] unitSlots = new UnitSlot[3];
        [SerializeField] private Button     takePartyButton;
        [SerializeField] private TextMeshProUGUI takePartyButtonText;

        [Header("Tutorial Dialogue")]
        [SerializeField] private DialogueManager tutorialDialogue;
        [TextArea(2,4)]
        [SerializeField] private string[] tutorialLines;

        [Header("Button Text")]
        [SerializeField] private string takePartyText      = "Recruit Party";
        [SerializeField] private string startAdventureText = "Start Adventure";

        [Header("Confirmation Dialogue")]
        [SerializeField] private DialogueManager confirmDialogue;
        [TextArea(2,4)]
        [SerializeField] private string[] confirmLines;

        [Header("Preview Anchors (world transforms)")]
        [SerializeField] private RectTransform[] previewAnchors = new RectTransform[3];

        private const string MAP_SCENE_NAME = "Game_Map";
        private Coroutine positionRefreshRoutine;
        private readonly List<GameObject> recruitPrefabs = new List<GameObject>();
        private bool poolBuilt  = false;
        private bool partyTaken = false;

        // Store confirmed units and original state for potential reset
        private readonly List<(UnitObject unit, Vector3 originalPos)> confirmedUnits = new();

        // Cache for button parent
        private Transform takePartyButtonOriginalParent;

        private void Awake()
        {
            instance = this;

            InitialiseTakePartyButton();

            if (Application.isPlaying)
            {
                gameObject.SetActive(false);
            }
        }

        private void InitialiseTakePartyButton()
        {
            takePartyButton.gameObject.SetActive(false);
            
            if (takePartyButton == null)
            {
                Debug.LogError("RecruitController: Take Party button reference missing.");
                return;
            }

            takePartyButton.onClick.RemoveAllListeners();
            takePartyButton.onClick.AddListener(OnTakeParty);
            takePartyButton.gameObject.SetActive(false);

            if (takePartyButtonText == null)
            {
                takePartyButtonText = takePartyButton.GetComponentInChildren<TextMeshProUGUI>();
            }

            if (takePartyButtonText != null)
            {
                takePartyButtonText.text = takePartyText;
            }
            else
            {
                Debug.LogError("RecruitController: No TextMeshProUGUI found on Take Party button.");
            }
        }

        private void OnEnable()
        {
            if (!poolBuilt)
            {
                BuildRecruitPool();
                UnitSlot.InitialisePool(recruitPrefabs);
                poolBuilt = true;
            }

            // Build slots then disable buttons so slot.Init() doesn't re-enable them
            InitialiseUnitSlots();

            // Disable interactive buttons until tutorial completes
            SetRecruitInteractable(false);

            // Refresh preview positions at end of frame 
            positionRefreshRoutine = StartCoroutine(RefreshPositionsEndOfFrame());

            if (tutorialDialogue != null && tutorialLines.Length > 0)
            {
                // Disable button until tutorial finishes
                if (takePartyButton != null)
                {
                    takePartyButton.gameObject.SetActive(false);
                    Global.DEBUG_PRINT("[RecruitController] Button hidden during initial NPC dialogue");
                }

                if (!tutorialDialogue.gameObject.activeSelf) tutorialDialogue.gameObject.SetActive(true);
                tutorialDialogue.StartDialogue(tutorialLines, () =>
                {
                    ShowTakePartyButton();
                    SetRecruitInteractable(true);
                    Global.DEBUG_PRINT("[RecruitController] Initial dialogue finished - Take Party button enabled");
                });
            }
            else
            {
                ShowTakePartyButton();
                SetRecruitInteractable(true);
            }
        }

        private void OnDisable()
        {
            CleanupSlotPreviews();

            // Stop any running coroutine to avoid leaks
            if (positionRefreshRoutine != null)
            {
                StopCoroutine(positionRefreshRoutine);
                positionRefreshRoutine = null;
            }
        }

        private void OnDestroy()
        {

        }

        private void BuildRecruitPool()
        {
            Sprite fallbackSprite = ResourceManager.instance.GetDefaultEffectSprite();
            foreach (GameObject prefab in ResourceManager.instance.GetAllUnitPrefabs())
            {
                if (prefab == null || recruitPrefabs.Contains(prefab))
                {
                    continue;
                }

                UnitObject uo = prefab.GetComponent<UnitObject>();
                if (uo == null) {
                    continue;
                }

                // Filter by tier, only tier 1 and 2
                if (uo.unitSO != null && uo.unitSO.eTier == eUnitTier.STAR_3) {
                    continue; 
                }

                Sprite icon = GetIconFromPrefab(prefab) ?? fallbackSprite;
                if (icon != null)
                {
                    recruitPrefabs.Add(prefab);
                }
            }

            if (recruitPrefabs.Count == 0)
            {
                Debug.LogError("RecruitController: No valid unit prefabs found – recruit pool empty.");
            }
        }

        private static Sprite GetIconFromPrefab(GameObject prefab)
        {
            SpriteRenderer sr = prefab.GetComponentInChildren<SpriteRenderer>(true);
            return sr != null ? sr.sprite : null;
        }

        private void InitialiseUnitSlots()
        {
            Canvas.ForceUpdateCanvases();

            for (int i = 0; i < unitSlots.Length; i++)
            {
                UnitSlot slot = unitSlots[i];
                if (slot == null)
                {
                    continue;
                }

                if (i < previewAnchors.Length && previewAnchors[i] != null)
                {
                    slot.SetAnchor(previewAnchors[i]);
                }

                slot.Init();
                slot.RefreshPreviewPosition();
            }
        }

        private IEnumerator RefreshPositionsEndOfFrame()
        {
            yield return new WaitForEndOfFrame();

            foreach (UnitSlot slot in unitSlots)
            {
                if (slot != null)
                {
                    slot.RefreshPreviewPosition();
                }
            }
        }

        private void ShowTakePartyButton()
        {
            Global.DEBUG_PRINT("[RecruitController] ShowTakePartyButton called");
            if (takePartyButton == null)
            {
                return;
            }

            takePartyButton.gameObject.SetActive(true);
            partyTaken = false;

            if (takePartyButtonText != null)
            {
                takePartyButtonText.text = takePartyText;
            }
            Global.DEBUG_PRINT($"[RecruitController] Button active: {takePartyButton.gameObject.activeSelf}, text: {takePartyButtonText?.text}");
        }

        // Shows the button as "Start Adventure" and makes it interactable
        private void ShowStartAdventureButton()
        {
            if (takePartyButton == null) return;

            takePartyButton.gameObject.SetActive(true);
            if (takePartyButtonText != null)
            {
                takePartyButtonText.text = startAdventureText;
            }
            takePartyButton.interactable = true;

            Global.DEBUG_PRINT("[RecruitController] Start Adventure button now ACTIVE and interactable");
        }

        private void OnTakeParty()
        {
            if (!partyTaken)
            {
                ProcessPartySelection();
                partyTaken = true;
            }
            else
            {
                // Re-enable the UI, then fade out and load map scene
                EnableHudForPlayerDeck();
                if (UIFade.instance != null)
                {
                    UIFade.instance.FadeOut(1.5f);
                    UIFade.instance.SetOnFadeFinish(() => {
                        SceneManager.LoadScene(MAP_SCENE_NAME);
                        UIFade.instance.FadeIn(1.5f);
                    });
                }
                else
                {
                    SceneManager.LoadScene(MAP_SCENE_NAME);
                }
            }
        }

        private void EnableHudForPlayerDeck()
        {
            Deck deck = DeckManager.instance.GetDeckByType(eDeckType.PLAYER);
            if (deck == null) return;
            foreach (UnitObject u in deck)
            {
                ShowUnitHud(u);
            }
        }

        private static void ShowUnitHud(UnitObject unit)
        {
            if (unit == null) return;
            Transform hb = unit.transform.Find("UIHealthBar(Clone)");
            if (hb != null) hb.gameObject.SetActive(true);
            Transform effectGrid = unit.transform.Find("UIEffectGrid(Clone)");
            if (effectGrid != null) effectGrid.gameObject.SetActive(true);
            Transform shieldGrid = unit.transform.Find("Shield(Clone)");
            if (shieldGrid != null) shieldGrid.gameObject.SetActive(true);
        }

        private void ProcessPartySelection()
        {
            // Ensure new deck
            Deck playerDeck = DeckManager.instance.GetDeckByType(eDeckType.PLAYER);
            playerDeck?.DestroyAllUnit();

            int addedCount = 0;

            foreach (UnitSlot slot in unitSlots)
            {
                if (slot == null) continue;

                string unitName = ResolveUnitName(slot.CurrentPrefab);
                if (string.IsNullOrEmpty(unitName))    
                {
                    continue;
                }

                UnitObject u = DeckManager.instance.AddUnit(eDeckType.PLAYER, unitName);
                if (u == null)
                {
                    continue;
                }

                HideUnitHud(u);
                addedCount++;
                slot.DestroyPreview();
                Global.DEBUG_PRINT($"[Recruit] Added {unitName}");
            }

            Global.DEBUG_PRINT($"[Recruit] Total recruited: {addedCount}");

            DisableRerollButtons();
            if (takePartyButton != null) takePartyButton.gameObject.SetActive(false);

            ShowStartAdventureButton();
            gameObject.SetActive(false);
        }

        private static void HideUnitHud(UnitObject unit)
        {
            if (unit == null)
            {
                return;
            }

            Transform hb = unit.transform.Find("UIHealthBar(Clone)");
            if (hb != null)
            {
                hb.gameObject.SetActive(false);
            }
        }

        private void DisableRerollButtons()
        {
            foreach (UnitSlot slot in unitSlots)
            {
                if (slot == null)
                {
                    continue;
                }

                Button rerollButton = slot.GetComponentInChildren<Button>();
                if (rerollButton != null)
                {
                    rerollButton.interactable = false;
                }
            }
        }

        private void UpdateTakePartyButtonText()
        {
            if (takePartyButtonText != null)
            {
                takePartyButtonText.text = startAdventureText;
            }
        }

        private void SetRecruitInteractable(bool state)
        {
            if (takePartyButton != null) takePartyButton.interactable = state;
            foreach (var slot in unitSlots)
            {
                if (slot == null) continue;
                Button b = slot.GetComponentInChildren<Button>(true);  
                if (b != null) b.interactable = state;
            }
        }

        private string ResolveUnitName(GameObject prefab)
        {
            if (prefab == null) return null;

            UnitObject uObj = prefab.GetComponent<UnitObject>();
            string key = uObj?.unitSO?.unitName;

            if (string.IsNullOrEmpty(key))
                key = prefab.name;

            if (ResourceManager.instance.GetUnit(key) != null)
                return key;

            return FindUnitNameIgnoreCase(key);
        }

        private string FindUnitNameIgnoreCase(string original)
        {
            if (string.IsNullOrEmpty(original)) return null;
            foreach (GameObject prefab in ResourceManager.instance.GetAllUnitPrefabs())
            {
                if (prefab == null) continue;
                if (prefab.name.Equals(original, System.StringComparison.OrdinalIgnoreCase))
                    return prefab.name;
            }
            return null;
        }

        private void CleanupSlotPreviews()
        {
            foreach (UnitSlot slot in unitSlots)
            {
                if (slot == null) continue;
                slot.DestroyPreview();
            }
        }
    }
} 