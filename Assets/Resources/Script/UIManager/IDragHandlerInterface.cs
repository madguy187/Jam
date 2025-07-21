using UnityEngine.EventSystems;

public interface IDragHandlerInterface
{
    MockItemType GetItemType();
    MockInventoryItem GetDraggedItem();
    void OnDropAccepted();
    void OnDropRejected();
}