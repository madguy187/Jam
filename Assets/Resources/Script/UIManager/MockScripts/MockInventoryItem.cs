using System.Collections.Generic;

public enum MockItemType { Unit, Relic, None }

[System.Serializable]
public class MockInventoryItem {
    public MockItemType itemType;
    public MockUnit unitData;
    public MockRelic relicData;

    public MockInventoryItem(MockUnit unit) {
        itemType = MockItemType.Unit;
        unitData = unit;
        relicData = null;
    }

    public MockInventoryItem(MockRelic relic) {
        itemType = MockItemType.Relic;
        relicData = relic;
        unitData = null;
    }

    public MockItemType GetItemType() {
        return itemType;
    }
}