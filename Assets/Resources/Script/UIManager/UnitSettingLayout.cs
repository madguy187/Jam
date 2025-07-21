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
        RefreshUI();
        Debug.Log("UnitSettingLayout initialized with player inventory.");
    }

    void RefreshUI() 
    {
        goldText.text = $"Gold: {inventory.gold}";

        // Populate team
        foreach (Transform child in teamUnitContainer) {
            Destroy(child.gameObject);
        }
        foreach (MockUnit unit in inventory.teamUnits) {
            var go = Instantiate(unitButtonPrefab, teamUnitContainer);
            go.GetComponent<UnitButton>().Init(unit, ShowDetails);
        }

        // Populate bag
        foreach (Transform child in bagContainer) {
            Destroy(child.gameObject);
        }
        foreach (MockRelic relic in inventory.bagRelics) {
            var go = Instantiate(relicSlotPrefab, bagContainer);
            go.GetComponent<MockRelicSlot>().Init(relic);
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
}

