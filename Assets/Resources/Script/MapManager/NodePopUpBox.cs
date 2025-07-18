using UnityEngine;
using System.Collections;

public class NodePopUpBox : MonoBehaviour
{
    public string messsage;
    private Coroutine popupCoroutine;

    private void OnMouseEnter()
    {
        popupCoroutine = StartCoroutine(ShowPopupWithDelay());
    }

    private void OnMouseExit()
    {
        if (popupCoroutine != null)
        {
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