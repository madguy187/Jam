using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class MockPlayerInventory 
{
    public int gold = 100;
    public List<MockInventoryItem> bagItems = new List<MockInventoryItem>();
}

