using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItemUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descText;
    [SerializeField] private Button buyButton;
    [SerializeField] private TextMeshProUGUI buyButtonText;
    [SerializeField] private GameObject overlay;

    private ShopItem currentItem;
    private System.Action<ShopItem> onBuyCallback;
    public ShopItem Item => currentItem;

    public void Setup(ShopItem item, System.Action<ShopItem> onBuy)
    {
        currentItem = item;
        onBuyCallback = onBuy;

        iconImage.sprite = item.icon;
        nameText.text = item.name;
        descText.text = item.description;

        if (item.isSold) {
            buyButtonText.text = "Sold Out";
            buyButton.interactable = false;
            if (overlay != null) {
                overlay.SetActive(true);  // show grey overlay
            }
        } else {
            buyButtonText.text = $"Buy ({item.cost}g)";
            buyButton.interactable = true;
            if (overlay != null) {
                overlay.SetActive(false);  // hide grey overlay
            }
        }

        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(() =>
        {
            onBuy?.Invoke(currentItem);
            Setup(currentItem, onBuy); // Refresh UI after purchase
        });
    }
}
