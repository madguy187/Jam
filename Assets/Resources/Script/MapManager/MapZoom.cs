using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MapZoom : MonoBehaviour, IScrollHandler, IPointerEnterHandler, IPointerExitHandler {
    public ScrollRect scrollRect;     // Assign ScrollRect component
    public RectTransform content;     // Assign content (inside ScrollRect)
    public float zoomSpeed = 0.1f;
    public float maxScale = 2f;

    private bool pointerOver = false;

    public void OnPointerEnter(PointerEventData eventData) {
        pointerOver = true;
    }

    public void OnPointerExit(PointerEventData eventData) {
        pointerOver = false;
    }

    public void OnScroll(PointerEventData eventData) {
        if (!pointerOver) return;

        float currentScale = content.localScale.x;
        float scrollDelta = eventData.scrollDelta.y * zoomSpeed;
        float targetScale = currentScale + scrollDelta;

        // Add padding to prevent small visual gaps at edges
        float padding = 10f;
        float minScaleX = (scrollRect.viewport.rect.width + padding) / content.rect.width;
        float minScaleY = (scrollRect.viewport.rect.height + padding) / content.rect.height;
        float minScale = Mathf.Max(minScaleX, minScaleY) * 1.02f; // Ensure it's a bit larger

        targetScale = Mathf.Clamp(targetScale, minScale, maxScale);
        if (Mathf.Approximately(targetScale, currentScale)) return;

        // Keep zoom centered under mouse
        Vector2 localCursor;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            scrollRect.viewport, eventData.position, eventData.pressEventCamera, out localCursor);

        Vector2 pivot = content.pivot;
        Vector2 contentPos = content.anchoredPosition;
        Vector2 contentSize = content.rect.size;

        Vector2 prevPivotLocalPos = new Vector2(
            (localCursor.x - contentPos.x) / (contentSize.x * currentScale),
            (localCursor.y - contentPos.y) / (contentSize.y * currentScale)
        );

        Vector2 newPivot = new Vector2(
            Mathf.Clamp01(pivot.x + (prevPivotLocalPos.x - pivot.x) * (1 - currentScale / targetScale)),
            Mathf.Clamp01(pivot.y + (prevPivotLocalPos.y - pivot.y) * (1 - currentScale / targetScale))
        );

        content.pivot = newPivot;
        content.localScale = new Vector3(targetScale, targetScale, 1f);

        Vector2 pivotDelta = content.pivot - pivot;
        Vector2 positionDelta = new Vector2(
            pivotDelta.x * content.rect.width * targetScale,
            pivotDelta.y * content.rect.height * targetScale
        );

        content.anchoredPosition -= positionDelta;

        ClampContentPosition();
        eventData.Use();
    }

    private void ClampContentPosition() {
        Vector2 contentSizeScaled = new Vector2(
            content.rect.width * content.localScale.x,
            content.rect.height * content.localScale.y
        );

        Vector2 viewportSize = scrollRect.viewport.rect.size;
        Vector2 clampedPosition = content.anchoredPosition;

        // Horizontal
        if (contentSizeScaled.x <= viewportSize.x) {
            clampedPosition.x = (viewportSize.x - contentSizeScaled.x) / 2f;
        } else {
            float maxX = 0f;
            float minX = viewportSize.x - contentSizeScaled.x;
            clampedPosition.x = Mathf.Clamp(clampedPosition.x, minX, maxX);
        }

        // Vertical
        if (contentSizeScaled.y <= viewportSize.y) {
            clampedPosition.y = (viewportSize.y - contentSizeScaled.y) / 2f;
        } else {
            float maxY = 0f;
            float minY = viewportSize.y - contentSizeScaled.y;
            clampedPosition.y = Mathf.Clamp(clampedPosition.y, minY, maxY);
        }

        // Snap to full pixel values
        content.anchoredPosition = new Vector2(
            Mathf.Round(clampedPosition.x),
            Mathf.Round(clampedPosition.y)
        );
    }

}
