using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class UnitInfoPanelController : MonoBehaviour
{
    [Header("Panel References")]
    [SerializeField] private Image unitIconImage;
    [SerializeField] private TextMeshProUGUI statsText;
    [SerializeField] private TextMeshProUGUI skillsText;

    private UnitObject currentUnit;

    public void ShowUnitInfo(UnitObject unit)
    {
        if (unit == null) return;
        
        currentUnit = unit;
        
        if (unitIconImage != null)
        {
            SpriteRenderer spriteRenderer = unit.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null && spriteRenderer.sprite != null)
            {
                unitIconImage.sprite = spriteRenderer.sprite;
                unitIconImage.gameObject.SetActive(true);
            }
            else
            {
                unitIconImage.gameObject.SetActive(false);
            }
        }

        // Update stats
        if (statsText != null)
        {
            statsText.text = $"HP: {unit.GetHealth()}/{unit.unitSO.hp}\n" +
                           $"Shield: {unit.GetShield()}/{unit.unitSO.shield}\n" +
                           $"Attack: {unit.GetAttack()}\n" +
                           $"Resistance: {unit.GetRes()}\n" +
                           $"Crit Rate: {unit.GetCritRate()}%\n" +
                           $"Crit Multi: {unit.GetCritMulti()}%";
        }

        // Update skills
        if (skillsText != null)
        {
            string skillsList = "Skills:\n";
            foreach (eRollType rollType in System.Enum.GetValues(typeof(eRollType)))
            {
                EffectList effects = unit.GetEffectList(rollType);
                if (effects != null)
                {
                    foreach (EffectScriptableObject effect in effects)
                    {
                        string effectName = effect.GetTypeName().Replace("EFFECT_", "").Replace("_", " ");
                        skillsList += $"• {rollType}: {effectName} ({effect.GetEffectVal()})\n";
                    }
                }
            }
            skillsText.text = skillsList;
        }

        gameObject.SetActive(true);
    }

    public void HidePanel()
    {
        gameObject.SetActive(false);
        currentUnit = null;
    }
} 