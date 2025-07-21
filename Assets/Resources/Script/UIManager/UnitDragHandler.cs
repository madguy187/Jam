using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UnitDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Canvas canvas;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Transform originalParent;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        // Find the root canvas
        canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
            Debug.LogError("Canvas not found for drag handler!");
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;
        transform.SetParent(canvas.transform); // move to top layer for dragging
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Return to original position/parent if needed
        transform.SetParent(originalParent);
        canvasGroup.blocksRaycasts = true;
    }
}
