using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum TrackerType {
    BagContainer,
    UnitContainer,
    RelicContainer,
    Revive
}

public class ItemTracker : MonoBehaviour {
    public static ItemTracker Instance { get; private set; }

    [Header("Limits")]
    [SerializeField] public int maxUnits = 5; // For UnitContainer
    [SerializeField] public int maxRelics = 15; // For RelicContainer
    [SerializeField] public int maxItems = 100; // For BagContainer (combined units + relics)
    [SerializeField] public int maxRevive = 1; // For revival (units)

    [Header("Debug Info")]
    public int currentUnits = 0;
    public int currentRelics = 0;
    public int currentBagItems = 0;
    public int currentRevive = 0;

    Dictionary<TrackerType, Action<MockItemType, MockInventoryItem>> mapOnAdd = new Dictionary<TrackerType, Action<MockItemType, MockInventoryItem>>();
    Dictionary<TrackerType, Action<MockItemType, MockInventoryItem>> mapOnRemove = new Dictionary<TrackerType, Action<MockItemType, MockInventoryItem>>();
    public void AddOnAdd(TrackerType type, Action<MockItemType, MockInventoryItem> action) {
        mapOnAdd[type] = action;
    }
    public void AddOnRemove(TrackerType type, Action<MockItemType, MockInventoryItem> action) {
        mapOnRemove[type] = action;
    }

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject); // Prevent duplicates
            return;
        }
        Instance = this;
        Global.DEBUG_PRINT("[ItemTracker::Awake] Instance created.");
    }

    public bool CanAccept(TrackerType trackerType, MockItemType itemType)
    {
        switch (trackerType) {
            case TrackerType.UnitContainer:
                return itemType == MockItemType.Unit && currentUnits < maxUnits;
            case TrackerType.RelicContainer:
                return itemType == MockItemType.Relic && currentRelics < maxRelics;
            case TrackerType.BagContainer:
                return currentBagItems < maxItems;
            case TrackerType.Revive:
                return itemType == MockItemType.Unit && currentRevive < maxRevive;
            default:
                return false;
        }
    }

    public bool AddItem(TrackerType trackerType, MockItemType itemType, MockInventoryItem item = null) {
        if (!CanAccept(trackerType, itemType)) {
            Global.DEBUG_PRINT($"[ItemTracker::AddItem] Cannot add item of type {itemType} to {trackerType}. Limit reached.");
            return false;
        }

        if (mapOnAdd.ContainsKey(trackerType)) {
            mapOnAdd[trackerType](itemType, item);
        }

        UpdateCount(trackerType, 1);

        return true;
    }

    void UpdateCount(TrackerType trackerType, int count) {
        switch (trackerType) {
            case TrackerType.UnitContainer:
                currentUnits += count;
                break;
            case TrackerType.RelicContainer:
                currentRelics += count;
                break;
            case TrackerType.BagContainer:
                currentBagItems += count;
                break;
            case TrackerType.Revive:
                currentRevive += count;
                break;
            default:
                break;
        }
    }

    public bool RemoveItem(TrackerType trackerType, MockItemType itemType, MockInventoryItem item = null) {
        if (mapOnRemove.ContainsKey(trackerType)) {
            mapOnRemove[trackerType](itemType, item);
        }
        
        UpdateCount(trackerType, -1);

        return true;
    }

    public int GetCurrentCount(MockItemType itemType) {
        return itemType == MockItemType.Unit ? currentUnits : currentRelics;
    }

    public int GetCurrentBagItemCount() => currentBagItems;

    public void ClearItems() {
        currentUnits = 0;
        currentRelics = 0;
        currentBagItems = 0;
        currentRevive = 0;
    }
    
    public void ResetCurrentRelicCount()
    {
        currentRelics = 0;
    }
}
