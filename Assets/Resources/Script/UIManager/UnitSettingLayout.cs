using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UnitSettingLayout : MonoBehaviour 
{
    [Header("UI References")]
    public TMP_Text goldText;
    public Transform teamUnitContainer;
    public Transform bagContainer;
    public GameObject unitButtonPrefab;
    public GameObject relicSlotPrefab;

    // [Header("Detail Panel")]
    public UnitDetailsPanel unitDetailsPanel;

    private MockPlayerInventory inventory;

    public void Init(MockPlayerInventory playerInventory) 
    {
        inventory = playerInventory;
        gameObject.SetActive(true);
        SetupDropZones();
        RefreshUI();
        Debug.Log("UnitSettingLayout initialized with player inventory.");
    }

    void RefreshUI() 
    {
        goldText.text = $"Gold: {inventory.gold}";
    
        // Clear and populate team units
        foreach (Transform child in teamUnitContainer) {
            Destroy(child.gameObject);
        }
        foreach (MockUnit unit in inventory.teamUnits) {
            var go = Instantiate(unitButtonPrefab, teamUnitContainer);
            go.GetComponent<UnitButton>().Init(unit, ShowDetails);
        }
    
        // Clear and populate bag with mixed items
        foreach (Transform child in bagContainer) {
            Destroy(child.gameObject);
        }
    
        foreach (MockInventoryItem item in inventory.bagItems) 
        {
            GameObject go;
            if (item.itemType == MockItemType.Unit) 
            {
                go = Instantiate(unitButtonPrefab, bagContainer);
                go.GetComponent<UnitButton>().Init(item.unitData, item, ShowDetails);
                // Assign the bound item so you can drag/drop later
                go.GetComponent<UnitButton>().boundItem = item;
            }
            else // Relic
            {
                go = Instantiate(relicSlotPrefab, bagContainer);
                go.GetComponent<MockRelicSlot>().Init(item.relicData, item);
            }
        }
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
}

