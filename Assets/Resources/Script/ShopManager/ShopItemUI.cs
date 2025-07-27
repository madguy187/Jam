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

    private ShopItem currentItem;
    private System.Action<ShopItem> onBuyCallback;

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
        } else {
            buyButtonText.text = $"Buy ({item.cost})";
            buyButton.interactable = true;
        }

        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(() =>
        {
            onBuy?.Invoke(currentItem);
            Setup(currentItem, onBuy); // Refresh UI after purchase
        });
    }
}
