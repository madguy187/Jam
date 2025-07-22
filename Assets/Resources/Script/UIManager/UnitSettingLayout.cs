using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UnitSettingLayout : MonoBehaviour {

    [Header("UI References")]
    public TMP_Text goldText;
    public Transform teamUnitContainer;
    public Transform bagContainer;

    [Header("Prefabs")]
    public GameObject unitSlotPrefab;
    public GameObject unitButtonPrefab;
    public GameObject itemSlotPrefab;
    public GameObject itemButtonPrefab;

    [Header("Detail Panel")]
    public UnitDetailsPanel unitDetailsPanel;

    private MockPlayerInventory inventory;

    public void Init(MockPlayerInventory playerInventory)
    {
        inventory = playerInventory;
        gameObject.SetActive(true);
        SetupDropZones();
        GenerateFixedTeamSlots();
        GenerateFixedBagSlots();
        RefreshUI();
        Debug.Log("[UnitSettingsLayout::Init] UnitSettingLayout initialized with player inventory.");
    }

    void RefreshUI()
    {
        goldText.text = $"Gold: {inventory.gold}";
        ClearTeamUnitButtonsOnly();
        PopulateTeamUnits();
        ClearBagItemsOnly();
        PopulateBagItems();
    }

    void ShowDetails(MockUnit unit)
    {
        unitDetailsPanel.Show(unit);
    }

    public void CloseLayout()
    {
        gameObject.SetActive(false);
    }

    public void SetupDropZones()
    {
        // TeamUnitContainer only accepts units
        DropZoneSetup.AddDropZone(teamUnitContainer.gameObject, DropZone.AllowedItemType.UnitsOnly);

        // BagContainer accepts both units and relics
        DropZoneSetup.AddDropZone(bagContainer.gameObject, DropZone.AllowedItemType.UnitsAndRelics);
    }

    private void GenerateFixedTeamSlots()
    {
        // Clear old slots
        foreach (Transform child in teamUnitContainer) {
            Destroy(child.gameObject);
        }

        // Get ItemTracker from teamUnitContainer or its children
        var tracker = teamUnitContainer.GetComponentInChildren<ItemTracker>();
        if (tracker == null) {
            Debug.LogWarning("[UnitSettingsLayout::GenerateFixedTeamSlots] ItemTracker component not found in teamUnitContainer or its children.");
            return;
        }
        for (int i = 0; i < tracker.maxUnits; i++) {
            Instantiate(unitSlotPrefab, teamUnitContainer);
        }
    }

    void ClearTeamUnitButtonsOnly()
    {
        foreach (Transform slot in teamUnitContainer) {
            if (slot.childCount > 0) {
                // Destroy any UnitButton children inside the slot
                foreach (Transform child in slot) {
                    Destroy(child.gameObject);
                }
            }
        }
    }

    void PopulateTeamUnits()
    {
        int unitIndex = 0;

        foreach (Transform slot in teamUnitContainer) {
            // Safety check: stop if no more units to assign
            if (unitIndex >= inventory.teamUnits.Count)
                break;

            // Check if slot is empty
            if (slot.childCount == 0) {
                var unit = inventory.teamUnits[unitIndex];

                GameObject unitGO = Instantiate(unitButtonPrefab, slot);
                unitGO.GetComponent<UnitButton>().Init(unit, ShowDetails);
                // Get ItemTracker from teamUnitContainer or its children
                var tracker = teamUnitContainer.GetComponentInChildren<ItemTracker>();
                if (tracker == null) {
                    Debug.LogWarning("[UnitSettingsLayout::GenerateFixedTeamSlots] ItemTracker component not found in teamUnitContainer or its children.");
                    return;
                }
                tracker.AddItem(MockItemType.Unit);

                // Optional but recommended: make it stretch to fill slot
                RectTransform rt = unitGO.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;

                unitIndex++;
            }
        }
    }

    void ClearBagItemsOnly()
    {
        foreach (Transform slot in bagContainer)
        {
            foreach (Transform child in slot)
            {
                Destroy(child.gameObject); // Remove the item inside the slot
            }
        }
    }

    private void GenerateFixedBagSlots() {
        // Get ItemTracker from bagContainer
        var tracker = bagContainer.GetComponentInChildren<ItemTracker>();
        if (tracker == null) {
            Debug.LogWarning("[UnitSettingsLayout::GenerateFixedBagSlots] ItemTracker component not found in bagContainer or its children.");
            return;
        }

        int totalSlots = tracker.maxUnits + tracker.maxRelics;

        for (int i = 0; i < totalSlots; i++) {
            Instantiate(itemSlotPrefab, bagContainer);
        }
        
        Debug.Log($"[UnitSettingsLayout::GenerateFixedBagSlots] BagContainer has {totalSlots} slots");
    }
    
    private void PopulateBagItems()
    {
        int bagItemIndex = 0;

        foreach (Transform slot in bagContainer)
        {
            if (bagItemIndex >= inventory.bagItems.Count)
                break;

            if (slot.childCount > 0)
                continue;

            var item = inventory.bagItems[bagItemIndex];
            GameObject go;

            if (item.itemType == MockItemType.Unit)
            {
                go = Instantiate(unitButtonPrefab, slot);
                var unitBtn = go.GetComponent<UnitButton>();
                unitBtn.Init(item.unitData, item, ShowDetails);
                unitBtn.boundItem = item;
            }
            else // Relic
            {
                go = Instantiate(itemButtonPrefab, slot);
                go.GetComponent<MockRelicSlot>().Init(item.relicData, item);
            }

            // Get ItemTracker from bag container or its children
            var tracker = bagContainer.GetComponentInChildren<ItemTracker>();
            if (tracker == null) {
                Debug.LogWarning("[UnitSettingsLayout::PopulateBagItems] ItemTracker component not found in bagContainer or its children.");
                return;
            }
            tracker.AddItem(item.itemType);

            // Stretch to fit slot
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            bagItemIndex++;
        }
    }
}
