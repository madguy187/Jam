using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableRelic : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public RelicScriptableObject relicData;

    private CanvasGroup canvasGroup;
    private Transform originalParent;
    private RectTransform rectTransform;

    public Transform OriginalSlot => originalParent;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;
        transform.SetParent(transform.root); // bring to front for dragging
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        // If not dropped onto a valid slot
        if (transform.parent == transform.root)
        {
            ReturnToOriginalSlot();
        }
    }

    public void ReturnToOriginalSlot()
    {
        transform.SetParent(originalParent);
        StretchToFill();
    }

    public void StretchToFill()
    {
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        rectTransform.localPosition = Vector3.zero;
    }
}
