using UnityEngine;

public enum RewardItemType
{
    Unit,
    Relic,
    Gold
}

public static class RewardItemTypeConverter
{
    public static string ToString(this RewardItemType type)
    {
        switch (type)
        {
            case RewardItemType.Unit:
                return "Unit";
            case RewardItemType.Relic:
                return "Relic";
            case RewardItemType.Gold:
                return "Gold";
            default:
                return type.ToString();
        }
    }
}
