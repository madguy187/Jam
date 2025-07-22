using UnityEngine;
using UnityEngine.EventSystems;

public class DragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDragHandlerInterface
{
    private Canvas canvas;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;

    private Vector3 originalPosition;
    private Transform originalParent;

    public MockInventoryItem draggedItem;  // Assign this when initializing the UI element

    private void Awake() 
    {
        canvas = GetComponentInParent<Canvas>();
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void Init(MockInventoryItem item)
    {
        draggedItem = item;
    }

    public MockItemType GetItemType()
    {
        return draggedItem?.itemType ?? MockItemType.None;
    }

    public MockInventoryItem GetDraggedItem()
    {
        return draggedItem;
    }

    public void OnDropAccepted()
    {
        // Optional: play a sound, highlight, etc.
    }

    public void OnDropRejected()
    {
        // Reset to original position and parent
        transform.SetParent(originalParent);
        rectTransform.position = originalPosition;
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalPosition = rectTransform.position;
        originalParent = transform.parent;

        if (originalParent != null) {
            var tracker = originalParent.GetComponentInParent<ItemTracker>();
            if (tracker != null) {
                tracker.RemoveItem(draggedItem.GetItemType());
            } else {
                Global.DEBUG_PRINT("[DragHandler::OnBeginDrag] ItemTracker cannot be found in parent.");
            }
        }

        canvasGroup.alpha = 0.6f;  // make transparent while dragging
        canvasGroup.blocksRaycasts = false; // so drop targets can receive events

        // Move to root canvas so it can drag over other UI elements
        transform.SetParent(canvas.transform);
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform, 
            eventData.position, 
            eventData.pressEventCamera, 
            out localPoint);

        rectTransform.localPosition = localPoint;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        // If dropped over a valid drop zone, the drop zone script should handle reparenting.
        // If not, revert to original position and parent.
        if (transform.parent == canvas.transform)
        {
            transform.SetParent(originalParent);
            rectTransform.position = originalPosition;
        }
    }
}