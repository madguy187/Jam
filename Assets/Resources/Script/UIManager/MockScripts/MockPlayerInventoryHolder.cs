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
            new MockUnit("Mage", 60),
            new MockUnit("Paladin", 500),
            new MockUnit("Rogue", 80),
            new MockUnit("Archer", 70)
        };

        playerInventory.bagRelics = new List<MockRelic>()
        {
            new MockRelic("Amulet of Power", MockRelicRarity.Rare),
            new MockRelic("Shield of Light", MockRelicRarity.Common),
            new MockRelic("Ring of Wisdom", MockRelicRarity.Epic),
            new MockRelic("Crown of Kings", MockRelicRarity.Legendary),
            new MockRelic("Staff of Ages", MockRelicRarity.Legendary),
            new MockRelic("Boots of Speed", MockRelicRarity.Rare)
        };
    }
}
