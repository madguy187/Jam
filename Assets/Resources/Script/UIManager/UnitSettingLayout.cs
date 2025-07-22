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
    public GameObject relicSlotPrefab;

    [Header("Detail Panel")]
    public UnitDetailsPanel unitDetailsPanel;

    private MockPlayerInventory inventory;

    public void Init(MockPlayerInventory playerInventory) {
        inventory = playerInventory;
        gameObject.SetActive(true);
        SetupDropZones();
        GenerateFixedTeamSlots();
        RefreshUI();
        Debug.Log("UnitSettingLayout initialized with player inventory.");
    }

    void RefreshUI() {
        goldText.text = $"Gold: {inventory.gold}";

        ClearTeamUnitButtonsOnly();
        PopulateTeamUnits();

        // Clear and populate bag with mixed items
        foreach (Transform child in bagContainer) {
            Destroy(child.gameObject);
        }

        foreach (MockInventoryItem item in inventory.bagItems) {
            GameObject go;
            if (item.itemType == MockItemType.Unit) {
                go = Instantiate(unitButtonPrefab, bagContainer);
                go.GetComponent<UnitButton>().Init(item.unitData, item, ShowDetails);
                // Assign the bound item so you can drag/drop later
                go.GetComponent<UnitButton>().boundItem = item;
            } else // Relic
              {
                go = Instantiate(relicSlotPrefab, bagContainer);
                go.GetComponent<MockRelicSlot>().Init(item.relicData, item);
            }
        }
    }

    void ShowDetails(MockUnit unit) {
        unitDetailsPanel.Show(unit);
    }

    public void CloseLayout() {
        gameObject.SetActive(false);
    }

    public void SetupDropZones() {
        // TeamUnitContainer only accepts units
        DropZoneSetup.AddDropZone(teamUnitContainer.gameObject, DropZone.AllowedItemType.UnitsOnly);

        // BagContainer accepts both units and relics
        DropZoneSetup.AddDropZone(bagContainer.gameObject, DropZone.AllowedItemType.UnitsAndRelics);
    }

    private void GenerateFixedTeamSlots() {
        // Clear old slots
        foreach (Transform child in teamUnitContainer) {
            Destroy(child.gameObject);
        }

        // Get ItemTracker from teamUnitContainer or its children
        var tracker = teamUnitContainer.GetComponentInChildren<ItemTracker>();
        if (tracker == null) {
            Debug.LogWarning("ItemTracker component not found in teamUnitContainer or its children.");
            return;
        }
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
    
    void PopulateTeamUnits()
    {
        int unitIndex = 0;

        foreach (Transform slot in teamUnitContainer)
        {
            // Safety check: stop if no more units to assign
            if (unitIndex >= inventory.teamUnits.Count)
                break;

            // Check if slot is empty
            if (slot.childCount == 0)
            {
                var unit = inventory.teamUnits[unitIndex];

                GameObject unitGO = Instantiate(unitButtonPrefab, slot);
                unitGO.GetComponent<UnitButton>().Init(unit, ShowDetails);
                // Get ItemTracker from teamUnitContainer or its children
                var tracker = teamUnitContainer.GetComponentInChildren<ItemTracker>();
                if (tracker == null) {
                    Debug.LogWarning("ItemTracker component not found in teamUnitContainer or its children.");
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
}
