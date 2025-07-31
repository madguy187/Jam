using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MatchHelpScreen : MonoBehaviour
{
    public static MatchHelpScreen instance;
    [SerializeField] private Button toggleButton;
    [Header("Layout Config")]
    [SerializeField] float verticalSpacing = 10f;
    [SerializeField] float iconTextSpacing = 4f;
    [SerializeField] float iconCellSize = 22f;
    [SerializeField] int leftMargin = 100;
    [SerializeField] float patternColumnWidth = 350f;
    [SerializeField] GameObject panel;    
    [SerializeField] Transform patternUIprefab;
    [SerializeField] Text resultsTextUI; 
    private GameObject panelRoot; 
    private Transform patternRoot;

    void Awake()
    {
        if (instance == null) 
        {
            instance = this; 
        }
        else 
        {
            Destroy(this);
        }
    }

    void Start()
    {
        if (toggleButton != null)
        {
            toggleButton.onClick.AddListener(Toggle);
        }

        if (panel != null)
        {
            panelRoot = panel;

            foreach (Transform child in panelRoot.transform)
                Destroy(child.gameObject);

            BuildContentInsideExistingPanel(panelRoot.transform);
            panelRoot.SetActive(false);
            return;
        }
    }

    public void Toggle()
    {
        if (panelRoot == null) return;
        panelRoot.SetActive(!panelRoot.activeSelf);
    }

    public void Show()  { if(panelRoot!=null) panelRoot.SetActive(true);} 
    public void Hide()  { if(panelRoot!=null) panelRoot.SetActive(false);} 

    public void DisplaySpinResults(SpinResult result)
    {
        if (result == null || resultsTextUI == null) return;

        var sb = new System.Text.StringBuilder("<b>LAST SPIN RESULTS</b>\n\n");
        foreach (var kvp in result.GetMatchesByType())
        {
            sb.AppendLine($"{kvp.Key}: {kvp.Value.Count} match(es)");
            foreach (var m in kvp.Value)
                sb.AppendLine($"    • {m.GetSymbol()} ({m.GetArchetype()})");

            sb.AppendLine(); 
        }
        resultsTextUI.text = sb.ToString();
    }

    void BuildHelpUI()
    {
        // Scroll view to allow future expansion
        GameObject scrollGO = new GameObject("ScrollView");
        scrollGO.transform.SetParent(panelRoot.transform, false);
        RectTransform scrollRT = scrollGO.AddComponent<RectTransform>();
        scrollRT.anchorMin = new Vector2(0, 0);
        scrollRT.anchorMax = new Vector2(1, 1);
        scrollRT.offsetMin = new Vector2(10, 10);
        scrollRT.offsetMax = new Vector2(-10, -10);

        ScrollRect scroll = scrollGO.AddComponent<ScrollRect>();
        scroll.horizontal = false;
        Image scrollBg = scrollGO.AddComponent<Image>();
        scrollBg.color = new Color(0, 0, 0, 0); // invisible background

        // Content holder
        GameObject contentGO = new GameObject("Content");
        contentGO.transform.SetParent(scrollRT, false);
        RectTransform contentRT = contentGO.AddComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0, 1);
        contentRT.anchorMax = new Vector2(1, 1);
        contentRT.pivot = new Vector2(0.5f, 1);
        contentRT.anchoredPosition = Vector2.zero;
        scroll.content = contentRT;

        VerticalLayoutGroup vlg = contentGO.AddComponent<VerticalLayoutGroup>();
        vlg.childAlignment = TextAnchor.UpperLeft;
        vlg.padding = new RectOffset(leftMargin, 20, 20, 20);
        vlg.spacing = verticalSpacing;
        ContentSizeFitter fitter = contentGO.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Main horizontal layout inside panel
        GameObject mainRow = new GameObject("MainRow");
        mainRow.transform.SetParent(panelRoot.transform, false);
        HorizontalLayoutGroup mainHL = mainRow.AddComponent<HorizontalLayoutGroup>();
        mainHL.childAlignment = TextAnchor.UpperLeft;
        mainHL.spacing = 30;
        mainHL.padding = new RectOffset(40,40,40,40);

        // Left pattern column 
        GameObject patternsGO = new GameObject("Patterns");
        patternsGO.transform.SetParent(mainRow.transform, false);
        VerticalLayoutGroup patVLG = patternsGO.AddComponent<VerticalLayoutGroup>();
        patVLG.childAlignment = TextAnchor.UpperLeft;
        patVLG.spacing = verticalSpacing;
        patternRoot = patternsGO.transform;

        // Fix width so results start after this column
        var patLE = patternsGO.AddComponent<LayoutElement>();
        patLE.preferredWidth = patternColumnWidth;
        patLE.minWidth = patternColumnWidth;

        // Header for patterns
        AddText(patternRoot, "MATCH PATTERNS", 24, FontStyle.Bold, Color.white, false);

        BuildPatternRows(patternRoot);

        // Right results column 
        GameObject resultsGO = new GameObject("Results");
        resultsGO.transform.SetParent(mainRow.transform, false);
        VerticalLayoutGroup resVLG = resultsGO.AddComponent<VerticalLayoutGroup>();
        resVLG.childAlignment = TextAnchor.UpperLeft;
        resVLG.spacing = verticalSpacing;
        resVLG.padding = new RectOffset(0,20,0,0);
        // placeholder initial text
        resultsTextUI = AddText(resultsGO.transform, "LAST SPIN RESULTS", 20, FontStyle.Bold, Color.white, false);
    }

    void BuildContentInsideExistingPanel(Transform root)
    {
        GameObject mainRow = new GameObject("MainRow");
        mainRow.transform.SetParent(root, false);
        RectTransform mrRT = mainRow.AddComponent<RectTransform>();
        mrRT.anchorMin = new Vector2(0, 0);
        mrRT.anchorMax = new Vector2(1, 1);
        mrRT.offsetMin = new Vector2(40, 40);
        mrRT.offsetMax = new Vector2(-40, -40);

        HorizontalLayoutGroup mainHL = mainRow.AddComponent<HorizontalLayoutGroup>();
        mainHL.childAlignment = TextAnchor.UpperLeft;
        mainHL.spacing = 40;
        mainHL.childForceExpandHeight = false;
        mainHL.childForceExpandWidth = false;

        GameObject patternsGO = new GameObject("Patterns");
        patternsGO.transform.SetParent(mainRow.transform, false);
        VerticalLayoutGroup patVLG = patternsGO.AddComponent<VerticalLayoutGroup>();
        patVLG.childAlignment = TextAnchor.UpperLeft;
        patVLG.spacing = verticalSpacing;
        patternRoot = patternsGO.transform;

        // Header
        AddText(patternRoot, "MATCH PATTERNS", 24, FontStyle.Bold, Color.white, false);

        BuildPatternRows(patternRoot);

        AddText(patternRoot, "(Press H to close)", 14, FontStyle.Italic, Color.gray, false);

        // Fixed width so results stay right side
        LayoutElement le = patternsGO.AddComponent<LayoutElement>();
        le.preferredWidth = patternColumnWidth;
        le.minWidth = patternColumnWidth;

        // Results Column
        GameObject resGO = new GameObject("Results");
        resGO.transform.SetParent(mainRow.transform, false);
        VerticalLayoutGroup resVLG = resGO.AddComponent<VerticalLayoutGroup>();
        resVLG.childAlignment = TextAnchor.UpperLeft;
        resVLG.spacing = verticalSpacing;
        resVLG.padding = new RectOffset(0, 20, 0, 0);

        resultsTextUI = AddText(resGO.transform, "LAST SPIN RESULTS", 20, FontStyle.Bold, Color.white, false);
    }

    Text AddText(Transform parent, string text, int fontSize, FontStyle fontStyle, Color color, bool allowWrap = true)
    {
        GameObject go = new GameObject("Text");
        go.transform.SetParent(parent, false);
        Text uiText = go.AddComponent<Text>();
        uiText.text = text;
        uiText.fontSize = fontSize;
        uiText.fontStyle = fontStyle;
        uiText.color = color;

        // Use built-in UI font (LegacyRuntime.ttf) to avoid version issues
        Font fallbackFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (fallbackFont == null)
        {
            // Fallback to OS Arial if LegacyRuntime is not found
            fallbackFont = Font.CreateDynamicFontFromOSFont("Arial", fontSize);
        }
        uiText.font = fallbackFont;

        uiText.horizontalOverflow = allowWrap ? HorizontalWrapMode.Wrap : HorizontalWrapMode.Overflow;
        uiText.verticalOverflow = VerticalWrapMode.Overflow;

        return uiText;
    }

    void BuildPatternRows(Transform parent)
    {
        foreach (var info in PatternDefinitions())
        {
            AddPatternRow(parent, info.title, info.description, info.activeCells);
        }
    }

    struct PatternDef { public string title; public string description; public List<(int,int)> activeCells; }

    List<PatternDef> PatternDefinitions()
    {
        return new List<PatternDef>
        {
            new PatternDef{ title="Horizontal", description="3 matching symbols in any full row.",       activeCells=new List<(int,int)>{(0,0),(0,1),(0,2)}},
            new PatternDef{ title="Vertical",   description="3 matching symbols in any full column.",    activeCells=new List<(int,int)>{(0,0),(1,0),(2,0)}},
            new PatternDef{ title="Diagonal",   description="3 matching symbols along either main diagonal.", activeCells=new List<(int,int)>{(0,0),(1,1),(2,2)}},
            new PatternDef{ title="ZIG-ZAG",    description="Two columns joined by the center row (or rotated variation).", activeCells=new List<(int,int)>{(0,0),(1,0),(2,0),(1,1),(0,2),(1,2),(2,2)}},
            new PatternDef{ title="X-Shape",    description="4 corners plus center all match.",           activeCells=new List<(int,int)>{(0,0),(0,2),(1,1),(2,0),(2,2)}},
            new PatternDef{ title="Full Grid",  description="All 9 slots match",               activeCells=new List<(int,int)>{(0,0),(0,1),(0,2),(1,0),(1,1),(1,2),(2,0),(2,1),(2,2)}}
        };
    }

    void AddPatternRow(Transform parent, string title, string desc, List<(int,int)> activeCells)
    {
        GameObject row = new GameObject(title);
        row.transform.SetParent(parent,false);
        HorizontalLayoutGroup h = row.AddComponent<HorizontalLayoutGroup>();
        h.spacing = iconTextSpacing;
        h.childAlignment = TextAnchor.MiddleLeft;
        h.childControlWidth = false;
        h.childForceExpandWidth = false;

        GameObject gridGO = new GameObject("Icon");
        gridGO.transform.SetParent(row.transform,false);
        GridLayoutGroup grid = gridGO.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(iconCellSize, iconCellSize);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 3;

        var le = gridGO.AddComponent<LayoutElement>();
        le.preferredWidth = iconCellSize * 3;
        le.minWidth = iconCellSize * 3;

        for(int r=0;r<3;r++)
            for(int c=0;c<3;c++)
                CreateSquare(gridGO.transform, activeCells.Contains((r,c)));

        var txt = AddText(row.transform, $"{title}:\n{desc}", 18, FontStyle.Normal, Color.white, true);
        txt.horizontalOverflow = HorizontalWrapMode.Overflow;
        txt.verticalOverflow = VerticalWrapMode.Overflow;
    }

    Image CreateSquare(Transform parent, bool filled)
    {
        var go = new GameObject("sq");
        go.transform.SetParent(parent,false);
        var img = go.AddComponent<Image>();
        img.color = filled ? Color.white : new Color(1,1,1,0.08f);
        return img;
    }
}   