using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class RelicBoxUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Components")]
    [SerializeField] private Image icon;

    [Header("Configuration")]
    // "Iron Shard", "Light Feather" name.
    [SerializeField] private string relicName; 

    private string tooltipText;
    private bool isInitialized;
    private System.Text.StringBuilder tooltipBuilder;  

    private void Awake()
    {
        InitializeComponents();
    }

    public string GetRelicName()
    {
        return relicName;
    }

    private void InitializeComponents()
    {
        if (isInitialized) return;

        if (icon == null)
        {
            icon = GetComponent<Image>();
            if (icon == null)
            {
                Debug.LogError($"[RelicBoxUI] Image component missing on {gameObject.name}!");
                enabled = false;
                return;
            }
        }

        if (string.IsNullOrEmpty(relicName))
        {
            Debug.LogError($"[RelicBoxUI] Relic name not set on {gameObject.name}!");
            enabled = false;
            return;
        }

        tooltipBuilder = new System.Text.StringBuilder(100); 
        isInitialized = true;
    }

    public void SetupRelic(RelicConfig.RelicData relic)
    {
        if (!isInitialized) return;

        if (relic.name != relicName) return;

        icon.sprite = relic.icon;
        Global.DEBUG_PRINT($"[RelicBoxUI] Set {relic.tier} relic: {relicName}");

        BuildTooltipText(relic);
    }

    public void SetupRelicSO(RelicScriptableObject so)
    {
        if (so == null) return;
        
        icon.sprite = so.GetRelicSprite();
        tooltipText = so.GetRelicName() + "\n" + so.GetRelicDescription();
    }

    private void BuildTooltipText(RelicConfig.RelicData relic)
    {
        tooltipBuilder.Clear();  
        tooltipBuilder.Append(relic.name)
                     .Append(" (")
                     .Append(relic.tier)
                     .Append(")\n")
                     .Append(relic.effect);

        if (relic.tier != RelicTier.Basic && 
            relic.requiredRelics != null && 
            relic.requiredRelics.Length > 0)
        {
            tooltipBuilder.Append("\nRequired: ")
                         .Append(string.Join(" + ", relic.requiredRelics));
        }

        tooltipText = tooltipBuilder.ToString();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (string.IsNullOrEmpty(tooltipText)) return;
        TooltipSystem.Show(tooltipText, eventData.position);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipSystem.Hide();
    }
} 