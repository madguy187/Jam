using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItemUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private Button buyButton;

    private ShopItem currentItem;
    private System.Action<ShopItem> onBuyCallback;

    public void Setup(ShopItem item, System.Action<ShopItem> onBuy)
    {
        currentItem = item;
        onBuyCallback = onBuy;

        iconImage.sprite = item.icon;
        nameText.text = item.name;
        costText.text = item.isSold ? "Sold Out" : $"Cost: {item.cost}";
        buyButton.interactable = !item.isSold;

        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(() =>
        {
            onBuy?.Invoke(currentItem);
            Setup(currentItem, onBuy); // Refresh UI after purchase
        });
    }
}
