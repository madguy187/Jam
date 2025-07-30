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

        // nameText.text = currentUnit.unitName;
        // levelText.text = $"Lv {currentUnit.level}";
        // hpText.text = $"HP: {currentUnit.currentHP} / {currentUnit.maxHP}";
        // atkText.text = $"ATK: {currentUnit.attack}";
        // defText.text = $"NOR: {currentUnit.equippedRelics.Count}"; // Placeholder to check relic count

        UpdateEquippedRelics();
    }

    private void UpdateEquippedRelics() {
        // foreach (Transform child in equippedRelicContainer) {
        //     Destroy(child.gameObject);
        // }
        // foreach (MockRelic relic in currentUnit.equippedRelics) {
        //     var go = Instantiate(relicSlotPrefab, equippedRelicContainer);
        //     var slot = go.GetComponent<RelicButton>();
        //     slot.Init(relic);
        // }
    }

    public string GetAnyUnitName(UnitObject unit) => unit?.unitSO?.unitName ?? "NULL";

    public string GetAnyUnitDetails(UnitObject unit) 
    {
        if (unit == null) return "NULL";

        HashSet<EffectType> uniqueTypes = new HashSet<EffectType>();

        void AddFromList(EffectList list)
        {
            if (list == null || !list.IsValid()) return;
            foreach (EffectScriptableObject eff in list)
            {
                if (eff == null) continue;
                uniqueTypes.Add(eff.GetEffectType());
            }
        }

        // Gather from all effect lists defined in UnitObject
        /*AddFromList(unit._listSingleEffect);
        AddFromList(unit._listHorizontalEffect);
        AddFromList(unit._listDiagonalEffect);
        AddFromList(unit._listZigZagEffect);
        AddFromList(unit._listXShapeEffect);
        AddFromList(unit._listFullGridEffect);*/

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        foreach (EffectType eType in uniqueTypes)
        {
            EffectDetailScriptableObject detail = ResourceManager.instance.GetEffectDetail(eType);
            if (detail != null && !string.IsNullOrEmpty(detail.strDescription))
            {
                sb.AppendLine(detail.strDescription);
            }
            else
            {
                sb.AppendLine(eType.ToString());
            }
        }

        return sb.ToString();
    }

    public UnitObject GetCurrentUnit() => currentUnit;
}
