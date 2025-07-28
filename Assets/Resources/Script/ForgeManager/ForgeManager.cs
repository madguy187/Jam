using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum ForgeSlotID { Slot1, Slot2 }

public class ForgeManager : MonoBehaviour
{
    public static ForgeManager Instance { get; private set; }

    [Header("Bag Panel")]
    [SerializeField] private Transform bagContainer;         // GridLayoutGroup parent for bag
    [SerializeField] private GameObject bagSlotPrefab;       // UI prefab that displays a relic
    [SerializeField] private GameObject relicItemPrefab;       // UI prefab that displays a relic

    [Header("Forge Panel")]
    [SerializeField] private Image relicSlot1;
    [SerializeField] private Image relicSlot2;
    [SerializeField] private Image resultSlot;

    [Header("Buttons")]
    [SerializeField] private Button mergeButton;
    [SerializeField] private Button breakButton;
    [SerializeField] private Button forgeButton;

    [Header("Combiner")]
    public RelicCombiner relicCombiner;

    [Header("Test Relics")]
    public List<RelicScriptableObject> testRelics;

    private RelicScriptableObject selectedRelic1;
    private RelicScriptableObject selectedRelic2;
    private RelicScriptableObject forgedResult;

    private void Awake()
    {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        mergeButton.onClick.AddListener(() => Debug.Log("Merge Mode Selected")); // Placeholder for toggle logic
        breakButton.onClick.AddListener(() => Debug.Log("Break Mode Selected"));
        forgeButton.onClick.AddListener(DoForge);
        ClearBagItemsOnly();
        GenerateFixedBagSlots(12);
        PopulateBagItems(testRelics);
    }

    // =============================
    // Bag Panel Setup
    // =============================

    public void ClearBagItemsOnly()
    {
        foreach (Transform slot in bagContainer) {
            foreach (Transform child in slot) {
                Destroy(child.gameObject); // Remove the item inside the slot
            }
        }
    }

    public void GenerateFixedBagSlots(int count)
    {
        // Clear old slots
        foreach (Transform child in bagContainer) {
            Destroy(child.gameObject);
        }
        // Create empty slots
        for (int i = 0; i < count; i++) {
            Instantiate(bagSlotPrefab, bagContainer);
        }
    }

    public void PopulateBagItems(List<RelicScriptableObject> relics)
    {
        for (int i = 0; i < relics.Count && i < bagContainer.childCount; i++) {
            var slot = bagContainer.GetChild(i);
            var relicData = relics[i];
            if (relicData == null) { continue; }
            GameObject relicGO = Instantiate(relicItemPrefab, slot);

            var image = relicGO.transform.Find("RelicImage").GetComponent<Image>();
            var label = relicGO.transform.Find("RelicNameText").GetComponent<TMP_Text>();
            var btn = relicGO.GetComponent<Button>();
            image.sprite = relicData.GetRelicSprite();
            label.text = relicData.GetRelicName();
            relicGO.GetComponent<DraggableRelic>().relicData = relicData;
    
            // Optional: stretch to fit
            RectTransform rt = relicGO.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }

    // =============================
    // Forge Logic
    // =============================
    private void TryUpdateForgeResult()
    {
        if (selectedRelic1 != null && selectedRelic2 != null) {
            RelicScriptableObject result;
            if (relicCombiner.TryCombine(selectedRelic1, selectedRelic2, out result)) {
                resultSlot.sprite = result.GetRelicSprite();
                resultSlot.color = Color.white;
                Global.DEBUG_PRINT($"[ForgeManager::TryUpdateForgeResult] Forge Result: {result.GetRelicName()}");
            } else {
                resultSlot.sprite = null;
                resultSlot.color = new Color(1, 1, 1, 0); // hide
            }
        }
    }

    private void DoForge()
    {
        if (selectedRelic1 != null && selectedRelic2 != null)
        {
            if (relicCombiner.TryCombine(selectedRelic1, selectedRelic2, out var forgedResult))
            {
                resultSlot.sprite = forgedResult.GetRelicSprite();
                resultSlot.color = Color.white;
                Debug.Log($"Forged: {forgedResult.GetRelicName()}");

                // Add forgedResult to testRelics list
                testRelics.Add(forgedResult);

                // Remove relic item GameObjects from the forge slots (destroy the children)
                if (relicSlot1.transform.childCount > 0)
                {
                    var go = relicSlot1.transform.GetChild(0).gameObject;
                    testRelics.Remove(go.GetComponent<DraggableRelic>().relicData);
                    Destroy(go);
                }
                if (relicSlot2.transform.childCount > 0)
                {
                    var go = relicSlot2.transform.GetChild(0).gameObject;
                    testRelics.Remove(go.GetComponent<DraggableRelic>().relicData);
                    Destroy(go);
                }

                // Clear selected relic references
                selectedRelic1 = null;
                selectedRelic2 = null;

                // Clear forge slot images and hide them
                relicSlot1.sprite = null;
                relicSlot1.color = new Color(1, 1, 1, 0);
                relicSlot2.sprite = null;
                relicSlot2.color = new Color(1, 1, 1, 0);

                // Clear result slot after a short delay or keep showing (optional)
                // resultSlot.sprite = null;
                // resultSlot.color = new Color(1, 1, 1, 0);

                // Refresh the bag display to show the new forged relic
                PopulateBagItems(testRelics);
            }
            else
            {
                Debug.Log("Invalid combination.");
                resultSlot.color = new Color(1, 1, 1, 0);
            }
        }
        else
        {
            Debug.Log("Select two relics first.");
        }
    }

    public void ClearForgeSlots()
    {
        selectedRelic1 = null;
        selectedRelic2 = null;
        relicSlot1.sprite = null;
        relicSlot2.sprite = null;
        relicSlot1.color = new Color(1, 1, 1, 0);
        relicSlot2.color = new Color(1, 1, 1, 0);
        resultSlot.sprite = null;
        resultSlot.color = new Color(1, 1, 1, 0);
    }

    // =============================
    // Drag-Drop Slot Assignment (Snap UI to slot)
    // =============================
    public void AssignRelicToSlot(RelicScriptableObject relic, ForgeSlotID slot, GameObject draggedObject)
    {
        if (slot == ForgeSlotID.Slot1)
        {
            selectedRelic1 = relic;
            // Move the dragged object into the slot
            draggedObject.transform.SetParent(relicSlot1.transform);
        }
        else if (slot == ForgeSlotID.Slot2)
        {
            selectedRelic2 = relic;
            // Move the dragged object into the slot
            draggedObject.transform.SetParent(relicSlot2.transform);
        }

        draggedObject.transform.localPosition = Vector3.zero;
        // Stretch to fit
        RectTransform rt = draggedObject.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        Transform originalSlot = draggedObject.GetComponent<DraggableRelic>()?.originalSlot;
        if (originalSlot != null)
            Destroy(originalSlot.gameObject);

        TryUpdateForgeResult();
    }
}
