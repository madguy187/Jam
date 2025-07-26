using UnityEngine;

public enum ItemType { Relic, Unit }

[System.Serializable]
public class ShopItem
{
    public string name;
    public ItemType type;
    public int cost;
    public Sprite icon;
    public bool isSold;

    public ShopItem(string name, ItemType type, int cost, Sprite icon)
    {
        this.name = name;
        this.type = type;
        this.cost = cost;
        this.icon = icon;
        this.isSold = false;
    }

    // Copy constructor
    public ShopItem(ShopItem original)
    {
        name = original.name;
        type = original.type;
        cost = original.cost;
        icon = original.icon;
        isSold = false;
    }
}

