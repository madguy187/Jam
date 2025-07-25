using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;

public class ToolTipDetails : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler 
{
    public string titleText;
    public string detailsText;

    private bool hasMouseOver;
    private float toolTipTimer;

    [Header("ToolTip Settings")]
    public float ToolTipDelaySeconds = 0.3f;
    public float ToolTipFrameIncrease = 3f; // How fast the tooltip fades in

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        hasMouseOver = false;
    }

    public void Init(string title, string details)
    {
        titleText = title;
        detailsText = details;
    }

    // Update is called once per frame
    virtual public void Update()
    {
        if (hasMouseOver && toolTipTimer < ToolTipDelaySeconds) {
            toolTipTimer += Time.deltaTime;
            if (toolTipTimer >= ToolTipDelaySeconds) {
                ToolTipManager.Instance.Show(titleText, detailsText, ToolTipFrameIncrease);
            }
        }
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        toolTipTimer = 0;
        hasMouseOver = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hasMouseOver = false;
        ToolTipManager.Instance.Hide();
    }
}
