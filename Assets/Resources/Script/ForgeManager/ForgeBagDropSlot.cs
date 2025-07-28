using UnityEngine;
using UnityEngine.EventSystems;

public class ForgeBagDropSlot : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        var dragged = eventData.pointerDrag?.GetComponent<DraggableRelic>();
        if (dragged != null && dragged.relicData != null)
        {
            // Remove from forge slot if present
            ForgeManager.Instance.RemoveRelicFromForge(dragged.relicData);

            // Try to snap to the first available bag slot
            bool snapped = ForgeManager.Instance.SnapToFirstAvailableSlot(dragged.gameObject);
            if (!snapped)
            {
                Global.DEBUG_PRINT("[ForgeBagDropSlot] No available slot to snap relic back.");
            }
        }
    }
}