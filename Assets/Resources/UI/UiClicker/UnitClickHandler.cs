using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(UnitObject))]
public class UnitClickHandler : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private PanelManager panelManager;
    private UnitObject unit;

    void Awake()
    {
        unit = GetComponent<UnitObject>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Global.DEBUG_PRINT("[UnitClickHandler] Unit clicked!");
        
        if (panelManager != null)
        {
            Global.DEBUG_PRINT("[UnitClickHandler] Showing info for unit: " + unit.unitSO.unitName);
            panelManager.ShowUnitInfo(unit);
        }
        else
        {
            Global.DEBUG_PRINT("[UnitClickHandler] ERROR: PanelManager reference not set!");
        }
    }
} 