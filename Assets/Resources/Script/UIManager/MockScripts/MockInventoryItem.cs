using System.Collections.Generic;

public enum MockItemType { Unit, Relic, None }

[System.Serializable]
public class MockInventoryItem {
    public MockItemType itemType;
    public UnitObject unitData;
    public RelicScriptableObject relicData;

    public MockInventoryItem(UnitObject unit) {
        itemType = MockItemType.Unit;
        unitData = unit;
        relicData = null;
    }

    public MockInventoryItem(RelicScriptableObject relic) {
        itemType = MockItemType.Relic;
        relicData = relic;
        unitData = null;
    }

    public MockItemType GetItemType() {
        return itemType;
    }
}