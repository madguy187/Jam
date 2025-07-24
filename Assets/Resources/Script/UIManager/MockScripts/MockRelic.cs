using UnityEngine;

[System.Serializable]
public class MockRelic 
{
    public string relicName;
    public string description;
    public Sprite icon;
    public MockRelicRarity rarity;

    // Optional effects (mocked as stat modifiers)
    public int bonusHP;
    public int bonusAttack;
    public int bonusDefense;

    public MockRelic(string name, MockRelicRarity rarity) 
    {
        relicName = name;
        this.rarity = rarity;
        icon = Resources.Load<Sprite>("Sprites/Axe_1");
        if (icon == null) {
            Global.DEBUG_PRINT("[MockRelic] Relic icon not found!");
        }
        bonusHP = Random.Range(5, 15);
        bonusAttack = Random.Range(1, 5);
        bonusDefense = Random.Range(0, 3);
    }
}

public enum MockRelicRarity 
{
    Common,
    Rare,
    Epic,
    Legendary
}

