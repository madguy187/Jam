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

    private List<ShopItemUI> itemUIs = new List<ShopItemUI>();
    private Dictionary<ShopItem, MockInventoryItem> shopToMockInventoryMap = new Dictionary<ShopItem, MockInventoryItem>();

    private int currentGold = 100;

    [Header("Shop Settings")]
    public int refreshCost = 2;
    public float randomizeValue = 0.5f; // Adjust this to control the randomness of item selection

    private void Start()
    {
        if (MockPlayerInventoryHolder.Instance == null) {
            Global.DEBUG_PRINT("[ShopManager::Start] MockPlayerInventoryHolder instance is null!");
        } else {
            currentGold = MockPlayerInventoryHolder.Instance.playerInventory.gold;
        }
        var refreshButtonText = refreshButton.GetComponentInChildren<TextMeshProUGUI>();
        refreshButtonText.text = $"Refresh ({refreshCost}g)";
        refreshButton.onClick.AddListener(OnRefreshClicked);
        GenerateShopItems();
        UpdateGoldUI();
    }

    void UpdateGoldUI()
    {
        goldText.text = $"Gold: {currentGold}";
    }

    void OnRefreshClicked()
    {
        if (currentGold < refreshCost) { return; }
        currentGold -= refreshCost;
        if (MockPlayerInventoryHolder.Instance != null) {
            MockPlayerInventoryHolder.Instance.playerInventory.gold = currentGold;
        }
        GenerateShopItems();
        UpdateGoldUI();
    }

    void GenerateShopItems()
    {
        Global.DEBUG_PRINT("[ShopManager::GenerateShopItems] Generating shop items...");
        for (int i = 0; i < itemUIs.Count; i++)
        {
            if (!itemUIs[i].Item.isSold)
            {
                ShopItem newItem = GetRandomItem();
                itemUIs[i].Setup(newItem, OnItemBought);
            }
            // Keep sold-out items unchanged
        }

        // Fill up any empty slots (in case initial shop has fewer than 3)
        while (itemUIs.Count < 3)
        {
            ShopItem newItem = GetRandomItem();
            var itemUI = Instantiate(itemPrefab, itemContainer);
            itemUI.Setup(newItem, OnItemBought);
            itemUIs.Add(itemUI);
        }
    }

    ShopItem GetRandomItem()
    {
        // Randomly pick either a relic or a unit
        bool isRelic = Random.value > randomizeValue;
        var resourceManager = ResourceManager.instance;
        if (resourceManager == null) {
            Global.DEBUG_PRINT("[ShopManager::GetRandomItem] ResourceManager instance is null!");
            return null; // Handle error appropriately
        }

        if (isRelic)
        {
            var relicSO = resourceManager.Debug_RandRelic();
            var shopItem = new ShopItem(
                relicSO.GetRelicName(),
                relicSO.GetRelicDescription(),
                ShopItemType.Relic,
                relicSO.GetRelicCost(),
                relicSO.GetRelicSprite()
            );
            shopToMockInventoryMap[shopItem] = new MockInventoryItem(relicSO);
            return shopItem;
        }
        else
        {
            string unitKey = resourceManager.Debug_RandUnit();
            GameObject unitPrefab = resourceManager.GetUnit(unitKey);
            if (unitPrefab == null) {
                Global.DEBUG_PRINT($"[ShopManager::GetRandomItem] Unit prefab not found for key: {unitKey}");
                return null;
            }
            // Instantiate unit temporarily in the scene
            GameObject unitInstance = GameObject.Instantiate(unitPrefab);
            unitInstance.SetActive(true);
            UnitObject unitObj = unitInstance.GetComponent<UnitObject>();
            var so = unitObj.unitSO;
            Sprite unitIcon = GetUnitSprite(unitInstance);
    
            var shopItem = new ShopItem(
                so.GetUnitName(),
                so.GetUnitDescription(),
                ShopItemType.Unit,
                so.GetUnitCost(),
                unitIcon
            );
            shopToMockInventoryMap[shopItem] = new MockInventoryItem(unitObj);
            Destroy(unitInstance); // Clean up the temporary unit instance
            return shopItem;
        }
    }

    Sprite GetUnitSprite(GameObject unit)
    {
        RenderTexture tex;
        GameObject camObj;
        var unitRoot = unit.transform.Find("UnitRoot");
        if (unitRoot == null) {
            Global.DEBUG_PRINT("[ShopManager::GetUnitSprite] UnitRoot not found on unitPrefab!");
            GameObject.DestroyImmediate(unit);
            return null; // Or handle error properly
        }
        (tex, camObj) = RenderUtilities.RenderUnitToTexture(unitRoot.gameObject, 1.25f);
        Sprite unitIcon = RenderUtilities.ConvertRenderTextureToSprite(tex);
        Destroy(camObj);
        Destroy(tex);
        return unitIcon;
    }

    void OnItemBought(ShopItem item)
    {
        if (item.isSold || currentGold < item.cost) { return; }
        currentGold -= item.cost;
        item.isSold = true;
        UpdateGoldUI();

        if (ItemTracker.Instance == null)
        {
            Global.DEBUG_PRINT("[ShopManager::OnItemBought] ItemTracker instance is null!");
        } else {
            // For unit or relic
            if (item.type == ShopItemType.Unit) {
                ItemTracker.Instance.AddItem(TrackerType.BagContainer, MockItemType.Unit, shopToMockInventoryMap[item]);
            } else {
                ItemTracker.Instance.AddItem(TrackerType.BagContainer, MockItemType.Relic, shopToMockInventoryMap[item]);
            }
            Global.DEBUG_PRINT($"[ShopManager::OnClaimPressed] Added {item.name} to inventory.");
        }

        // Hide refresh button if all items are sold out
        bool allSoldOut = itemUIs.TrueForAll(ui => ui.Item.isSold);
        refreshButton.gameObject.SetActive(!allSoldOut);
    }

#if UNITY_EDITOR
    [ContextMenu("Manual Refresh Shop Items")]
    public void EditorRefresh()
    {
        GenerateShopItems();
    }
#endif
}
