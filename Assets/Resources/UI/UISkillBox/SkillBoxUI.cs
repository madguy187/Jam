using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SkillBoxUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler
{
    [Header("UI Components")]
    [SerializeField] private Image patternIcon;
    
    [Header("Skill Settings")]
    [SerializeField] private eRollType skillType;

    [Header("Visual Settings")]
    [SerializeField] private float backgroundAlpha = 0f;
    
    private Image backgroundImage;
    private RectTransform rectTransform;
    private const string PATTERN_SPRITE_PATH = "Sprites/Patterns/{0}";
    private string description;
    private bool isHovering = false;
    private bool isInitialized = false;

    private void Awake()
    {
        CacheComponents();
    }

    private void CacheComponents()
    {
        if (isInitialized) return;
        
        backgroundImage = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();
        
        if (backgroundImage != null)
        {
            SetBackgroundTransparency(backgroundAlpha);
        }
        
        isInitialized = true;
    }

    private void Start()
    {
        Global.DEBUG_PRINT($"SkillBox initialized for type: {skillType}");
        LoadPatternSprite();
    }

    private void LoadPatternSprite()
    {
        if (patternIcon == null) 
        {
            Global.DEBUG_PRINT("Pattern icon reference missing!");
            return;
        }

        string spritePath = string.Format(PATTERN_SPRITE_PATH, skillType.ToString().ToLower());
        Sprite sprite = Resources.Load<Sprite>(spritePath);
        
        if (sprite != null)
        {
            patternIcon.sprite = sprite;
            Global.DEBUG_PRINT($"Loaded pattern sprite from: {spritePath}");
        }
        else
        {
            Global.DEBUG_PRINT($"Failed to load pattern sprite from: {spritePath}");
        }
    }

    private void SetBackgroundTransparency(float alpha)
    {
        Color color = backgroundImage.color;
        color.a = alpha;
        backgroundImage.color = color;
    }

    public void SetupForUnit(string description, float value)
    {
        if (!isInitialized)
        {
            CacheComponents();
        }

        Global.DEBUG_PRINT($"Setting up skill box for type {skillType} with description: {description}");
        this.description = description;
    }

    public eRollType GetRollType()
    {
        return skillType;
    }

    #region Tooltip Handling
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (string.IsNullOrEmpty(description)) return;

        Global.DEBUG_PRINT($"Pointer Enter - Has description: true");
        isHovering = true;
        ShowTooltip(eventData.position);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Global.DEBUG_PRINT("Pointer Exit");
        isHovering = false;
        HideTooltip();
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        if (!isHovering || string.IsNullOrEmpty(description)) return;
        ShowTooltip(eventData.position);
    }

    private void ShowTooltip(Vector2 position)
    {
        TooltipSystem.Show(description, position);
    }

    private void HideTooltip()
    {
        TooltipSystem.Hide();
    }
    #endregion

    private void OnDestroy()
    {
        description = null;
        isHovering = false;
    }
} 