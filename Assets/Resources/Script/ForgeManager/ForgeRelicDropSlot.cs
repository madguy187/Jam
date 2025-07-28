using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ForgeRelicDropSlot : MonoBehaviour, IDropHandler
{
    public ForgeSlotID slotID;

    public void OnDrop(PointerEventData eventData)
    {
        var dragged = eventData.pointerDrag?.GetComponent<DraggableRelic>();
        if (dragged != null && dragged.relicData != null) {
            ForgeManager.Instance.AssignRelicToSlot(dragged.relicData, slotID, dragged.gameObject);
        }
    }
}