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
        [SerializeField] private string takePartyText      = "Take Party";
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

                if (prefab.GetComponent<UnitObject>() == null)
                {
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
                SceneManager.LoadScene(MAP_SCENE_NAME);
            }
        }

        private void ProcessPartySelection()
        {
            var addedUnits = new List<UnitObject>();
            foreach (UnitSlot slot in unitSlots)
            {
                if (slot == null || slot.CurrentPrefab == null)
                {
                    continue;
                }

                // Get the unit name from the prefab
                UnitObject unitObj = slot.CurrentPrefab.GetComponent<UnitObject>();
                string unitName = unitObj?.unitSO?.unitName;
                if (string.IsNullOrEmpty(unitName))
                {
                    unitName = slot.CurrentPrefab.name;
                }

                UnitObject u = DeckManager.instance.AddUnit(eDeckType.PLAYER, unitName);
                if (u != null)
                {
                    HideUnitHud(u);
                    addedUnits.Add(u);
                }
                slot.DestroyPreview();
            }

            Global.DEBUG_PRINT($"RecruitController: Added {addedUnits.Count} units to deck.");

            DisableRerollButtons();

            // Keep button visible but non-interactable during sequence
            if (takePartyButton != null) {
                takePartyButton.interactable = false;
            }

            // Hide button during confirmation sequence
            if (takePartyButton != null)
            {
                takePartyButton.gameObject.SetActive(false);
                Global.DEBUG_PRINT("[RecruitController] Take Party button hidden for confirmation sequence");
            }

            ShowStartAdventureButton();
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

            Transform eg = unit.transform.Find("UIEffectGrid(Clone)");
            if (eg != null)
            {
                eg.gameObject.SetActive(false);
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