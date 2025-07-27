using UnityEngine;
using UnityEngine.EventSystems;

public class RelicDropSlot : MonoBehaviour, IDropHandler
{
    public GameObject currentRelic;

    public void OnDrop(PointerEventData eventData)
    {
        var draggable = eventData.pointerDrag;
        if (draggable != null && draggable.GetComponent<DraggableRelic>() != null)
        {
            // If slot already has a relic, return that relic to its original slot
            if (currentRelic != null)
            {
                // Move the old relic back to its original parent (assuming it has a DraggableRelic script)
                currentRelic.transform.SetParent(currentRelic.GetComponent<DraggableRelic>().originalParent, false);
            }

            // Set the dragged relic as child of this slot
            draggable.transform.SetParent(transform, false);
            currentRelic = draggable;
        }
    }
}
