using UnityEngine;
using TMPro;

public class TooltipSystem : MonoBehaviour
{
    private static TooltipSystem current;
    
    [Header("References")]
    [SerializeField] private GameObject tooltipContainer;
    [SerializeField] private TextMeshProUGUI tooltipText;
    [SerializeField] private Vector2 offset = new Vector2(15f, 15f);
    
    private void Awake()
    {
        current = this;
        HideTooltip();
        
        CanvasGroup canvasGroup = tooltipContainer.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = tooltipContainer.AddComponent<CanvasGroup>();
        }
        canvasGroup.blocksRaycasts = false;
    }

    public static void Show(string content, Vector2 position)
    {
        if (current == null) return;
        
        current.tooltipText.text = content;
        current.tooltipContainer.SetActive(true);
        
        RectTransform rect = current.tooltipContainer.GetComponent<RectTransform>();
        
        // Position tooltip relative to cursor using offset
        Vector2 screenPoint = position;
        // We apply the configurable offset
        screenPoint += current.offset;
        
        // Keep tooltip on screen
        screenPoint.x = Mathf.Clamp(screenPoint.x, rect.rect.width * 0.5f, Screen.width - rect.rect.width * 0.5f);
        screenPoint.y = Mathf.Clamp(screenPoint.y, rect.rect.height, Screen.height - rect.rect.height * 0.5f);
        
        rect.position = screenPoint;
    }
    
    public static void Hide()
    {
        if (current == null) return;
        current.HideTooltip();
    }
    
    private void HideTooltip()
    {
        tooltipContainer.SetActive(false);
    }
} 