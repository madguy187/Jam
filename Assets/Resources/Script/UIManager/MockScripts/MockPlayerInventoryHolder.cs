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
            new MockUnit("Warrior", 100, Resources.Load<Sprite>("Sprites/game-icons.net/brute")),
            new MockUnit("Mage", 60, Resources.Load<Sprite>("Sprites/game-icons.net/monk-face")),
            new MockUnit("Paladin", 500, Resources.Load<Sprite>("Sprites/game-icons.net/mounted-knight")),
            new MockUnit("Rogue", 80, Resources.Load<Sprite>("Sprites/game-icons.net/orc-head")),
            new MockUnit("Archer", 70, Resources.Load<Sprite>("Sprites/game-icons.net/troll"))
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
