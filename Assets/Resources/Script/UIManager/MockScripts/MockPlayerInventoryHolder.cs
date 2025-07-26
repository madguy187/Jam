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
    }

    private void Start() {
        // Initialize with some test data here
        playerInventory.gold = 100;
        
        Action<MockItemType, MockInventoryItem> onAddAction = (MockItemType itemType, MockInventoryItem item) => {
            playerInventory.bagItems.Add(item);
            Deck cDeck = DeckManager.instance.GetDeckByType(eDeckType.PLAYER);
            cDeck.RemoveUnit(item.unitData);
        };
        ItemTracker.Instance.AddOnAdd(TrackerType.BagContainer, onAddAction);

        Action<MockItemType, MockInventoryItem> onRemoveAction = (MockItemType itemType, MockInventoryItem item) => {
            playerInventory.bagItems.Remove(item);
            Deck cDeck = DeckManager.instance.GetDeckByType(eDeckType.PLAYER);
            cDeck.AddUnit(item.unitData);
        };
        ItemTracker.Instance.AddOnRemove(TrackerType.BagContainer, onRemoveAction);
    }
}
