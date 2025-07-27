using UnityEngine;

public enum ShopItemType { Relic, Unit }

[System.Serializable]
public class ShopItem
{
    public string name;
    public string description;
    public ShopItemType type;
    public int cost;
    public Sprite icon;
    public bool isSold;

    public ShopItem(string name, string description, ShopItemType type, int cost, Sprite icon)
    {
        this.name = name;
        this.description = description;
        this.type = type;
        this.cost = cost;
        this.icon = icon;
        this.isSold = false;
    }

    // Copy constructor
    public ShopItem(ShopItem original)
    {
        name = original.name;
        description = original.description;
        type = original.type;
        cost = original.cost;
        icon = original.icon;
        isSold = false;
    }
}