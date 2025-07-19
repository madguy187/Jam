using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SkillBoxUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler
{
    [Header("Skill Settings")]
    [SerializeField] private MatchType skillType;
    
    private RectTransform rectTransform;
    private string description;
    private bool isHovering = false;
    private bool isInitialized = false;

    private void Awake()
    {
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        if (isInitialized) return;
        rectTransform = GetComponent<RectTransform>();
        isInitialized = true;
    }

    private void Start()
    {
        Global.DEBUG_PRINT($"SkillBox initialized for type: {skillType}");
    }

    public void SetupForUnit(string description, float value)
    {
        this.description = description;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
        TooltipSystem.Show(description, eventData.position);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        TooltipSystem.Hide();
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        if (isHovering)
        {
            TooltipSystem.Show(description, eventData.position);
        }
    }

    public MatchType GetMatchType()
    {
        return skillType;
    }
} 