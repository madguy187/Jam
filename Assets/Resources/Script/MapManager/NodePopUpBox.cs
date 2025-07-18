using UnityEngine;

public class NodePopUpBox : MonoBehaviour
{
    public string messsage;

    private void OnMouseEnter()
    {
        NodePopUpManager._instance.SetAndShowPopUp(messsage);
    }

    private void OnMouseExit()
    {
        NodePopUpManager._instance.HidePopUp();
    }
}
