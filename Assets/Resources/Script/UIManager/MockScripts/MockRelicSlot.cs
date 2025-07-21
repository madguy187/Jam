using UnityEngine;
using UnityEngine.UI;

public class MockRelicSlot : MonoBehaviour
{
    public Image iconImage;

    public void Init(MockRelic relic)
    {
        if (relic == null) return;

        if (iconImage != null)
        {
            iconImage.sprite = relic.icon; // Make sure your MockRelic has a sprite icon field
            iconImage.enabled = relic.icon != null;
        }
    }
}
