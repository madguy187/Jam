using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class MockPlayerInventory 
{
    public int gold;
    public List<MockUnit> teamUnits;
    public List<MockInventoryItem> bagItems = new List<MockInventoryItem>();
}

