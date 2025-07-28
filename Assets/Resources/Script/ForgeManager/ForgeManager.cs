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
    [SerializeField] private GameObject relicItemPrefab;     // UI prefab that displays a relic
    public int bagSize = 100;  // Drop slot for bag items

    [Header("UI Panels")]
    public TMP_Text goldText;
    [SerializeField] private GameObject forgeArea;
    [SerializeField] private GameObject breakArea;
    [SerializeField] private Sprite questionMarkSprite;

    [Header("Dialogue System")]
    public DialogueManager dialogueManager;
    [TextArea(2, 4)]
    private string[] dialogueLines;

    [Header("Buttons")]
    [SerializeField] private Button mergeButton;
    [SerializeField] private Button breakButton;

    [Header("Combiner")]
    public RelicCombiner relicCombiner;

    [Header("Test Relics")]
    [SerializeField] public List<RelicScriptableObject> playerRelics;

    private Image relicSlot1;
    private Image relicSlot2;
    private Image resultSlot;
    private Button forgeButton;

    private Image breakInputSlot;
    private Image breakResult1Slot;
    private Image breakResult2Slot;
    private Button breakRelicButton;

    private bool isBreakMode = false;
    private RelicScriptableObject selectedRelic1;
    private RelicScriptableObject selectedRelic2;
    private RelicScriptableObject forgedResult;
    [SerializeField] private Transform resultSlotContainer;

    private int currentGold = 100;
    private ItemTracker tracker;

    private void Awake()
    {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
        }

        InitMergeUI();
        InitBreakUI();
    }

    private void Start()
    {
        if (MockPlayerInventoryHolder.Instance == null) {
            Global.DEBUG_PRINT("[ForgeManager::Start] MockPlayerInventoryHolder instance is null!");
        } else {
            currentGold = MockPlayerInventoryHolder.Instance.playerInventory.gold;
        }
        if (ItemTracker.Instance == null) {
            Global.DEBUG_PRINT("[ForgeManager::Start] ItemTracker instance is null!");
        } else {
            tracker = ItemTracker.Instance;
        }
        RefreshGoldUI();
        mergeButton.onClick.AddListener(() => SetBreakMode(false));
        breakButton.onClick.AddListener(() => SetBreakMode(true));
        forgeButton.onClick.AddListener(DoForgeOrBreak);
        breakRelicButton.onClick.AddListener(DoForgeOrBreak);

        ClearBagItemsOnly();
        GenerateFixedBagSlots(bagSize);

        if (MockPlayerInventoryHolder.Instance != null)
        {
            List<RelicScriptableObject> realRelics = new List<RelicScriptableObject>();
            foreach (MockInventoryItem item in MockPlayerInventoryHolder.Instance.playerInventory.bagItems)
            {
                if (item.itemType == MockItemType.Relic && item.relicData != null)
                {
                    realRelics.Add(item.relicData);
                }
            }
            playerRelics = realRelics;
        }
        PopulateBagItems(playerRelics);

        forgeButton.gameObject.SetActive(false);
        SetBreakMode(false); // default to merge mode

        dialogueLines = new string[]
        {
            "Ho ho! You found some relics!",
            "Click Merge at the top left to merge relics.",
            "Click Break at the top left to break relics.",
            "Each action uses gold so plan accordingly.",
            "Either a relic or a question mark appears as the result.",
            "Relic means your combination works, question mark means not.",
            "Press the Forge or Break button below to add it to your bag!",
            "Now, drag your relics from your bag on the right and into the slots and watch the magic happen!",
        };

        dialogueManager.StartDialogue(dialogueLines, () =>
        {
            // claimButton.interactable = true; // lazy to disable interaction for now
        });
    }

    void RefreshGoldUI()
    {
        goldText.text = $"Gold: {currentGold}";
    }

    void UpdatePlayerGold()
    {
        if (MockPlayerInventoryHolder.Instance == null) {
            Global.DEBUG_PRINT("[ForgeManager::UpdatePlayerGold] MockPlayerInventoryHolder instance is null!");
        } else {
            MockPlayerInventoryHolder.Instance.playerInventory.gold = currentGold;
        }
    }

    private void InitMergeUI()
    {
        if (forgeArea == null)
        {
            Debug.LogError("ForgeArea not assigned in ForgeManager!");
            return;
        }

        relicSlot1 = forgeArea.transform.Find("RelicSlot1")?.GetComponent<Image>();
        relicSlot2 = forgeArea.transform.Find("RelicSlot2")?.GetComponent<Image>();
        resultSlot = forgeArea.transform.Find("ResultSlot")?.GetComponent<Image>();
        forgeButton = forgeArea.transform.Find("ForgeButton")?.GetComponent<Button>();

        if (relicSlot1 == null || relicSlot2 == null || resultSlot == null)
        {
            Debug.LogError("One or more forge slots are missing under ForgeArea!");
        }
    }

    private void InitBreakUI()
    {
        if (breakArea == null)
        {
            Debug.LogError("BreakArea not assigned in ForgeManager!");
            return;
        }
        
        breakInputSlot = breakArea.transform.Find("RelicSlot")?.GetComponent<Image>();
        breakResult1Slot = breakArea.transform.Find("ResultSlot1")?.GetComponent<Image>();
        breakResult2Slot = breakArea.transform.Find("ResultSlot2")?.GetComponent<Image>();
        breakRelicButton = breakArea.transform.Find("BreakButton")?.GetComponent<Button>();

        if (breakInputSlot == null || breakResult1Slot == null || breakResult2Slot == null)
        {
            Debug.LogError("One or more break slots are missing under BreakArea!");
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
        foreach (Transform child in bagContainer) {
            Destroy(child.gameObject);
        }
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
            image.sprite = relicData.GetRelicSprite();
            label.text = relicData.GetRelicName();
            relicGO.GetComponent<DraggableRelic>().relicData = relicData;

            RectTransform rt = relicGO.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }

    // =============================
    // Mode toggle
    // =============================
    private void SetBreakMode(bool enabled)
    {
        isBreakMode = enabled;

        // Toggle UI panel visibility
        forgeArea.SetActive(!enabled);
        breakArea.SetActive(enabled);

        // Toggle button interactables
        mergeButton.interactable = isBreakMode;
        breakButton.interactable = !isBreakMode;

        // Clear UI state
        ReturnAllSlotItemsToBag();
        ClearResultSlot();

        selectedRelic1 = null;
        selectedRelic2 = null;

        breakRelicButton.gameObject.SetActive(false);
        forgeButton.gameObject.SetActive(false);
    }

    private void SetQuestionMark(Image slot)
    {
        if (slot == null) return;

        foreach (Transform child in slot.transform)
            Destroy(child.gameObject);

        GameObject relicGO = Instantiate(relicItemPrefab, slot.transform);

        var image = relicGO.transform.Find("RelicImage").GetComponent<Image>();
        var label = relicGO.transform.Find("RelicNameText").GetComponent<TMP_Text>();

        image.sprite = questionMarkSprite;
        label.text = "?";

        RectTransform rt = relicGO.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    // =============================
    // Forge Logic
    // =============================
    private void TryUpdateForgeResult()
    {
        ClearResultSlot();

        if (!isBreakMode)
        {
            // Merge mode
            if (selectedRelic1 != null && selectedRelic2 != null)
            {
                if (relicCombiner.TryCombine(selectedRelic1, selectedRelic2, out var result))
                {
                    InstantiateResultRelic(result, resultSlotContainer);

                    int cost = relicCombiner.GetCombineCost(selectedRelic1, selectedRelic2);
                    forgeButton.GetComponentInChildren<TMP_Text>().text = $"{cost} Gold to Forge";
                    forgeButton.gameObject.SetActive(true);
                }
                else
                {
                    SetQuestionMark(resultSlot);
                    forgeButton.gameObject.SetActive(false);
                    forgeButton.GetComponentInChildren<TMP_Text>().text = "Forge";
                }
            }
            else
            {
                SetQuestionMark(resultSlot);
                forgeButton.gameObject.SetActive(false);
                forgeButton.GetComponentInChildren<TMP_Text>().text = "Forge";
            }
        }
        else
        {
            // Break mode
            if (selectedRelic1 != null)
            {
                if (relicCombiner.TryBreak(selectedRelic1, out var part1, out var part2))
                {
                    InstantiateResultRelic(part1, breakResult1Slot.transform);
                    InstantiateResultRelic(part2, breakResult2Slot.transform);

                    int breakCost = relicCombiner.GetBreakCost(selectedRelic1);
                    breakRelicButton.GetComponentInChildren<TMP_Text>().text = $"{breakCost} Gold to Break";
                    breakRelicButton.gameObject.SetActive(true);
                }
                else
                {
                    SetQuestionMark(breakResult1Slot);
                    SetQuestionMark(breakResult2Slot);
                    breakRelicButton.gameObject.SetActive(false);
                    breakRelicButton.GetComponentInChildren<TMP_Text>().text = "Break";
                }
            }
            else
            {
                SetQuestionMark(breakResult1Slot);
                SetQuestionMark(breakResult2Slot);
                breakRelicButton.gameObject.SetActive(false);
                breakRelicButton.GetComponentInChildren<TMP_Text>().text = "Break";
            }
        }
    }


    private void InstantiateResultRelic(RelicScriptableObject relic, Transform parent)
    {
        GameObject relicGO = Instantiate(relicItemPrefab, parent);
        var image = relicGO.transform.Find("RelicImage").GetComponent<Image>();
        var label = relicGO.transform.Find("RelicNameText").GetComponent<TMP_Text>();
        image.sprite = relic.GetRelicSprite();
        label.text = relic.GetRelicName();

        RectTransform rt = relicGO.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private void ClearResultSlot()
    {
        foreach (Transform child in resultSlotContainer)
            Destroy(child.gameObject);

        if (breakResult1Slot != null)
        {
            foreach (Transform child in breakResult1Slot.transform)
                Destroy(child.gameObject);
        }

        if (breakResult2Slot != null)
        {
            foreach (Transform child in breakResult2Slot.transform)
                Destroy(child.gameObject);
        }
    }

    private void DoForgeOrBreak()
    {
        if (!isBreakMode)
        {
            // Merge mode
            if (selectedRelic1 != null && selectedRelic2 != null)
            {
                if (relicCombiner.TryCombine(selectedRelic1, selectedRelic2, out var forgedResult))
                {
                    int cost = relicCombiner.GetCombineCost(selectedRelic1, selectedRelic2);
                    currentGold -= cost;
                    UpdatePlayerGold();
                    RefreshGoldUI();

                    playerRelics.Add(forgedResult);
                    if (tracker != null) {
                        tracker.AddItem(TrackerType.BagContainer, MockItemType.Relic, new MockInventoryItem(forgedResult));
                    }

                    // Remove relic GameObjects from forge slots, also remove relic data from list
                    RemoveRelicFromSlot(relicSlot1, selectedRelic1);
                    RemoveRelicFromSlot(relicSlot2, selectedRelic2);

                    selectedRelic1 = null;
                    selectedRelic2 = null;

                    ClearResultSlot();

                    ClearBagItemsOnly();
                    PopulateBagItems(playerRelics);
                    forgeButton.gameObject.SetActive(false);
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
        else
        {
            // Break mode
            if (selectedRelic1 != null)
            {
                if (relicCombiner.TryBreak(selectedRelic1, out var part1, out var part2))
                {
                    int cost = relicCombiner.GetBreakCost(selectedRelic1);
                    currentGold -= cost;
                    UpdatePlayerGold();
                    RefreshGoldUI();

                    playerRelics.Remove(selectedRelic1);
                    playerRelics.Add(part1);
                    playerRelics.Add(part2);
                    if (tracker != null) {
                        tracker.AddItem(TrackerType.BagContainer, MockItemType.Relic, new MockInventoryItem(part1));
                        tracker.AddItem(TrackerType.BagContainer, MockItemType.Relic, new MockInventoryItem(part2));
                        tracker.RemoveItem(TrackerType.BagContainer, MockItemType.Relic, new MockInventoryItem(selectedRelic1));
                    }

                    if (breakInputSlot.transform.childCount > 0)
                        Destroy(breakInputSlot.transform.GetChild(0).gameObject);

                    selectedRelic1 = null;

                    ClearResultSlot();
                    ClearBagItemsOnly();
                    PopulateBagItems(playerRelics);
                    breakRelicButton.gameObject.SetActive(false);
                }
                else
                {
                    Debug.Log("Cannot break this relic.");
                }
            }
            else
            {
                Debug.Log("Select a relic to break first.");
            }
        }
    }

    private void RemoveRelicFromSlot(Image slotImage, RelicScriptableObject relic)
    {
        if (slotImage.transform.childCount > 0)
        {
            var go = slotImage.transform.GetChild(0).gameObject;
            var draggable = go.GetComponent<DraggableRelic>();
            if (draggable != null && draggable.relicData == relic)
            {
                playerRelics.Remove(relic);
                if (tracker != null) {
                    tracker.RemoveItem(TrackerType.BagContainer, MockItemType.Relic, new MockInventoryItem(relic));
                }
                Destroy(go);
            }
        }
    }

    private void ClearAllForgeAndBreakSlots()
    {
        selectedRelic1 = null;
        selectedRelic2 = null;

        void ClearSlot(Image slot)
        {
            if (slot == null) return;
            foreach (Transform child in slot.transform)
            {
                Destroy(child.gameObject);
            }
        }

        ClearSlot(relicSlot1);
        ClearSlot(relicSlot2);
        ClearSlot(breakInputSlot);
    }

    private void ReturnAllSlotItemsToBag()
    {
        // Local function – try to move the child (if any) of a slot back to bag
        void MoveChildToBag(Image slot)
        {
            if (slot == null) { return; }
            if (slot.transform.childCount == 0) return;

            var go = slot.transform.GetChild(0).gameObject;
            SnapToFirstAvailableSlot(go);
        }

        // Merge‑area slots
        MoveChildToBag(relicSlot1);
        MoveChildToBag(relicSlot2);

        // Break‑area slots
        MoveChildToBag(breakInputSlot);
    }

    // =============================
    // Drag-Drop Slot Assignment
    // =============================
    public void AssignRelicToSlot(RelicScriptableObject relic, ForgeSlotID slot, GameObject draggedObject)
    {
        if (isBreakMode)
        {
            selectedRelic1 = relic;

            // Clear break input slot if already occupied
            foreach (Transform child in breakInputSlot.transform)
            {
                Destroy(child.gameObject);
            }
            draggedObject.transform.SetParent(breakInputSlot.transform);
        }
        else
        {
            if (slot == ForgeSlotID.Slot1)
            {
                selectedRelic1 = relic;
                draggedObject.transform.SetParent(relicSlot1.transform);
            }
            else if (slot == ForgeSlotID.Slot2)
            {
                selectedRelic2 = relic;
                draggedObject.transform.SetParent(relicSlot2.transform);
            }
        }

        draggedObject.transform.localPosition = Vector3.zero;
        RectTransform rt = draggedObject.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        Transform originalSlot = draggedObject.GetComponent<DraggableRelic>()?.OriginalSlot;
        if (originalSlot != null && originalSlot.childCount > 0)
        {
            Destroy(originalSlot.GetChild(0).gameObject);
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

    public bool IsBreakMode => isBreakMode;
}
