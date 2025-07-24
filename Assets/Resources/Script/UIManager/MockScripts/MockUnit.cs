using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class MockUnit 
{
    public string unitName;
    public int level;
    public int maxHP;
    public int currentHP;
    public int attack;
    public int defense;
    public List<MockRelic> equippedRelics;
    public Sprite icon;

    public MockUnit(string name, int lvl, Sprite icon) 
    {
        unitName = name;
        level = lvl;
        maxHP = 100 + lvl * 10;
        currentHP = maxHP;
        this.icon = icon;
        attack = 10 + lvl * 2;
        defense = 5 + lvl;
        equippedRelics = new List<MockRelic> {
            // Adding default relics for testing
            new MockRelic("Amulet of Power", MockRelicRarity.Rare)
        };
    }
    
    public bool EquipRelic(MockRelic relic)
    {
        if (!equippedRelics.Contains(relic)) {
            equippedRelics.Add(relic);
            Global.DEBUG_PRINT($"[MockUnit::EquipRelic] Equipped relic: {relic.relicName} to unit: {unitName}, size of equipped relics: {equippedRelics.Count}");
            return true;
        }
        Global.DEBUG_PRINT($"[MockUnit::EquipRelic] Relic: {relic.relicName} is already equipped to unit: {unitName}");
        return false;
    }

    public bool UnequipRelic(MockRelic relic)
    {
        if (equippedRelics.Contains(relic)) {
            equippedRelics.Remove(relic);
            Global.DEBUG_PRINT($"[MockUnit::UnequipRelic] Unequipped relic: {relic.relicName} from unit: {unitName}, size of equipped relics: {equippedRelics.Count}");
            return true;
        }
        Global.DEBUG_PRINT($"[MockUnit::UnequipRelic] Relic: {relic.relicName} not found in unit: {unitName}");
        return false;
    }
}
