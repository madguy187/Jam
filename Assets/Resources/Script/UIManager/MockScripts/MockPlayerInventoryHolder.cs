using UnityEngine;
using System;

public class MockPlayerInventoryHolder : MonoBehaviour
{
    public static MockPlayerInventoryHolder Instance { get; private set; }
    public MockPlayerInventory playerInventory = new MockPlayerInventory();

    private void Awake()
    {
        if (Instance != null && Instance != this) {
            Destroy(gameObject); // Prevent duplicates
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start() 
    {
        Action<MockItemType, MockInventoryItem> onAddAction = (MockItemType itemType, MockInventoryItem item) => {
            playerInventory.bagItems.Add(item);
            Deck cDeck = DeckManager.instance.GetDeckByType(eDeckType.PLAYER);
            if (itemType == MockItemType.Unit) {
                cDeck.RemoveUnit(item.unitData);
            }
        };
        ItemTracker.Instance.AddOnAdd(TrackerType.BagContainer, onAddAction);

        Action<MockItemType, MockInventoryItem> onRemoveAction = (MockItemType itemType, MockInventoryItem item) => {
            playerInventory.bagItems.Remove(item);
            Deck cDeck = DeckManager.instance.GetDeckByType(eDeckType.PLAYER);
            if (itemType == MockItemType.Unit) {
                cDeck.AddUnit(item.unitData);
            }
        };
        ItemTracker.Instance.AddOnRemove(TrackerType.BagContainer, onRemoveAction);
    }

    public void ClearInventory()
    {
        playerInventory.bagItems.Clear();
        ItemTracker.Instance.ClearItems();
        Global.DEBUG_PRINT("[MockPlayerInventoryHolder::ClearInventory] Cleared player inventory and ITEM TRACKER.");
    }
}
