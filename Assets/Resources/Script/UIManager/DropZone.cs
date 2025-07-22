using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DropZone : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    public enum AllowedItemType
    {
        UnitsOnly,
        RelicsOnly,
        UnitsAndRelics
    }

    public AllowedItemType allowedType = AllowedItemType.UnitsAndRelics;

    public Transform contentParent; // Where the dropped UI elements should be parented

    // Optional visual feedback on pointer enter/exit
    public Color normalColor = Color.white;
    public Color highlightColor = Color.yellow;
    private UnityEngine.UI.Image image;
    private ItemTracker itemTracker;

    private void Awake() {
        image = GetComponent<UnityEngine.UI.Image>();
        if (image != null)
            image.color = normalColor;

        if (contentParent == null)
            contentParent = this.transform;
            
        if (itemTracker == null)
            itemTracker = GetComponent<ItemTracker>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (image != null)
            image.color = highlightColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (image != null)
            image.color = normalColor;
    }

    public void OnDrop(PointerEventData eventData)
    {
        var dragHandler = eventData.pointerDrag?.GetComponent<IDragHandlerInterface>();
        if (dragHandler == null)
            return;
    
        var draggedItemType = dragHandler.GetItemType();
    
        // Strict check: Reject if drop zone is UnitsOnly but dragged item is NOT unit
        if (allowedType == AllowedItemType.UnitsOnly && draggedItemType != MockItemType.Unit)
        {
            dragHandler.OnDropRejected();
            return;
        }
    
        // Strict check: Reject if drop zone is RelicsOnly but dragged item is NOT relic
        if (allowedType == AllowedItemType.RelicsOnly && draggedItemType != MockItemType.Relic)
        {
            dragHandler.OnDropRejected();
            return;
        }
    
        // Accept UnitsAndRelics for both types
        bool acceptDrop = allowedType == AllowedItemType.UnitsAndRelics &&
                          (draggedItemType == MockItemType.Unit || draggedItemType == MockItemType.Relic);
    
        if (!acceptDrop && allowedType != AllowedItemType.UnitsOnly && allowedType != AllowedItemType.RelicsOnly)
        {
            dragHandler.OnDropRejected();
            return;
        }
    
        if (!itemTracker.CanAccept(draggedItemType))
        {
            Global.DEBUG_PRINT("Cannot drop more items, max limit reached.");
            dragHandler.OnDropRejected();
            return;
        }
    
        // If passed all checks, accept drop
        eventData.pointerDrag.transform.SetParent(contentParent, false);
        LayoutRebuilder.MarkLayoutForRebuild(contentParent.GetComponent<RectTransform>());
        itemTracker.AddItem(draggedItemType);
        dragHandler.OnDropAccepted();
    }
}
