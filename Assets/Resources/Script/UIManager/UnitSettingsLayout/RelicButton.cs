using UnityEngine;
using UnityEngine.UI;

public class RelicButton : MonoBehaviour
{
    public Image iconImage;

    // Hold a reference to the inventory item (can be a relic or unit)
    public MockInventoryItem boundItem;

    // Initialize relic slot with relic and inventory item wrapper
    public void Init(RelicScriptableObject relic, MockInventoryItem inventoryItem)
    {
        if (relic == null) return;

        boundItem = inventoryItem;

        if (iconImage != null)
        {
            iconImage.sprite = relic.GetRelicSprite();
            iconImage.enabled = relic.GetRelicSprite() != null;
        }
    }
}
