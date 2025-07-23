using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DropZone : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler {
    [SerializeField] public TrackerType trackerType; // Default to BagContainer
    public enum AllowedItemType {
        UnitsOnly,
        RelicsOnly,
        UnitsAndRelics
    }

    public AllowedItemType allowedType = AllowedItemType.UnitsAndRelics;

    public Transform contentParent; // Where the dropped UI elements should be parented

    // Optional visual feedback on pointer enter/exit
    public Color normalColor = Color.white;
    public Color highlightColor = Color.yellow;
    private Image image;
    private ItemTracker itemTracker;

    private void Awake()
    {
        image = GetComponent<Image>();
        if (image != null) { image.color = normalColor; }

        if (contentParent == null) { contentParent = this.transform; }

        if (itemTracker == null) { itemTracker = ItemTracker.Instance; }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (image != null) { image.color = highlightColor; }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (image != null) { image.color = normalColor; }
    }

    public void OnDrop(PointerEventData eventData)
    {
        var dragHandler = eventData.pointerDrag?.GetComponent<IDragHandlerInterface>();
        if (dragHandler == null)
            return;

        var draggedItemType = dragHandler.GetItemType();

        // Strict check: Reject if drop zone is UnitsOnly but dragged item is NOT unit
        if (allowedType == AllowedItemType.UnitsOnly && draggedItemType != MockItemType.Unit) {
            dragHandler.OnDropRejected();
            return;
        }

        // Strict check: Reject if drop zone is RelicsOnly but dragged item is NOT relic
        if (allowedType == AllowedItemType.RelicsOnly && draggedItemType != MockItemType.Relic) {
            dragHandler.OnDropRejected();
            return;
        }

        // Accept UnitsAndRelics for both types
        bool acceptDrop = allowedType == AllowedItemType.UnitsAndRelics &&
                          (draggedItemType == MockItemType.Unit || draggedItemType == MockItemType.Relic);

        if (!acceptDrop && allowedType != AllowedItemType.UnitsOnly && allowedType != AllowedItemType.RelicsOnly) {
            dragHandler.OnDropRejected();
            return;
        }

        if (!itemTracker.CanAccept(trackerType, draggedItemType)) {
            Global.DEBUG_PRINT("[DropZone::OnDrop] Cannot drop more items, max limit reached.");
            dragHandler.OnDropRejected();
            return;
        }

        // Should not fail here but just in case...
        if (!SnapToFirstAvailableSlot(eventData.pointerDrag/*, eventData*/)) {
            dragHandler.OnDropRejected();
            return;
        }

        // If we reach here, we can accept the drop but this is specific for relic slots for a unit
        if (allowedType == AllowedItemType.RelicsOnly && draggedItemType == MockItemType.Relic) {
            // Get the current active unit from the layout
            UnitSettingLayout layout = GetComponentInParent<UnitSettingLayout>();
            if (layout.ActiveUnit != null) {
                layout.ActiveUnit.EquipRelic(dragHandler.GetDraggedItem().relicData);
                Global.DEBUG_PRINT($"Equipped {dragHandler.GetDraggedItem().relicData.relicName} to {layout.ActiveUnit.unitName}");
            }
        }

        // If passed all checks, accept drop
        // eventData.pointerDrag.transform.SetParent(contentParent, false);
        LayoutRebuilder.MarkLayoutForRebuild(contentParent.GetComponent<RectTransform>());
        itemTracker.AddItem(trackerType, draggedItemType);
        dragHandler.OnDropAccepted();
    }

    public bool SnapToFirstAvailableSlot(GameObject draggedGO)
    {
        foreach (Transform slot in contentParent)
        {
            if (slot.childCount == 0)
            {
                // Snap to this slot
                draggedGO.transform.SetParent(slot, false);

                // Stretch to fit
                RectTransform rt = draggedGO.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;

                return true;
            }
        }

        Global.DEBUG_PRINT("[DropZone::SnapToFirstAvailableSlot] No empty slot found.");
        return false;
    }


    // This method attempts to snap the dragged item to the closest empty slot, commenting it first
    // maybe useful for future reference.
    // public bool SnapToClosestEmptySlot(GameObject draggedGO, PointerEventData eventData)
    // {
    //     Vector2 mousePos = eventData.position;
    //     Transform closestSlot = null;
    //     float closestDistance = float.MaxValue;
    //     foreach (Transform slot in contentParent)
    //     {
    //         if (slot.childCount > 0)
    //             continue;
    //         Vector2 slotScreenPos = RectTransformUtility.WorldToScreenPoint(eventData.pressEventCamera, slot.position);
    //         float distance = Vector2.Distance(mousePos, slotScreenPos);
    //         if (distance < closestDistance)
    //         {
    //             closestDistance = distance;
    //             closestSlot = slot;
    //         }
    //     }
    //     if (closestSlot == null)
    //     {
    //         Global.DEBUG_PRINT("[DropZone::SnapToClosestEmptySlot] Snap failed: No available empty slot.");
    //         return false;
    //     }
    //     // Snap into the closest empty slot
    //     draggedGO.transform.SetParent(closestSlot, false);
    //     // Stretch the dragged item to fit the slot
    //     RectTransform rt = draggedGO.GetComponent<RectTransform>();
    //     rt.anchorMin = Vector2.zero;
    //     rt.anchorMax = Vector2.one;
    //     rt.offsetMin = Vector2.zero;
    //     rt.offsetMax = Vector2.zero;
    //     return true;
    // }
}
