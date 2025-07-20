using UnityEngine;

public class UnitClickHandler : MonoBehaviour
{
    private UnitObject unit;

    void Awake()
    {
        unit = GetComponent<UnitObject>();
        
        // Add BoxCollider2D if dont have
        // need to change method as previously i used IPointerClickHandler which works with only UI elementts
        if (GetComponent<BoxCollider2D>() == null)
        {
            var collider = gameObject.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;  
        }
    }

    void OnMouseDown()
    {
        Global.DEBUG_PRINT("[UnitClickHandler] Unit clicked!");
        
        var panelManager = PanelManager.GetInstance();
        if (panelManager != null)
        {
            Global.DEBUG_PRINT("[UnitClickHandler] Showing info for unit: " + unit.unitSO.unitName);
            panelManager.ShowUnitInfo(unit);
        }
        else
        {
            Global.DEBUG_PRINT("[UnitClickHandler] ERROR: PanelManager instance not found in scene!");
        }
    }
}   