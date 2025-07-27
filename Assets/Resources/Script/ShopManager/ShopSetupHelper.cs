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

        GameObject leftPanel = new GameObject("LeftPanel", typeof(RectTransform));
        leftPanel.transform.SetParent(canvas.transform);
        RectTransform leftRT = leftPanel.GetComponent<RectTransform>();
        leftRT.anchorMin = new Vector2(0f, 0f);
        leftRT.anchorMax = new Vector2(0.25f, 1f);
        leftRT.offsetMin = Vector2.zero;
        leftRT.offsetMax = Vector2.zero;

        VerticalLayoutGroup leftLayout = leftPanel.AddComponent<VerticalLayoutGroup>();
        leftLayout.childAlignment = TextAnchor.MiddleCenter;
        leftLayout.spacing = 20;

        GameObject goldTextGO = CreateTMP("GoldText", "Gold: 100", new Vector2(200, 50), leftPanel.transform);
        shop.goldText = goldTextGO.GetComponent<TextMeshProUGUI>();

        GameObject refreshButtonGO = CreateButton("Refresh", "Refresh (5g)", leftPanel.transform, new Vector2(200, 60));
        shop.refreshButton = refreshButtonGO.GetComponent<Button>();

        GameObject centerPanel = new GameObject("CenterPanel", typeof(RectTransform));
        centerPanel.transform.SetParent(canvas.transform);
        RectTransform centerRT = centerPanel.GetComponent<RectTransform>();
        centerRT.anchorMin = new Vector2(0.25f, 0f);
        centerRT.anchorMax = new Vector2(1f, 1f);
        centerRT.offsetMin = Vector2.zero;
        centerRT.offsetMax = Vector2.zero;

        HorizontalLayoutGroup centerLayout = centerPanel.AddComponent<HorizontalLayoutGroup>();
        centerLayout.childAlignment = TextAnchor.MiddleCenter;
        centerLayout.spacing = 40;
        centerLayout.padding = new RectOffset(40, 40, 40, 40);

        shop.itemContainer = centerPanel.transform;

        GameObject itemPrefabGO = new GameObject("ShopItemUI", typeof(ShopItemUI), typeof(Image));
        itemPrefabGO.GetComponent<Image>().color = new Color(0.8f, 0.8f, 0.8f, 1f);
        RectTransform itemRT = itemPrefabGO.GetComponent<RectTransform>();
        itemRT.sizeDelta = new Vector2(300, 520);

        VerticalLayoutGroup itemLayout = itemPrefabGO.AddComponent<VerticalLayoutGroup>();
        itemLayout.childAlignment = TextAnchor.UpperCenter;
        itemLayout.spacing = 10;
        itemLayout.padding = new RectOffset(10, 10, 10, 10);

        GameObject icon = new GameObject("Icon", typeof(Image));
        icon.transform.SetParent(itemPrefabGO.transform);
        RectTransform iconRT = icon.GetComponent<RectTransform>();
        iconRT.sizeDelta = new Vector2(150, 150);
        iconRT.localScale = Vector3.one;

        GameObject descriptionBox = new GameObject("DescriptionBox", typeof(RectTransform));
        descriptionBox.transform.SetParent(itemPrefabGO.transform);
        VerticalLayoutGroup descLayout = descriptionBox.AddComponent<VerticalLayoutGroup>();
        descLayout.childAlignment = TextAnchor.UpperCenter;
        descLayout.spacing = 5;

        GameObject nameText = CreateTMP("NameText", "Item Name", new Vector2(260, 30), descriptionBox.transform);
        GameObject descText = CreateTMP("DescText", "Item description goes here...", new Vector2(260, 60), descriptionBox.transform);

        GameObject buyButtonGO = CreateButton("Buy", "Buy for 10g", itemPrefabGO.transform, new Vector2(220, 50));

        var itemUI = itemPrefabGO.GetComponent<ShopItemUI>();
        itemUI.GetType().GetField("iconImage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(itemUI, icon.GetComponent<Image>());
        itemUI.GetType().GetField("nameText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(itemUI, nameText.GetComponent<TextMeshProUGUI>());
        itemUI.GetType().GetField("costText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(itemUI, descText.GetComponent<TextMeshProUGUI>());
        itemUI.GetType().GetField("buyButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(itemUI, buyButtonGO.GetComponent<Button>());

        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }
        PrefabUtility.SaveAsPrefabAsset(itemPrefabGO, "Assets/Prefabs/ShopItemUI.prefab");
        shop.itemPrefab = AssetDatabase.LoadAssetAtPath<ShopItemUI>("Assets/Prefabs/ShopItemUI.prefab");

        Object.DestroyImmediate(itemPrefabGO);
    }

    static GameObject CreateTMP(string name, string text, Vector2 size, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = size;
        rt.localScale = Vector3.one;
        var tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 24;
        tmp.alignment = TextAlignmentOptions.Center;
        return go;
    }

    static GameObject CreateButton(string name, string buttonText, Transform parent, Vector2 size)
    {
        GameObject go = new GameObject(name + "Button", typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = size;
        rt.localScale = Vector3.one;

        GameObject label = CreateTMP("Text", buttonText, size, go.transform);
        return go;
    }
}
#endif
