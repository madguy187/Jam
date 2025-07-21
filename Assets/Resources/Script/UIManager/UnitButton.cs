using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using TMPro;

public class UnitButton : MonoBehaviour
{
    public Image unitIcon;
    public TMP_Text nameText;

    private MockUnit unitData;
    private System.Action<MockUnit> onClickCallback;

    public void Init(MockUnit unit, System.Action<MockUnit> onClick)
    {
        unitData = unit;
        onClickCallback = onClick;
        nameText.text = unit.unitName;
        if (unit.icon != null)
            unitIcon.sprite = unit.icon;
    }

    public void OnClick()
    {
        onClickCallback?.Invoke(unitData);
    }
}

