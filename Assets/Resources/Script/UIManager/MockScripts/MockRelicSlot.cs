using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MockRelicSlot : MonoBehaviour, IPointerClickHandler 
{
    [Header("UI References")]
    public Image iconImage;
    public Image borderImage;
    public GameObject highlightObject;

    private MockRelic relicData;
    private System.Action<MockRelic> onClick;

    public void Init(MockRelic relic, System.Action<MockRelic> onClickCallback = null) 
    {
        relicData = relic;
        onClick = onClickCallback;

        if (iconImage != null) {
            iconImage.sprite = relic.icon;
        }
        if (borderImage != null) {
            borderImage.color = GetRarityColor(relic.rarity);
        }
        if (highlightObject != null) {
            highlightObject.SetActive(false);
        }
    }

    public void OnPointerClick(PointerEventData eventData) 
    {
        if (highlightObject != null) {
            highlightObject.SetActive(true); // Or toggle
        }
        // Invoke the callback if set
        onClick?.Invoke(relicData);
    }

    private Color GetRarityColor(MockRelicRarity rarity) {
        return rarity switch {
            MockRelicRarity.Common => Color.gray,
            MockRelicRarity.Rare => Color.blue,
            MockRelicRarity.Epic => new Color(0.6f, 0.2f, 0.8f),
            MockRelicRarity.Legendary => Color.yellow,
            _ => Color.white
        };
    }

    public MockRelic GetRelicData() => relicData;
}
