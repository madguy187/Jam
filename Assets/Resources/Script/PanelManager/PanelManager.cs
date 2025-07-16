using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;

public class UnitInfoPanelController : MonoBehaviour
{
    [Header("Panel References")]
    [SerializeField] private Image unitIconImage;
    [SerializeField] private TextMeshProUGUI statsText;
    [SerializeField] private TextMeshProUGUI skillsText;
    [SerializeField] private Transform relicsContainer;
    [SerializeField] private Transform skillsContainer;
    
    [Header("Optional")]
    [SerializeField] private Sprite defaultUnitIcon;

    private UnitObject currentUnit;

    public void ShowUnitInfo(UnitObject unit)
    {
        if (unit == null) return;
        
        currentUnit = unit;
        
        // Update unit icon
        if (unitIconImage != null)
        {
            // Try to get sprite from SpriteRenderer first
            SpriteRenderer spriteRenderer = unit.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null && spriteRenderer.sprite != null)
            {
                unitIconImage.sprite = spriteRenderer.sprite;
                unitIconImage.gameObject.SetActive(true);
            }
            else
            {
                // If no SpriteRenderer, try to get from UI Image
                Image unitImage = unit.GetComponentInChildren<Image>();
                if (unitImage != null && unitImage.sprite != null)
                {
                    unitIconImage.sprite = unitImage.sprite;
                    unitIconImage.gameObject.SetActive(true);
                }
                else if (defaultUnitIcon != null)
                {
                    // Use default icon if available
                    unitIconImage.sprite = defaultUnitIcon;
                    unitIconImage.gameObject.SetActive(true);
                }
                else
                {
                    unitIconImage.gameObject.SetActive(false);
                }
            }
        }

        if (statsText != null)
        {
            statsText.text = $"HP: {unit.GetHealth()}/{unit.unitSO.hp}\n" +
                           $"Shield: {unit.GetShield()}/{unit.unitSO.shield}\n" +
                           $"Attack: {unit.GetAttack()}\n" +
                           $"Resistance: {unit.GetRes()}\n" +
                           $"Crit Rate: {unit.GetCritRate()}%\n" +
                           $"Crit Multi: {unit.GetCritMulti()}%";
        }

        if (skillsText != null)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Skills:");

            foreach (eRollType rollType in System.Enum.GetValues(typeof(eRollType)))
            {
                EffectList effects = unit.GetEffectList(rollType);
                if (effects != null)
                {
                    sb.AppendLine($"\n{rollType}:");
                    
                    foreach (EffectScriptableObject effect in effects)
                    {
                        string effectName = effect.GetTypeName().Replace("EFFECT_", "").Replace("_", " ");
                        sb.AppendLine($"  • {effectName}: {effect.GetEffectVal()}");
                    }
                }
            }

            skillsText.text = sb.ToString();
        }

        gameObject.SetActive(true);
    }

    public void HidePanel()
    {
        gameObject.SetActive(false);
        currentUnit = null;
    }
} 