using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace StoryManager
{
    public class RecruitController : MonoBehaviour
    {
        public static RecruitController instance { get; private set; }

        [Header("UI References")]
        [SerializeField] private UnitSlot[] unitSlots = new UnitSlot[3];
        [SerializeField] private Button     takePartyButton;
        [SerializeField] private TextMeshProUGUI takePartyButtonText;

        [Header("Hint Text Fade")]
        [SerializeField] private TextMeshProUGUI hintText;
        [SerializeField] private float hintFadeDuration = 1f;

        [Header("Button Text")]
        [SerializeField] private string takePartyText      = "Take Party";
        [SerializeField] private string startAdventureText = "Start Adventure";

        [Header("Preview Anchors (world transforms)")]
        [SerializeField] private RectTransform[] previewAnchors = new RectTransform[3];

        [Header("Reroll Cost Settings")]
        [SerializeField] private int freeRerollsPerSlot = 3;

        private readonly List<GameObject> recruitPrefabs = new List<GameObject>();
        private bool poolBuilt  = false;
        private bool partyTaken = false;

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

        /* ----------------------------- Life-cycle ------------------------*/
        private void OnEnable()
        {
            if (!poolBuilt)
            {
                BuildRecruitPool();
                UnitSlot.InitialisePool(recruitPrefabs);
                poolBuilt = true;
            }

            ShowTakePartyButton();
            InitialiseUnitSlots();
            StartCoroutine(RefreshPositionsEndOfFrame());
        }

        private void OnDisable()
        {
            foreach (UnitSlot slot in unitSlots)
            {
                if (slot != null)
                {
                    slot.DestroyPreview();
                }
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
            GameObject temp = Instantiate(prefab);
            SpriteRenderer spriteRenderer = temp.GetComponentInChildren<SpriteRenderer>();
            Sprite icon = spriteRenderer != null ? spriteRenderer.sprite : null;
            DestroyImmediate(temp);
            return icon;
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
                Debug.Log("RecruitController: proceed to next scene.");
            }
        }

        private void ProcessPartySelection()
        {
            Deck playerDeck = DeckManager.instance.GetDeckByType(eDeckType.PLAYER);
            if (playerDeck == null)
            {
                Debug.LogError("RecruitController: Player deck missing.");
                return;
            }

            int addedCount = 0;
            foreach (UnitSlot slot in unitSlots)
            {
                if (slot == null)
                {
                    continue;
                }

                GameObject prefab = slot.CurrentPrefab;
                if (prefab == null)
                {
                    continue;
                }

                UnitObject unit = playerDeck.AddUnit(prefab);
                if (unit != null)
                {
                    unit.transform.position += Vector3.down; 
                    HideUnitHud(unit);
                    addedCount++;
                }
            }

            Debug.Log($"RecruitController: Added {addedCount} units to deck.");

            DisableRerollButtons();
            UpdateTakePartyButtonText();
            StartCoroutine(FadeAndDisableHint());
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

        private IEnumerator FadeAndDisableHint()
        {
            if (hintText == null)
            {
                yield break;
            }

            float elapsed = 0f;
            Color startColor = hintText.color;
            Color endColor   = new Color(startColor.r, startColor.g, startColor.b, 0f);

            while (elapsed < hintFadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / hintFadeDuration);
                hintText.color = Color.Lerp(startColor, endColor, t);
                yield return null;
            }

            hintText.gameObject.SetActive(false);
        }
    }
} 