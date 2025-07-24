using UnityEngine;

public interface IDragHandlerInterface {
    MockItemType GetItemType();
    MockInventoryItem GetDraggedItem();
    void OnDropAccepted();
    void OnDropRejected();
}