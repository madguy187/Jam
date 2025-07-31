using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class UnitDetailsPanel : MonoBehaviour 
{
    [Header("UI References")]
    public TMP_Text nameText;
    public TMP_Text levelText;
    public TMP_Text hpText;
    public TMP_Text atkText;
    public TMP_Text defText;

    public Transform equippedRelicContainer;   // UI parent for relic slots
    public GameObject relicSlotPrefab;         // Prefab for displaying equipped relics

    private UnitObject currentUnit;

    public void Show(UnitObject unit) 
    {
        // for debug
        // gameObject.SetActive(true);
        currentUnit = unit;
        UpdateUI();
    }

    public void Hide() 
    {
        gameObject.SetActive(false);
    }

    private void UpdateUI() 
    {
        if (currentUnit == null) { return; }
    }

    public string GetAnyUnitName(UnitObject unit) => unit?.unitSO?.GetUnitName() ?? "NULL";

    public string GetAnyUnitDetails(UnitObject unit) => $"Tier: {unit?.unitSO?.GetUnitTierString()}" ?? "NULL";

    public UnitObject GetCurrentUnit() => currentUnit;
}
