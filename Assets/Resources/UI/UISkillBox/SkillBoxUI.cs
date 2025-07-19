using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SkillBoxUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler
{
    [Header("UI Components")]
    [SerializeField] private Image patternIcon;
    
    [Header("Skill Settings")]
    [SerializeField] private eRollType skillType;
    
    private string description;
    private bool isHovering = false;

    private void Start()
    {
        Global.DEBUG_PRINT($"SkillBox initialized for type: {skillType}");
    }

    public void SetupForUnit(string description, float value)
    {
        Global.DEBUG_PRINT($"Setting up skill box for type {skillType} with description: {description}");
        this.description = description;

        if (patternIcon != null)
        {
            patternIcon.sprite = Resources.Load<Sprite>($"Sprites/Patterns/{skillType.ToString().ToLower()}");
        }
    }

    public eRollType GetRollType()
    {
        return skillType;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Global.DEBUG_PRINT($"Pointer Enter - Has description: {!string.IsNullOrEmpty(description)}");
        if (!string.IsNullOrEmpty(description))
        {
            isHovering = true;
            TooltipSystem.Show(description, eventData.position);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Global.DEBUG_PRINT("Pointer Exit");
        isHovering = false;
        TooltipSystem.Hide();
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        if (isHovering && !string.IsNullOrEmpty(description))
        {
            TooltipSystem.Show(description, eventData.position);
        }
    }
} 