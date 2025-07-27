#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RelicForgeSceneCreator
{
    [MenuItem("Tools/Create Relic Forge Scene Layout")]
    public static void CreateRelicForgeLayout()
    {
        Canvas canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasGO = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
        }

        GameObject CreatePanel(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 size, Vector2 anchoredPosition, Color bgColor)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.sizeDelta = size;
            rt.anchoredPosition = anchoredPosition;
            go.GetComponent<Image>().color = bgColor;
            return go;
        }

        GameObject CreateButton(string name, Transform parent, Vector2 size, Vector2 anchoredPosition, string buttonText)
        {
            GameObject btnGO = new GameObject(name, typeof(RectTransform), typeof(Button), typeof(Image));
            btnGO.transform.SetParent(parent, false);
            RectTransform rt = btnGO.GetComponent<RectTransform>();
            rt.sizeDelta = size;
            rt.anchoredPosition = anchoredPosition;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.zero;
            rt.pivot = new Vector2(0.5f, 0.5f);
            btnGO.GetComponent<Image>().color = new Color(0.85f, 0.85f, 0.85f, 1f);

            GameObject textGO = new GameObject("TMPText", typeof(TextMeshProUGUI));
            textGO.transform.SetParent(btnGO.transform, false);
            TextMeshProUGUI tmp = textGO.GetComponent<TextMeshProUGUI>();
            tmp.text = buttonText;
            tmp.fontSize = 24;
            tmp.color = Color.black;
            tmp.alignment = TextAlignmentOptions.Center;

            RectTransform textRT = textGO.GetComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.offsetMin = Vector2.zero;
            textRT.offsetMax = Vector2.zero;

            return btnGO;
        }

        GameObject CreateTMPLabel(string text, Transform parent, int fontSize = 24)
        {
            GameObject go = new GameObject("TMPLabel", typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            TextMeshProUGUI tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = Color.black;
            tmp.alignment = TextAlignmentOptions.Center;
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return go;
        }

        // --- Top Left Merge/Break Buttons ---
        GameObject topLeft = CreatePanel("TopLeftPanel", canvas.transform,
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1),
            new Vector2(300, 80), new Vector2(160, -40), new Color(1, 1, 1, 0.1f));

        CreateButton("MergeButton", topLeft.transform, new Vector2(120, 40), new Vector2(70, -20), "Merge");
        CreateButton("BreakButton", topLeft.transform, new Vector2(120, 40), new Vector2(230, -20), "Break");

        // --- Center Forge Area ---
        GameObject center = CreatePanel("ForgeArea", canvas.transform,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(600, 200), Vector2.zero, new Color(1f, 1f, 1f, 0.1f));

        CreateTMPLabel("Relic X + Relic Y = Relic Z", center.transform, 30);

        GameObject forgeBtn = CreateButton("ForgeButton", center.transform, new Vector2(200, 50), new Vector2(0, -60), "Forge");

        // --- Bottom Left NPC Sprite Placeholder ---
        GameObject npc = CreatePanel("NPCSprite", canvas.transform,
            new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0),
            new Vector2(200, 200), new Vector2(100, 100), new Color(0.9f, 0.9f, 1f, 0.4f));
        CreateTMPLabel("NPC", npc.transform, 20);

        // --- Bottom Right Bag Grid ---
        GameObject bagPanel = CreatePanel("BagPanel", canvas.transform,
            new Vector2(1, 0), new Vector2(1, 0), new Vector2(1, 0),
            new Vector2(400, 400), new Vector2(-220, 100), new Color(1, 0.95f, 0.8f, 0.2f));

        GridLayoutGroup grid = bagPanel.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(80, 80);
        grid.spacing = new Vector2(10, 10);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 3;

        for (int i = 0; i < 9; i++)
        {
            GameObject slot = CreatePanel($"BagSlot{i + 1}", bagPanel.transform,
                Vector2.zero, Vector2.zero, new Vector2(0.5f, 0.5f),
                new Vector2(80, 80), Vector2.zero, new Color(1, 1, 1, 0.3f));
            CreateTMPLabel((i + 1).ToString(), slot.transform, 18);
        }

        Debug.Log("✅ Relic Forge Scene layout created successfully with TMP buttons.");
    }
}
#endif
