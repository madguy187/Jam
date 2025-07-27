using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableRelic : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public RelicScriptableObject data;
    public ForgeManager forgeManager;

    public Transform originalParent;
    private Vector2 offset;

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;
        transform.SetParent(transform.root); // drag to top
        offset = eventData.position - (Vector2)transform.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position - offset;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        GameObject target = eventData.pointerEnter;
        if (target != null && (target.name == "RelicXSlot" || target.name == "RelicYSlot"))
        {
            // forgeManager.AcceptDrop(data, target.transform);
            Destroy(gameObject);
        }
        else
        {
            transform.SetParent(originalParent);
            transform.localPosition = Vector3.zero;
        }
    }
}
