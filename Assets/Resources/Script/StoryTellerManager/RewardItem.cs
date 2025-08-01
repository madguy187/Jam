using UnityEngine;

[System.Serializable]
public class RewardItem
{
    public string name;
    public string description;
    public RewardItemType type;
    public int cost;
    public Sprite icon;
    public bool isSold;

    public RewardItem(string name, string description, RewardItemType type, int cost, Sprite icon)
    {
        this.name = name;
        this.description = description;
        this.type = type;
        this.cost = cost;
        this.icon = icon;
        this.isSold = false;
    }

    // Copy constructor
    public RewardItem(RewardItem original)
    {
        name = original.name;
        description = original.description;
        type = original.type;
        cost = original.cost;
        icon = original.icon;
        isSold = false;
    }
}