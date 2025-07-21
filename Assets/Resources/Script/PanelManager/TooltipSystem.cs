using UnityEngine;
using TMPro;

public class TooltipSystem : MonoBehaviour
{
    public static TooltipSystem instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject tooltipContainer;
    [SerializeField] private TextMeshProUGUI tooltipText;
    
    [Header("Settings")]
    [SerializeField] private Vector2 offset = new Vector2(15f, 15f);
    [SerializeField] private float edgePadding = 10f; 
    private RectTransform containerRect;
    private CanvasGroup canvasGroup;
    private const float MIN_SCREEN_PADDING = 5f;
    private const float HALF_MULTIPLIER = 0.5f;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            InitializeComponents();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeComponents()
    {
        ValidateReferences();
        SetupContainer();
        HideTooltip();
    }

    private void ValidateReferences()
    {
        if (tooltipContainer == null)
        {
            Debug.LogError("[TooltipSystem] Tooltip container not assigned!");
            return;
        }

        if (tooltipText == null)
        {
            Debug.LogError("[TooltipSystem] Tooltip text not assigned!");
            return;
        }

        containerRect = tooltipContainer.GetComponent<RectTransform>();
        if (containerRect == null)
        {
            Debug.LogError("[TooltipSystem] RectTransform not found on tooltip container!");
            return;
        }
    }

    private void SetupContainer()
    {
        canvasGroup = tooltipContainer.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = tooltipContainer.AddComponent<CanvasGroup>();
        }

        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
    }

    public static void Show(string content, Vector2 position)
    {
        if (string.IsNullOrEmpty(content)) return;
        instance.ShowTooltip(content, position);
    }

    private void ShowTooltip(string content, Vector2 position)
    {
        tooltipText.text = content;
        tooltipContainer.SetActive(true);

        Vector2 screenPoint = CalculateTooltipPosition(position);
        containerRect.position = screenPoint;
    }

    private Vector2 CalculateTooltipPosition(Vector2 basePosition)
    {
        Vector2 screenPoint = basePosition + offset;

        float tooltipWidth = containerRect.rect.width;
        float tooltipHeight = containerRect.rect.height;

        screenPoint.x = Mathf.Clamp(
            screenPoint.x,
            tooltipWidth * HALF_MULTIPLIER + MIN_SCREEN_PADDING,
            Screen.width - (tooltipWidth * HALF_MULTIPLIER + edgePadding)
        );

        screenPoint.y = Mathf.Clamp(
            screenPoint.y,
            tooltipHeight + MIN_SCREEN_PADDING,
            Screen.height - (tooltipHeight * HALF_MULTIPLIER + edgePadding)
        );

        return screenPoint;
    }

    public static void Hide()
    {
        instance.HideTooltip();
    }

    private void HideTooltip()
    {
        if (tooltipContainer != null)
        {
            tooltipContainer.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
} 