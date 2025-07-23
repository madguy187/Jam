using UnityEngine;
using TMPro;

public class UnitSettingLayout : MonoBehaviour {
    [Header("UI References")]
    public TMP_Text goldText;
    public Transform teamUnitContainer;
    public Transform relicContainer;
    public Transform bagContainer;

    [Header("Prefabs")]
    public GameObject unitSlotPrefab;
    public GameObject unitButtonPrefab;
    public GameObject relicSlotPrefab;
    public GameObject relicButtonPrefab;
    public GameObject itemSlotPrefab;
    public GameObject itemButtonPrefab;

    [Header("Detail Panel")]
    public UnitDetailsPanel unitDetailsPanel;

    private MockPlayerInventory inventory;
    private MockUnit activeUnit;
    private ItemTracker tracker;
    private bool hasInitialized = false;

    public void Init(MockPlayerInventory playerInventory)
    {
        tracker = ItemTracker.Instance;
        if (tracker == null) {
            Global.DEBUG_PRINT("[UnitSettingsLayout::Init] ItemTracker is null.");
        }

        inventory = playerInventory;
        gameObject.SetActive(true);

        if (!hasInitialized) {
            SetupDropZones();
            GenerateFixedTeamSlots();
            GenerateFixedUnitRelicSlots();
            GenerateFixedBagSlots();
            hasInitialized = true;
        }
        
        RefreshUI();
        relicContainer.gameObject.SetActive(false);
        Debug.Log("[UnitSettingsLayout::Init] UnitSettingLayout initialized with player inventory.");
    }

    void RefreshUI()
    {
        goldText.text = $"Gold: {inventory.gold}";
        // Always clear tracker counts before repopulating
        tracker.ClearItems();
        ClearTeamUnitButtonsOnly();
        PopulateTeamUnits();
        ClearBagItemsOnly();
        PopulateBagItems();
        ClearUnitRelicsOnly();
        if (activeUnit != null) {
            PopulateUnitRelics(activeUnit);
        } else {
            relicContainer.gameObject.SetActive(false);
        }
    }

    public void RefreshRelicUI()
    {
        ClearUnitRelicsOnly();
        if (activeUnit != null) {
            PopulateUnitRelics(activeUnit);
        }
    }


    void ShowDetails(MockUnit unit)
    {
        unitDetailsPanel.Show(unit);
        activeUnit = unit;
        relicContainer.gameObject.SetActive(true);
        RefreshRelicUI();
    }

    public void CloseLayout()
    {
        ClearUnitRelicsOnly();
        ClearTeamUnitButtonsOnly();
        ClearBagItemsOnly();
        activeUnit = null;
        relicContainer.gameObject.SetActive(false);
        gameObject.SetActive(false);
    }

    public void SetupDropZones() {
        // TeamUnitContainer only accepts units
        DropZoneSetup.AddDropZone(teamUnitContainer.gameObject, DropZone.AllowedItemType.UnitsOnly);

        // RelicContainer only accepts relics
        DropZoneSetup.AddDropZone(relicContainer.gameObject, DropZone.AllowedItemType.RelicsOnly);

        // BagContainer accepts both units and relics
        DropZoneSetup.AddDropZone(bagContainer.gameObject, DropZone.AllowedItemType.UnitsAndRelics);
    }

    private void GenerateFixedTeamSlots() {
        // Clear old slots
        foreach (Transform child in teamUnitContainer) {
            Destroy(child.gameObject);
        }

        // Get ItemTracker from teamUnitContainer or its children
        for (int i = 0; i < tracker.maxUnits; i++) {
            Instantiate(unitSlotPrefab, teamUnitContainer);
        }
    }

    void ClearTeamUnitButtonsOnly() {
        foreach (Transform slot in teamUnitContainer) {
            if (slot.childCount > 0) {
                // Destroy any UnitButton children inside the slot
                foreach (Transform child in slot) {
                    Destroy(child.gameObject);
                }
            }
        }
    }

    void PopulateTeamUnits() {
        int unitIndex = 0;

        foreach (Transform slot in teamUnitContainer) {
            // Safety check: stop if no more units to assign
            if (unitIndex >= inventory.teamUnits.Count)
                break;

            // Check if slot is empty
            if (slot.childCount == 0) {
                var unit = inventory.teamUnits[unitIndex];

                GameObject unitGO = Instantiate(unitButtonPrefab, slot);
                var unitItem = new MockInventoryItem(unit);
                unitGO.GetComponent<UnitButton>().Init(unit, unitItem, ShowDetails);
                unitGO.GetComponent<DragHandler>().Init(unitItem);
                tracker.AddItem(TrackerType.UnitContainer, MockItemType.Unit);

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

    void ClearBagItemsOnly() {
        foreach (Transform slot in bagContainer) {
            foreach (Transform child in slot) {
                Destroy(child.gameObject); // Remove the item inside the slot
            }
        }
    }

    private void GenerateFixedBagSlots() {
        for (int i = 0; i < tracker.maxItems; i++) {
            Instantiate(itemSlotPrefab, bagContainer);
        }

        Global.DEBUG_PRINT($"[UnitSettingsLayout::GenerateFixedBagSlots] BagContainer has {tracker.maxItems} slots");
    }

    private void PopulateBagItems() {
        int bagItemIndex = 0;

        foreach (Transform slot in bagContainer) {
            if (bagItemIndex >= inventory.bagItems.Count)
                break;

            if (slot.childCount > 0)
                continue;

            var item = inventory.bagItems[bagItemIndex];
            GameObject go;

            if (item.itemType == MockItemType.Unit) {
                go = Instantiate(unitButtonPrefab, slot);
                var unitBtn = go.GetComponent<UnitButton>();
                unitBtn.Init(item.unitData, item, ShowDetails);
                unitBtn.boundItem = item;
                unitBtn.GetComponent<DragHandler>().Init(item);
            } else // Relic
              {
                go = Instantiate(itemButtonPrefab, slot);
                go.GetComponent<RelicButton>().Init(item.relicData, item);
                go.GetComponent<DragHandler>().Init(item);
            }
            ItemTracker.Instance.AddItem(TrackerType.BagContainer, item.itemType);

            // Stretch to fit slot
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            bagItemIndex++;
        }
    }

    void ClearUnitRelicsOnly() {
        foreach (Transform slot in relicContainer) {
            foreach (Transform child in slot) {
                Destroy(child.gameObject);
            }
        }
    }

    private void GenerateFixedUnitRelicSlots() {
        for (int i = 0; i < tracker.maxRelics / tracker.maxUnits; i++) {
            Instantiate(relicSlotPrefab, relicContainer);
        }
    }

    void PopulateUnitRelics(MockUnit unit)
    {
        for (int i = 0; i < relicContainer.childCount; i++) {
            var slot = relicContainer.GetChild(i);
            if (i < unit.equippedRelics.Count) {
                var relic = unit.equippedRelics[i];
                GameObject go = Instantiate(relicButtonPrefab, slot);
                var relicItem = new MockInventoryItem(relic);
                go.GetComponent<RelicButton>().Init(relic, relicItem);
                go.GetComponent<DragHandler>().Init(relicItem);
                ItemTracker.Instance.AddItem(TrackerType.RelicContainer, MockItemType.Relic);
                // Stretch to fit
                RectTransform rt = go.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
            }
        }
    }

    public MockUnit ActiveUnit => activeUnit;
}
