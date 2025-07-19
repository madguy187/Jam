using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class UnitInfoPanelController : MonoBehaviour
{
    [Header("Panel References")]
    [SerializeField] private Image unitIconImage;
    [SerializeField] private TextMeshProUGUI statsText;

    [Header("Skill UI")]
    [SerializeField] private SkillBoxUI[] skillBoxes;

    [Header("Unit Skill Descriptions")]
    [Tooltip("Assign the UnitSkillMapping asset here")]
    [SerializeField] private UnitSkillMapping skillMapping;
    
    private UnitObject currentUnit;

    public void ShowUnitInfo(UnitObject unit)
    {
        if (unit == null) return;
        
        currentUnit = unit;
        
        // Update unit icon
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

        // Update skill boxes
        if (skillBoxes != null && skillBoxes.Length > 0)
        {
            // Get the appropriate skill config
            UnitSkillConfig skillConfig = GetSkillConfigForUnit(unit);
            bool hasSkillConfig = (skillConfig != null);

            foreach (var skillBox in skillBoxes)
            {
                if (skillBox != null)
                {
                    if (hasSkillConfig)
                    {
                        string description = skillConfig.GetDescription(skillBox.GetRollType());
                        float value = skillConfig.GetValue(skillBox.GetRollType());
                        
                        Global.DEBUG_PRINT($"Setting up {skillBox.GetRollType()} with description: {description}");
                        skillBox.SetupForUnit(description, value);
                        skillBox.gameObject.SetActive(true);
                    }
                    else
                    {
                        // hide if no skills
                        skillBox.gameObject.SetActive(false);
                        Global.DEBUG_PRINT($"No skills configured for unit: {unit.name} - hiding skill box");
                    }
                }
            }
        }

        gameObject.SetActive(true);
    }

    private UnitSkillConfig GetSkillConfigForUnit(UnitObject unit)
    {
        if (unit == null || unit.unitSO == null || skillMapping == null) return null;

        UnitSkillConfig config = skillMapping.GetSkillConfig(unit.unitSO.name);
        if (config != null) return config;

        return skillMapping.GetSkillConfig(unit.name);
    }

    private MatchType GetMatchTypeForRollType(eRollType rollType)
    {
        switch (rollType)
        {
            case eRollType.SINGLE:
                return MatchType.SINGLE;
            case eRollType.DIAGONAL:
                return MatchType.DIAGONAL;
            case eRollType.ZIGZAG:
                return MatchType.ZIGZAG;
            case eRollType.XSHAPE:
                return MatchType.XSHAPE;
            case eRollType.FULLGRID:
                return MatchType.FULLGRID;
            case eRollType.VERTICAL:
                return MatchType.VERTICAL;
            case eRollType.HORIZONTAL:
                return MatchType.HORIZONTAL;
            default:
                return MatchType.SINGLE;
        }
    }

    public void HidePanel()
    {
        gameObject.SetActive(false);
        currentUnit = null;
    }
} 