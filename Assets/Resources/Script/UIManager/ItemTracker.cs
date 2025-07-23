using UnityEngine;

public enum TrackerType
{
    BagContainer,
    UnitContainer,
    RelicContainer
}

public class ItemTracker : MonoBehaviour
{
    public static ItemTracker Instance { get; private set; }

    [Header("Limits")]
    [SerializeField] public int maxUnits = 5; // For UnitContainer
    [SerializeField] public int maxRelics = 15; // For RelicContainer
    [SerializeField] public int maxItems = 40; // For BagContainer (combined units + relics)

    [Header("Debug Info")]
    public int currentUnits = 0;
    public int currentRelics = 0;
    public int currentBagItems = 0;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Prevent duplicates
            return;
        }
        Instance = this;
    }

    public bool CanAccept(TrackerType trackerType, MockItemType itemType)
    {
        switch (trackerType)
        {
            case TrackerType.UnitContainer:
                return itemType == MockItemType.Unit && currentUnits < maxUnits;
            case TrackerType.RelicContainer:
                return itemType == MockItemType.Relic && currentRelics < maxRelics;
            case TrackerType.BagContainer:
                return currentBagItems < maxItems;
            default:
                return false;
        }
    }

    public bool AddItem(TrackerType trackerType, MockItemType itemType)
    {
        if (!CanAccept(trackerType, itemType)) {
            Global.DEBUG_PRINT($"[ItemTracker::AddItem] Cannot add item of type {itemType} to {trackerType}. Limit reached.");
            return false;
        }

        switch (trackerType)
        {
            case TrackerType.UnitContainer:
                currentUnits++;
                return true;
            case TrackerType.RelicContainer:
                currentRelics++;
                return true;
            case TrackerType.BagContainer:
                currentBagItems++;
                return true;
            default:
                return false;
        }
    }

    public bool RemoveItem(TrackerType trackerType, MockItemType itemType)
    {
        switch (trackerType)
        {
            case TrackerType.UnitContainer:
                if (currentUnits > 0)
                {
                    currentUnits--;
                    return true;
                }
                break;

            case TrackerType.RelicContainer:
                if (currentRelics > 0)
                {
                    currentRelics--;
                    return true;
                }
                break;

            case TrackerType.BagContainer:
                if (currentBagItems > 0)
                {
                    currentBagItems--;
                }
                break;
        }
        return false;
    }
    public int GetCurrentCount(MockItemType itemType)
    {
        return itemType == MockItemType.Unit ? currentUnits : currentRelics;
    }
    
    public int GetCurrentBagItemCount() => currentBagItems;

    public void ClearItems()
    {
        currentUnits = 0;
        currentRelics = 0;
        currentBagItems = 0;
    }
}
