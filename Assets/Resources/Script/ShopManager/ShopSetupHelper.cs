#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class ShopSetupHelper
{
    [MenuItem("Tools/Create Sample Shop Scene")]
    public static void CreateShopScene()
    {
        var canvas = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvas.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        GameObject shopGO = new GameObject("ShopManager", typeof(ShopManager));
        ShopManager shop = shopGO.GetComponent<ShopManager>();

        GameObject goldTextGO = CreateTMP("GoldText", "Gold: 100", new Vector2(200, 50), canvas.transform, new Vector2(0, 0));
        shop.goldText = goldTextGO.GetComponent<TextMeshProUGUI>();

        GameObject refreshButtonGO = CreateButton("Refresh", canvas.transform, new Vector2(160, 50), new Vector2(0, -60));
        shop.refreshButton = refreshButtonGO.GetComponent<Button>();

        GameObject container = new GameObject("ItemContainer", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        container.transform.SetParent(canvas.transform);
        RectTransform containerRT = container.GetComponent<RectTransform>();
        containerRT.sizeDelta = new Vector2(800, 400);
        containerRT.anchoredPosition = new Vector2(500, 0);
        containerRT.localScale = Vector3.one;

        shop.itemContainer = container.transform;

        GameObject itemPrefabGO = new GameObject("ShopItemUI", typeof(ShopItemUI), typeof(Image));
        var itemUI = itemPrefabGO.GetComponent<ShopItemUI>();

        GameObject icon = new GameObject("Icon", typeof(Image));
        icon.transform.SetParent(itemPrefabGO.transform);
        itemUI.GetType().GetField("iconImage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(itemUI, icon.GetComponent<Image>());

        GameObject nameText = CreateTMP("NameText", "Name", new Vector2(200, 30), itemPrefabGO.transform, Vector2.zero);
        itemUI.GetType().GetField("nameText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(itemUI, nameText.GetComponent<TextMeshProUGUI>());

        GameObject costText = CreateTMP("CostText", "Cost", new Vector2(200, 30), itemPrefabGO.transform, Vector2.zero);
        itemUI.GetType().GetField("costText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(itemUI, costText.GetComponent<TextMeshProUGUI>());

        GameObject buyButtonGO = CreateButton("Buy", itemPrefabGO.transform, new Vector2(150, 40), Vector2.zero);
        itemUI.GetType().GetField("buyButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(itemUI, buyButtonGO.GetComponent<Button>());

        PrefabUtility.SaveAsPrefabAsset(itemPrefabGO, "Assets/Prefabs/ShopItemUI.prefab");
        shop.itemPrefab = AssetDatabase.LoadAssetAtPath<ShopItemUI>("Assets/Prefabs/ShopItemUI.prefab");

        Object.DestroyImmediate(itemPrefabGO);
    }

    static GameObject CreateTMP(string name, string text, Vector2 size, Transform parent, Vector2 anchoredPos)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = size;
        rt.anchoredPosition = anchoredPos;
        var tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 24;
        return go;
    }

    static GameObject CreateButton(string text, Transform parent, Vector2 size, Vector2 pos)
    {
        GameObject go = new GameObject(text + "Button", typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = size;
        rt.anchoredPosition = pos;
        rt.localScale = Vector3.one;

        GameObject label = CreateTMP("Text", text, size, go.transform, Vector2.zero);
        return go;
    }
}
#endif
