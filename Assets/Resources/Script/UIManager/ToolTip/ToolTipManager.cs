using UnityEngine;
using TMPro;

public class ToolTipManager : MonoBehaviour {
    public static ToolTipManager Instance { get; private set; }
    public CanvasGroup ToolTipCanvasGroup; // Reference to the canvas group for the tooltip
    public Canvas parentCanvas; // Reference to the parent canvas for the tooltip
    public RectTransform ToolTipTransform; // The transform of the tooltip UI element

    public TMP_Text currTitle, currDetails; // Text components for title and details
    private bool isShowing;
    private float _frameIncrease;
    
    private void Awake()
    {
        if (Instance != null && Instance != this) {
            Destroy(gameObject); // Prevent duplicates
            return;
        }
        Instance = this;
    }

    void Start()
    {
        isShowing = false;
    }

    void Update()
    {
        if (isShowing) {
            if (ToolTipCanvasGroup.alpha < 1) {
                ToolTipCanvasGroup.alpha += Time.deltaTime * _frameIncrease; // Fade in the tooltip
            }
            Vector2 localPoint;
            // Convert screen position to local position in the canvas
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentCanvas.transform as RectTransform,
                Input.mousePosition,
                parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : parentCanvas.worldCamera,
                out localPoint
            );

            ToolTipTransform.anchoredPosition = localPoint;
        }
    }

    public void Show(string titleText, string detailsText, float frameIncrease = 3f) {
        ToolTipCanvasGroup.alpha = 0;
        currTitle.text = titleText;
        currDetails.text = detailsText;
        _frameIncrease = frameIncrease;
        ToolTipTransform.gameObject.SetActive(true);
        isShowing = true;
    }

    public void Hide() {
        ToolTipTransform.gameObject.SetActive(false);
        isShowing = false;
    }
}
