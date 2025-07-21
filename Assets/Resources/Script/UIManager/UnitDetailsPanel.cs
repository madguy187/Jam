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

    private MockUnit currentUnit;

    public void Show(MockUnit unit) 
    {
        gameObject.SetActive(true);
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

        nameText.text = currentUnit.unitName;
        levelText.text = $"Lv {currentUnit.level}";
        hpText.text = $"HP: {currentUnit.currentHP} / {currentUnit.maxHP}";
        atkText.text = $"ATK: {currentUnit.attack}";
        defText.text = $"DEF: {currentUnit.defense}";

        UpdateEquippedRelics();
    }

    private void UpdateEquippedRelics() 
    {
        foreach (Transform child in equippedRelicContainer) {
            Destroy(child.gameObject);
        }

        foreach (MockRelic relic in currentUnit.equippedRelics) {
            var go = Instantiate(relicSlotPrefab, equippedRelicContainer);
            var slot = go.GetComponent<MockRelicSlot>();
            slot.Init(relic);
        }
    }

    public MockUnit GetCurrentUnit() => currentUnit;
}
