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
    [SerializeField] private Transform resultSlotContainer;

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
        forgeButton.gameObject.SetActive(false);
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
            bagContainer.GetChild(i).gameObject.AddComponent<ForgeBagDropSlot>();
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

    public bool SnapToFirstAvailableSlot(GameObject draggedGO)
    {
        foreach (Transform slot in bagContainer)
        {
            if (slot.childCount == 0)
            {
                // Snap to this slot
                draggedGO.transform.SetParent(slot, true);

                // Stretch to fit
                RectTransform rt = draggedGO.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;

                return true;
            }
        }

        Global.DEBUG_PRINT("[ForgeManager::SnapToFirstAvailableSlot] No empty slot found.");
        return false;
    }

    // =============================
    // Forge Logic
    // =============================
    private void TryUpdateForgeResult()
    {
        // Clear any previous forged relic UI in the result slot container first
        foreach (Transform child in resultSlotContainer)
        {
            Destroy(child.gameObject);
        }

        if (selectedRelic1 != null && selectedRelic2 != null)
        {
            RelicScriptableObject result;
            if (relicCombiner.TryCombine(selectedRelic1, selectedRelic2, out result))
            {
                // Instantiate the forged relic prefab inside the result slot container
                GameObject forgedRelicGO = Instantiate(relicItemPrefab, resultSlotContainer);

                var image = forgedRelicGO.transform.Find("RelicImage").GetComponent<Image>();
                var label = forgedRelicGO.transform.Find("RelicNameText").GetComponent<TMP_Text>();
                image.sprite = result.GetRelicSprite();
                label.text = result.GetRelicName();

                // Stretch to fit container
                RectTransform rt = forgedRelicGO.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;

                forgeButton.gameObject.SetActive(true);
                Global.DEBUG_PRINT($"[ForgeManager::TryUpdateForgeResult] Forge Result: {result.GetRelicName()}");
            }
        } else {
            forgeButton.gameObject.SetActive(false);
            Global.DEBUG_PRINT("[ForgeManager::TryUpdateForgeResult] Not enough relics selected for forging.");
        }
    }

    private void DoForge()
    {
        if (selectedRelic1 != null && selectedRelic2 != null)
        {
            if (relicCombiner.TryCombine(selectedRelic1, selectedRelic2, out var forgedResult))
            {
                Debug.Log($"Forged: {forgedResult.GetRelicName()}");

                // Add forgedResult to testRelics list
                testRelics.Add(forgedResult);

                // Remove relic item GameObjects from the forge slots (destroy the children)
                var go1 = relicSlot1.transform.GetChild(0).gameObject;
                testRelics.Remove(go1.GetComponent<DraggableRelic>().relicData);
                Destroy(go1);
                var go2 = relicSlot2.transform.GetChild(0).gameObject;
                testRelics.Remove(go2.GetComponent<DraggableRelic>().relicData);
                Destroy(go2);

                // Clear selected relic references
                selectedRelic1 = null;
                selectedRelic2 = null;

                // Clear the result slot container children (destroy forged relic UI)
                foreach (Transform child in resultSlotContainer)
                {
                    Destroy(child.gameObject);
                }

                // Clear forge slot images and hide them
                // relicSlot1.sprite = null;
                // relicSlot1.color = new Color(1, 1, 1, 0);
                // relicSlot2.sprite = null;
                // relicSlot2.color = new Color(1, 1, 1, 0);

                // Refresh the bag display to show the new forged relic
                ClearBagItemsOnly();
                PopulateBagItems(testRelics);
            }
            else
            {
                Debug.Log("Invalid combination.");
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
        // relicSlot1.sprite = null;
        // relicSlot2.sprite = null;
        // relicSlot1.color = new Color(1, 1, 1, 0);
        // relicSlot2.color = new Color(1, 1, 1, 0);
        // resultSlot.sprite = null;
        // resultSlot.color = new Color(1, 1, 1, 0);
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

        Transform originalSlot = draggedObject.GetComponent<DraggableRelic>()?.OriginalSlot;
        if (originalSlot != null && originalSlot.childCount > 0) {
            Destroy(originalSlot.GetChild(0).gameObject); // only destroys the item
        }

        TryUpdateForgeResult();
    }

    public void RemoveRelicFromForge(RelicScriptableObject relic)
    {
        if (selectedRelic1 == relic)
        {
            selectedRelic1 = null;
            if (relicSlot1.transform.childCount > 0)
            {
                var go = relicSlot1.transform.GetChild(0).gameObject;
                Destroy(go);
            }
        }
        else if (selectedRelic2 == relic)
        {
            selectedRelic2 = null;
            if (relicSlot2.transform.childCount > 0)
            {
                var go = relicSlot2.transform.GetChild(0).gameObject;
                Destroy(go);
            }
        }

        TryUpdateForgeResult();
    }
}
