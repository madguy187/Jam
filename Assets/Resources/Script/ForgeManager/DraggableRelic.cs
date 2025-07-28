using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableRelic : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public RelicScriptableObject relicData;

    private CanvasGroup canvasGroup;
    private Transform originalParent;
    private RectTransform rectTransform;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;
        transform.SetParent(transform.root); // drag on top
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        if (transform.parent == transform.root)
        {
            // Not dropped on a valid slot, return to original slot
            transform.SetParent(originalParent);
            rectTransform.localPosition = Vector3.zero;
        }
    }

    public Transform originalSlot
    {
        get { return originalParent; }
    }
}
