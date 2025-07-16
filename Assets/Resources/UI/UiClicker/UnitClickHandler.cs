using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(UnitObject))]
public class UnitClickHandler : MonoBehaviour, IPointerClickHandler
{
    private UnitObject _unit;

    void Awake()
    {
        _unit = GetComponent<UnitObject>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Global.DEBUG_PRINT("[UnitClickHandler] Unit clicked!");
        
        UnitInfoPanelController panelController = FindObjectOfType<UnitInfoPanelController>();
        
        if (panelController != null)
        {
            Global.DEBUG_PRINT("[UnitClickHandler] Showing info for unit: " + _unit.unitSO.unitName);
            panelController.ShowUnitInfo(_unit);
        }
        else
        {
            Global.DEBUG_PRINT("[UnitClickHandler] ERROR: UnitInfoPanelController not found in scene!");
        }
    }
} 