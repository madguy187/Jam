using UnityEngine;
using System.Collections;

public class NodePopUpBox : MonoBehaviour
{
    public string messsage;
    private Coroutine popupCoroutine;

    private Map.MapView mapView;
    
    void Start()
    {
        mapView = Map.MapView.Instance;
        if (mapView == null) {
            Global.DEBUG_PRINT("[NodePopUpBox::Start] MapView instance is null, cannot show popups.");
        }
    }
    
    private void OnMouseEnter()
    {
        if (mapView.AreInteractionsLocked()) { return; }
        popupCoroutine = StartCoroutine(ShowPopupWithDelay());
    }

    private void OnMouseExit()
    {
        if (mapView.AreInteractionsLocked()) { return; }
        if (popupCoroutine != null) {
            StopCoroutine(popupCoroutine);
            popupCoroutine = null;
        }
        NodePopUpManager._instance.HidePopUp();
    }

    private IEnumerator ShowPopupWithDelay()
    {
        yield return new WaitForSeconds(0.25f); // quarter of a second delay before showing the popup
        NodePopUpManager._instance.SetAndShowPopUp(messsage);
    }
}