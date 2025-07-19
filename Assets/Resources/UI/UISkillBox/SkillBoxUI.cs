using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SkillBoxUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler
{
    [Header("UI Components")]
    [SerializeField] private Image patternIcon;
    
    [Header("Skill Settings")]
    [SerializeField] private eRollType skillType;
    [SerializeField] private Sprite patternSprite;
    
    private RectTransform rectTransform;
    private string description;
    private bool isHovering = false;
    private bool isInitialized = false;

    private void Awake()
    {
        InitializeComponents();
        SetupPatternIcon();
    }

    private void InitializeComponents()
    {
        if (isInitialized) return;
        
        rectTransform = GetComponent<RectTransform>();
        
        if (patternIcon == null)
        {
            patternIcon = GetComponentInChildren<Image>();
            Global.DEBUG_PRINT("Pattern icon reference was missing - attempting to find in children");
        }
        
        isInitialized = true;
    }

    private void SetupPatternIcon()
    {
        if (patternIcon == null) 
        {
            Global.DEBUG_PRINT("Pattern icon reference missing!");
            return;
        }

        if (patternSprite != null)
        {
            patternIcon.sprite = patternSprite;
            Global.DEBUG_PRINT($"Pattern sprite set for type: {skillType}");
        }
        else
        {
            Global.DEBUG_PRINT($"No pattern sprite assigned for type: {skillType}");
        }
    }

    private void Start()
    {
        Global.DEBUG_PRINT($"SkillBox initialized for type: {skillType}");
    }

    public void SetupForUnit(string description, float value)
    {
        if (!isInitialized)
        {
            InitializeComponents();
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