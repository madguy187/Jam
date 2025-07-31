using UnityEngine;
using TMPro;
using Map;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

public class UnitSettingLayout : MonoBehaviour 
{
    public static UnitSettingLayout instance;

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

    [Header("Game Information")]
    public UnitDetailsPanel unitDetailsPanel;
    public MockPlayerInventory inventory;

    private UnitObject activeUnit;
    public UnitButton currentSelectedUnitButton;
    private ItemTracker tracker;
    private bool hasInitialized = false;

    public void Awake() 
    {
        if (instance != null) {
            Destroy(instance);
        }

        instance = this;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        // Example: Perform actions only for a specific scene
        if (scene.name == "Game_Necromancer") {
            OpenLayout();
        } else {
            CloseLayout();
        }
    }

    public void Init() {
        if (ItemTracker.Instance == null) {
            Global.DEBUG_PRINT("[UnitSettingsLayout::Init] ItemTracker instance is null!");
            tracker = new ItemTracker(); // Create a new instance if it doesn't exist
        } else {
            tracker = ItemTracker.Instance;
        }
        if (MockPlayerInventoryHolder.Instance == null) {
            Global.DEBUG_PRINT("[UnitSettingsLayout::Init] MockPlayerInventoryHolder instance is null!");
        } else {
            inventory = MockPlayerInventoryHolder.Instance.playerInventory;
        }
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
        Global.DEBUG_PRINT("[UnitSettingsLayout::Init] UnitSettingLayout initialized with player inventory.");
    }

    void RefreshUI()
    {
        goldText.text = $"Gold: {GoldManager.instance.GetCurrentGold()}";
        // Always clear tracker counts before repopulating
        tracker.ClearItems();
        ClearTeamUnitButtonsOnly();
        PopulateTeamUnits();
        ClearBagItemsOnly();
        PopulateBagItems();
        ClearUnitRelicsOnly();
        if (activeUnit != null) {
            PopulateUnitRelics(activeUnit);
        }
    }

    public void RefreshRelicUI()
    {
        ClearUnitRelicsOnly();
        tracker.ResetCurrentRelicCount();
        if (activeUnit != null) {
            PopulateUnitRelics(activeUnit);
        }
    }

    void ShowDetails(UnitObject unit)
    {
        unitDetailsPanel.Show(unit);
        activeUnit = unit;
        relicContainer.gameObject.SetActive(true);
        RefreshRelicUI();
        HighlightSelectedUnit(unit);
    }

    public void OpenLayout()
    {
        Init();
        if (MapView.Instance != null) {
            MapView.Instance.LockMapInteractions(true);
        } else {
            Global.DEBUG_PRINT("[UnitSettingsLayout::OpenLayout] MapView is null, cannot unlock map interactions.");
        }
        gameObject.SetActive(true);
    }

    public void CloseLayout()
    {
        ClearUnitRelicsOnly();
        ClearTeamUnitButtonsOnly();
        ClearBagItemsOnly();
        activeUnit = null;
        currentSelectedUnitButton = null;
        relicContainer.gameObject.SetActive(false);
        if (MapView.Instance != null) {
            MapView.Instance.LockMapInteractions(false);
        } else {
            Global.DEBUG_PRINT("[UnitSettingsLayout::OpenLayout] MapView is null, cannot unlock map interactions.");
        }
        gameObject.SetActive(false);
    }

    public void SetupDropZones()
    {
        // TeamUnitContainer only accepts units
        DropZoneSetup.AddDropZone(teamUnitContainer.gameObject, DropZone.AllowedItemType.UnitsOnly);

        // RelicContainer only accepts relics
        DropZoneSetup.AddDropZone(relicContainer.gameObject, DropZone.AllowedItemType.RelicsOnly);

        // BagContainer accepts both units and relics
        DropZoneSetup.AddDropZone(bagContainer.gameObject, DropZone.AllowedItemType.UnitsAndRelics);
    }
    
    void HighlightSelectedUnit(UnitObject unit)
    {
        if (currentSelectedUnitButton != null) {
            // Deselect the previously selected unit button
            currentSelectedUnitButton.SetSelected(false);
        }

        foreach (Transform child in teamUnitContainer) {
            var unitButton = child.GetComponentInChildren<UnitButton>();
            if (unitButton != null && unitButton.GetUnit() == unit) {
                unitButton.SetSelected(true);
                currentSelectedUnitButton = unitButton;
                break;
            } else {
                Global.DEBUG_PRINT($"[UnitSettingLayout::HighlightSelectedUnit] No matching unit button found for {unitButton.GetUnit().name}");
            }
        }
    }

    private void GenerateFixedTeamSlots()
    {
        // Clear old slots
        foreach (Transform child in teamUnitContainer) {
            Destroy(child.gameObject);
        }

        // Get ItemTracker from teamUnitContainer or its children
        for (int i = 0; i < tracker.maxUnits; i++) {
            Instantiate(unitSlotPrefab, teamUnitContainer);
        }
    }

    private void ClearTeamUnitButtonsOnly()
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

    private void PopulateTeamUnits() 
    {
        List<UnitObject> listUnit = DeckHelperFunc.GetAllUnitIncludeEmpty(DeckManager.instance.GetDeckByType(eDeckType.PLAYER));

        for (int i = 0; i < teamUnitContainer.childCount; i++) {
            if (i >= listUnit.Count)
                break;

            var slot = teamUnitContainer.GetChild(i);
            // Check if slot is empty
            if (slot.childCount == 0) {
                UnitObject unit = listUnit[i];
                if (unit == null) {
                    continue;
                }
                GameObject unitGO = Instantiate(unitButtonPrefab, slot);
                var unitItem = new MockInventoryItem(unit);
                //unit.uiGameObject = unitGO; // nico: what is this?
                unitGO.GetComponent<UnitButton>().Init(unit, unitItem, ShowDetails);
                unitGO.GetComponent<DragHandler>().Init(unitItem);
                unitGO.GetComponent<ToolTipDetails>().Init(unitDetailsPanel.GetAnyUnitName(unit), unitDetailsPanel.GetAnyUnitDetails(unit));

                // Optional but recommended: make it stretch to fill slot
                RectTransform rt = unitGO.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
            }
        }
    }

    private void ClearBagItemsOnly()
    {
        foreach (Transform slot in bagContainer) {
            foreach (Transform child in slot) {
                Destroy(child.gameObject); // Remove the item inside the slot
            }
        }
    }

    private void GenerateFixedBagSlots()
    {
        for (int i = 0; i < tracker.maxItems; i++) {
            Instantiate(itemSlotPrefab, bagContainer);
        }

        Global.DEBUG_PRINT($"[UnitSettingsLayout::GenerateFixedBagSlots] BagContainer has {tracker.maxItems} slots");
    }

    private void PopulateBagItems() {
        for (int i = 0; i < bagContainer.childCount; i++) {
            if (i >= inventory.bagItems.Count)
                break;

            var slot = bagContainer.GetChild(i);
            var item = inventory.bagItems[i];
            GameObject go;

            if (item.itemType == MockItemType.Unit) {
                go = Instantiate(unitButtonPrefab, slot);
                var unitBtn = go.GetComponent<UnitButton>();
                unitBtn.Init(item.unitData, item, ShowDetails);
                unitBtn.boundItem = item;
                unitBtn.GetComponent<DragHandler>().Init(item);
            } else // Relic
              {
                go = Instantiate(relicButtonPrefab, slot);
                go.GetComponent<RelicButton>().Init(item.relicData, item);
                go.GetComponent<DragHandler>().Init(item);
            }

            // Stretch to fit slot
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }

    private void ClearUnitRelicsOnly()
    {
        foreach (Transform slot in relicContainer) {
            foreach (Transform child in slot) {
                Destroy(child.gameObject);
            }
        }
    }

    private void GenerateFixedUnitRelicSlots()
    {
        for (int i = 0; i < tracker.maxRelics / tracker.maxUnits; i++) {
            Instantiate(relicSlotPrefab, relicContainer);
        }
    }

    private void PopulateUnitRelics(UnitObject unit) {
        int relicIndex = 0;
        List<RelicScriptableObject> listRelic = unit.GetRelic();
        for (int i = 0; i < relicContainer.childCount; i++) {
            if (relicIndex >= listRelic.Count)
                break;

            var slot = relicContainer.GetChild(i);
            GameObject go = Instantiate(relicButtonPrefab, slot);

            RelicScriptableObject relic = listRelic.ElementAt(i);
            MockInventoryItem relicItem = new MockInventoryItem(relic);

            go.GetComponent<RelicButton>().Init(relic, relicItem);
            go.GetComponent<DragHandler>().Init(relicItem);
            //ItemTracker.Instance.AddItem(TrackerType.RelicContainer, MockItemType.Relic);
            // Stretch to fit
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            relicIndex++;
        }
    }

    public UnitObject ActiveUnit => activeUnit;
    public MockPlayerInventory ActiveInventory => inventory;
}
