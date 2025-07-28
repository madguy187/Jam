using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ForgeRelicDropSlot : MonoBehaviour, IDropHandler
{
    public ForgeSlotID slotID;

    public void OnDrop(PointerEventData eventData)
    {
        if (!CanAcceptDrop()) { return; }

        var dragged = eventData.pointerDrag?.GetComponent<DraggableRelic>();
        if (dragged != null && dragged.relicData != null)
        {
            if (ForgeManager.Instance.IsBreakMode)
            {
                // In break mode, always assign to Slot1 and use breakInputSlot
                ForgeManager.Instance.AssignRelicToSlot(dragged.relicData, ForgeSlotID.Slot1, dragged.gameObject);
            }
            else
            {
                // In merge mode, use defined slotID
                ForgeManager.Instance.AssignRelicToSlot(dragged.relicData, slotID, dragged.gameObject);
            }
        }
    }

    private bool CanAcceptDrop()
    {
        // Check if slot already has a child (relic icon)
        if (transform.childCount > 0)
            return false;

        return true;
    }
}