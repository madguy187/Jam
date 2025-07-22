using UnityEngine;

public class ItemTracker : MonoBehaviour
{
    [Header("Limits")]
    [SerializeField] public int maxUnits = 5;
    [SerializeField] public int maxRelics = 10;

    public int currentUnits = 0;
    public int currentRelics = 0;

    // Call this to check if the container can accept another item of the given type
    public bool CanAccept(MockItemType itemType)
    {
        switch (itemType)
        {
            case MockItemType.Unit:
                return currentUnits < maxUnits;
            case MockItemType.Relic:
                return currentRelics < maxRelics;
            default:
                return false;
        }
    }

    // Call this to add an item count of the given type
    public bool AddItem(MockItemType itemType)
    {
        if (!CanAccept(itemType))
            return false;

        switch (itemType)
        {
            case MockItemType.Unit:
                currentUnits++;
                return true;
            case MockItemType.Relic:
                currentRelics++;
                return true;
        }
        return false;
    }

    // Call this to remove an item count of the given type
    public bool RemoveItem(MockItemType itemType)
    {
        switch (itemType)
        {
            case MockItemType.Unit:
                if (currentUnits > 0)
                {
                    currentUnits--;
                    return true;
                }
                break;
            case MockItemType.Relic:
                if (currentRelics > 0)
                {
                    currentRelics--;
                    return true;
                }
                break;
        }
        return false;
    }

    // Optional: get current counts
    public int GetCurrentCount(MockItemType itemType)
    {
        return itemType == MockItemType.Unit ? currentUnits : currentRelics;
    }
}