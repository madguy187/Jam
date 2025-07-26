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

    private void Start()
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
        if (dragHandler == null) {
            return;
        }
        Global.DEBUG_PRINT("[DropZone::OnDrop] DragHandler found: " + dragHandler.GetType().Name);

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

        if (itemTracker == null) {
            Global.DEBUG_PRINT("[DropZone::OnDrop] ItemTracker is null.");
            dragHandler.OnDropRejected();
            return;
        }

        if (!itemTracker.CanAccept(trackerType, draggedItemType)) {
            Global.DEBUG_PRINT("[DropZone::OnDrop] Cannot drop more items, max limit reached.");
            dragHandler.OnDropRejected();
            return;
        }

        //UnitSettingLayout layout = GetComponentInParent<UnitSettingLayout>();
        UnitSettingLayout layout = UnitSettingLayout.instance;
        if (layout == null) {
            Global.DEBUG_PRINT("[DropZone::OnDrop] layout null.");
        }
        MockPlayerInventory inventory = layout?.ActiveInventory;

        if (inventory == null) {
            Global.DEBUG_PRINT("[DropZone::OnDrop] Inventory is null.");
            dragHandler.OnDropRejected();
            return;
        }

        var draggedItem = dragHandler.GetDraggedItem();

        // === Handle relics ===
        if (draggedItemType == MockItemType.Relic) {
            UnitObject activeUnit = layout?.ActiveUnit;

            if (activeUnit == null) {
                Global.DEBUG_PRINT("[DropZone::OnDrop] No active unit to equip/unequip relic.");
                dragHandler.OnDropRejected();
                return;
            }

            if (trackerType == TrackerType.BagContainer) {
                // Unequip relic
                bool success = activeUnit.RemoveRelic(draggedItem.relicData);
                if (success) {
                    inventory.bagItems.Remove(draggedItem);
                } else {
                    Global.DEBUG_PRINT($"[DropZone::OnDrop] Relic {draggedItem.relicData.name} not found on {activeUnit.name}.");
                    dragHandler.OnDropRejected();
                    return;
                }
            } else if (allowedType == AllowedItemType.RelicsOnly) {
                // Equip relic
                activeUnit.AddRelic(draggedItem.relicData);
                inventory.bagItems.Remove(draggedItem);
            }
        }

        // Handle units
        if (draggedItemType == MockItemType.Unit) {
            if (trackerType == TrackerType.BagContainer) {
                //ItemTracker.Instance.AddItem(TrackerType.BagContainer, MockItemType.Unit, draggedItem);
                //inventory.MoveUnitToBag(draggedItem);
                Global.DEBUG_PRINT($"[DropZone::OnDrop] Moved {draggedItem.unitData.name} to Bag");
            } else if (trackerType == TrackerType.UnitContainer) {
                //ItemTracker.Instance.RemoveItem(TrackerType.BagContainer, MockItemType.Unit, draggedItem);
                //inventory.MoveUnitToTeam(draggedItem);
                Global.DEBUG_PRINT($"[DropZone::OnDrop] Moved {draggedItem.unitData.name} to Team");
            }
        }

        // Snap UI and finalize
        if (!SnapToFirstAvailableSlot(eventData.pointerDrag)) {
            dragHandler.OnDropRejected();
            return;
        }

        LayoutRebuilder.MarkLayoutForRebuild(contentParent.GetComponent<RectTransform>());
        itemTracker.AddItem(trackerType, draggedItemType, draggedItem);
        dragHandler.OnDropAccepted();
    }


    public bool SnapToFirstAvailableSlot(GameObject draggedGO)
    {
        foreach (Transform slot in contentParent)
        {
            if (slot.childCount == 0)
            {
                // Snap to this slot
                draggedGO.transform.SetParent(slot, true);

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
