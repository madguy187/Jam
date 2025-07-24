using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class UnitButton : MonoBehaviour
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
        GetComponent<Button>().onClick.AddListener(() => onClick?.Invoke(unit));

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
}
