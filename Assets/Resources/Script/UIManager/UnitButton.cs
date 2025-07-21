using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class UnitButton : MonoBehaviour, IDragHandlerInterface
{
    public Image unitIcon;
    public TMP_Text nameText;

    private MockUnit unitData;
    private Action<MockUnit> onClickCallback;

    // This is the inventory wrapper holding the unit or relic reference
    public MockInventoryItem boundItem;

    // Initialize with the unit and click callback, plus assign the inventory item
    public void Init(MockUnit unit, MockInventoryItem inventoryItem, Action<MockUnit> onClick)
    {
        unitData = unit;
        boundItem = inventoryItem;
        onClickCallback = onClick;

        nameText.text = unit.unitName;
        if (unit.icon != null)
            unitIcon.sprite = unit.icon;
    }
    
    public void Init(MockUnit unit, Action<MockUnit> onClick)
    {
        Init(unit, null, onClick); // Call the main Init with null for boundItem
    }

    public void OnClick()
    {
        onClickCallback?.Invoke(unitData);
    }

    // IDragHandlerInterface implementations
    public MockItemType GetItemType()
    {
        return MockItemType.Unit;
    }

    public MockInventoryItem GetDraggedItem()
    {
        return boundItem;
    }

    public void OnDropAccepted()
    {
        // Optional: Add visual or state changes here on drop accepted
    }

    public void OnDropRejected()
    {
        // Optional: Reset position or provide feedback on drop rejection
    }
}
