using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class MockPlayerInventory {
    public int gold;
    public List<MockUnit> teamUnits;
    public List<MockInventoryItem> bagItems = new List<MockInventoryItem>();
    
    public void MoveUnitToBag(MockUnit unit)
    {
        if (teamUnits.Contains(unit)) {
            teamUnits.Remove(unit);
            bagItems.Add(new MockInventoryItem(unit));
        }
    }

    public void MoveUnitToTeam(MockInventoryItem item)
    {
        if (item.itemType == MockItemType.Unit && !teamUnits.Contains(item.unitData)) {
            bagItems.Remove(item);
            teamUnits.Add(item.unitData);
        }
    }
}

