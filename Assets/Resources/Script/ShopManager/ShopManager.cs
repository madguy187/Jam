using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ShopManager : MonoBehaviour
{
    public TextMeshProUGUI goldText;
    public Button refreshButton;
    public Transform itemContainer;
    public ShopItemUI itemPrefab;

    public List<ShopItem> relicPool = new();
    public List<ShopItem> unitPool = new();

    private List<ShopItemUI> itemUIs = new();

    private int currentGold = 100;

    [Header("Shop Settings")]
    public int refreshCost = 10;
    public float randomizeValue = 0.5f; // Adjust this to control the randomness of item selection

    private void Start() {
        refreshButton.onClick.AddListener(OnRefreshClicked);
        GenerateDummyItems();
        GenerateShopItems();
        UpdateGoldUI();
    }

    void GenerateDummyItems()
    {
        // Replace with your actual icons
        Sprite dummyIcon1 = Resources.Load<Sprite>("Sprites/game-icons.net/brute");
        Sprite dummyIcon2 = Resources.Load<Sprite>("Sprites/game-icons.net/monk-face");
        Sprite dummyIcon3 = Resources.Load<Sprite>("Sprites/game-icons.net/mounted-knight");
        Sprite dummyIcon4 = Resources.Load<Sprite>("Sprites/game-icons.net/orc-head");

        relicPool.Add(new ShopItem("Relic of Strength", "So strong", ItemType.Relic, 30, dummyIcon1));
        relicPool.Add(new ShopItem("Relic of Speed", "So speedy", ItemType.Relic, 25, dummyIcon2));
        unitPool.Add(new ShopItem("Warrior", "A brave warrior", ItemType.Unit, 40, dummyIcon3));
        unitPool.Add(new ShopItem("Archer", "A skilled archer", ItemType.Unit, 35, dummyIcon4));
    }

    void UpdateGoldUI()
    {
        goldText.text = $"Gold: {currentGold}";
    }

    void OnRefreshClicked()
    {
        if (currentGold < refreshCost) { return; }
        currentGold -= refreshCost;
        GenerateShopItems();
        UpdateGoldUI();
    }

    void GenerateShopItems()
    {
        foreach (Transform child in itemContainer) {
            Destroy(child.gameObject); 
        }
        itemUIs.Clear();

        for (int i = 0; i < 3; i++) {
            ShopItem item = GetRandomItem();
            var itemUI = Instantiate(itemPrefab, itemContainer);
            itemUI.Setup(item, OnItemBought);
            itemUIs.Add(itemUI);
        }
    }

    ShopItem GetRandomItem()
    {
        bool isRelic = Random.value > randomizeValue;
        var pool = isRelic ? relicPool : unitPool;
        return new ShopItem(pool[Random.Range(0, pool.Count)]);
    }

    void OnItemBought(ShopItem item)
    {
        if (item.isSold || currentGold < item.cost) { return; }
        currentGold -= item.cost;
        item.isSold = true;
        UpdateGoldUI();
    }

#if UNITY_EDITOR
    [ContextMenu("Manual Refresh Shop Items")]
    public void EditorRefresh()
    {
        GenerateShopItems();
    }
#endif
}
