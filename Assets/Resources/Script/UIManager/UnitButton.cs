using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class UnitButton : MonoBehaviour, IPointerClickHandler 
{
    [Header("UI Elements")]
    public Text nameText;
    public Image unitIcon;
    public GameObject selectionHighlight;

    private MockUnit unitData;
    private Action<MockUnit> onClickCallback;

    public void Init(MockUnit unit, Action<MockUnit> onClick) 
    {
        unitData = unit;
        onClickCallback = onClick;

        if (nameText != null) {
            nameText.text = unit.unitName;
        }

        if (unitIcon != null) {
            unitIcon.sprite = unit.icon; // optional if you're using portraits
        }

        if (selectionHighlight != null) {
            selectionHighlight.SetActive(false);
        }
    }

    public void OnPointerClick(PointerEventData eventData) 
    {
        onClickCallback?.Invoke(unitData);
        if (selectionHighlight != null) {
            selectionHighlight.SetActive(true); // or toggle
        }
    }

    public void Deselect() {
        if (selectionHighlight != null) 
        {
            selectionHighlight.SetActive(false);
        }
    }

    public MockUnit GetUnit() => unitData;
}
