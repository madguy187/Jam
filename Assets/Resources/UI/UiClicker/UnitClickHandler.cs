using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

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

        if (SceneManager.GetActiveScene().name != "Game")
        {
            return;
        }

        if (unit != null)
        {
            PanelManager.instance.ShowUnitInfo(unit);
        }
    }
}   