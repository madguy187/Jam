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
        equippedRelics = new List<MockRelic>();
    }

    public void EquipRelic(MockRelic relic) 
    {
        if (!equippedRelics.Contains(relic)) {
            equippedRelics.Add(relic);
        }
    }

    public void UnequipRelic(MockRelic relic) 
    {
        equippedRelics.Remove(relic);
    }
}
