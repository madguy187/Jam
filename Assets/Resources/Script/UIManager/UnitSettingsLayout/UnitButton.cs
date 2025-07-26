using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System;

public class UnitButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    [Header("UI Components")]
    public Image unitIcon;
    public TMP_Text nameText;

    private UnitObject unitData;
    private Action<UnitObject> onClickCallback;

    // This is the inventory wrapper holding the unit or relic reference
    public MockInventoryItem boundItem;

    private Vector3 normalScale = Vector3.one;
    private Vector3 hoverScale = new Vector3(1.2f, 1.2f, 1f);
    private Vector3 selectedScale;
    private bool isSelected = false;

    private void Start() {
        selectedScale = hoverScale;
    }

    // Initialize with the unit and click callback, plus assign the inventory item
    public void Init(UnitObject unit, MockInventoryItem inventoryItem, Action<UnitObject> onClick) {
        unitData = unit;
        boundItem = inventoryItem;
        onClickCallback = onClick;

        GetComponent<Button>().onClick.RemoveAllListeners(); // Safe rebind
        GetComponent<Button>().onClick.AddListener(OnClick);

        nameText.text = unit.unitSO.unitName;

        Sprite sprite = DeckDisplayManager.RenderUnitToSprite(unit);
        if (sprite != null) {
            unitIcon.sprite = sprite;
        }

        SetSelected(false);
        SetHover(false);
    }

    public void Init(UnitObject unit, Action<UnitObject> onClick) {
        Init(unit, null, onClick); // Call the main Init with null for boundItem
    }

    public void OnClick() {
        onClickCallback?.Invoke(unitData);
    }

    public void SetSelected(bool selected) {
        isSelected = selected;
        UpdateIconScale();
    }

    // Call this on hover enter/exit if you have hover effects
    public void SetHover(bool isHovering) {
        if (isSelected) { return; } // don't override selected scale while selected

        unitIcon.rectTransform.localScale = isHovering ? hoverScale : normalScale;
    }

    private void UpdateIconScale() {
        unitIcon.rectTransform.localScale = isSelected ? selectedScale : normalScale;
    }

    public UnitObject GetUnit() => unitData;
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        SetHover(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SetHover(false);
    }
}
