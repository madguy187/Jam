using UnityEngine;
using System.Collections.Generic;

public class MockPlayerInventoryHolder : MonoBehaviour
{
    public MockPlayerInventory playerInventory = new MockPlayerInventory();

    private void Awake()
    {
        // Initialize with some test data here
        playerInventory.gold = 100;

        playerInventory.teamUnits = new List<MockUnit>()
        {
            new MockUnit("Warrior", 100),
            new MockUnit("Mage", 60)
        };

        playerInventory.bagRelics = new List<MockRelic>()
        {
            new MockRelic("Amulet of Power", MockRelicRarity.Rare),
            new MockRelic("Shield of Light", MockRelicRarity.Common)
        };
    }
}
