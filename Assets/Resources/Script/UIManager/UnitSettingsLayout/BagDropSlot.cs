using UnityEngine;
using UnityEngine.EventSystems;

public class BagDropSlot : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        GameObject dropped = eventData.pointerDrag;
        if (dropped != null)
        {
            dropped.transform.SetParent(transform);
            dropped.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        }
    }
}
