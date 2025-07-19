using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class PanelManager : MonoBehaviour
{
    [Header("Panel References")]
    [SerializeField] private Image unitIconImage;
    [SerializeField] private TextMeshProUGUI statsText;
    [SerializeField] private TextMeshProUGUI unitIconTitle;
    [SerializeField] private TextMeshProUGUI statsTitle;   

    [Header("Skill UI")]
    [SerializeField] private SkillBoxUI[] skillBoxes;
    [SerializeField] private TextMeshProUGUI skillsTitle;  

    [Header("Unit Skill Descriptions")]
    [Tooltip("Assign the UnitSkillMapping asset here")]
    [SerializeField] private UnitSkillMapping skillMapping;
    
    private UnitObject currentUnit;

    private void Start()
    {
        // Hide all skill boxes
        if (skillBoxes != null)
        {
            foreach (var skillBox in skillBoxes)
            {
                if (skillBox != null)
                {
                    skillBox.gameObject.SetActive(false);
                }
            }
        }
        
        // Hide the panel
        gameObject.SetActive(false);
    }

    public void ShowUnitInfo(UnitObject unit)
    {
        if (unit == null) 
        {
            HidePanel();
            return;
        }
        
        currentUnit = unit;
        
        // Show and set up titles
        unitIconTitle.text = "Unit Icon";
        statsTitle.text = "Combat Stats";
        skillsTitle.text = "Skills";
        
        // Update unit icon
        if (unitIconImage != null)
        {
            SpriteRenderer spriteRenderer = unit.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null && spriteRenderer.sprite != null)
            {
                unitIconImage.sprite = spriteRenderer.sprite;
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
                        skillBox.SetupForUnit(description, value);
                        skillBox.gameObject.SetActive(true);
                    }
                    else
                    {
                        skillBox.gameObject.SetActive(false);
                    }
                }
            }
        }

        // Show the panel
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
        // Hide all skill boxes first
        if (skillBoxes != null)
        {
            foreach (var skillBox in skillBoxes)
            {
                if (skillBox != null)
                {
                    skillBox.gameObject.SetActive(false);
                }
            }
        }
        
        gameObject.SetActive(false);
        currentUnit = null;
    }
} 