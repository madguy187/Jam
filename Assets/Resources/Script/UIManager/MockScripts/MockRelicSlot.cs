using UnityEngine;
using UnityEngine.UI;

public class MockRelicSlot : MonoBehaviour, IDragHandlerInterface
{
    public Image iconImage;

    // Hold a reference to the inventory item (can be a relic or unit)
    public MockInventoryItem boundItem;

    // Initialize relic slot with relic and inventory item wrapper
    public void Init(MockRelic relic, MockInventoryItem inventoryItem)
    {
        if (relic == null) return;

        boundItem = inventoryItem;

        if (iconImage != null)
        {
            iconImage.sprite = relic.icon;
            iconImage.enabled = relic.icon != null;
        }
    }

    // IDragHandlerInterface implementations
    public MockItemType GetItemType()
    {
        return MockItemType.Relic;
    }

    public MockInventoryItem GetDraggedItem()
    {
        return boundItem;
    }

    public void OnDropAccepted()
    {
        // Optional: Add visual or state changes here on drop accepted
    }

    public void OnDropRejected()
    {
        // Optional: Reset position or provide feedback on drop rejection
    }
}
