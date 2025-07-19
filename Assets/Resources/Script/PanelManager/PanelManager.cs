using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System;

public class PanelManager : MonoBehaviour
{
    private static PanelManager instance;
    
    public static PanelManager GetInstance()
    {
        return instance;
    }

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
    public event Action<UnitObject> OnUnitSelected;
    public event Action OnPanelHidden;
    private const string STATS_FORMAT = "HP: {0}/{1}\nShield: {2}/{3}\nAttack: {4}\nResistance: {5}\nCrit Rate: {6}%\nCrit Multi: {7}%";
    private const string UNIT_ICON_TITLE = "Unit Icon";
    private const string COMBAT_STATS_TITLE = "Combat Stats";
    private const string SKILLS_TITLE = "Skills";

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            ValidateReferences();
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void ValidateReferences()
    {
        if (unitIconImage == null) Global.DEBUG_PRINT("Unit icon image not assigned!");
        if (statsText == null) Global.DEBUG_PRINT("Stats text not assigned!");
        if (skillBoxes == null || skillBoxes.Length == 0) Global.DEBUG_PRINT("No skill boxes assigned!");
        if (skillMapping == null) Global.DEBUG_PRINT("Skill mapping not assigned!");
    }

    private void Start()
    {
        HidePanel();
    }

    public void ShowUnitInfo(UnitObject unit)
    {
        if (unit == null) 
        {
            HidePanel();
            return;
        }
        
        currentUnit = unit;
        
        unitIconTitle.text = UNIT_ICON_TITLE;
        statsTitle.text = COMBAT_STATS_TITLE;
        skillsTitle.text = SKILLS_TITLE;
        
        if (unitIconImage != null)
        {
            SpriteRenderer spriteRenderer = unit.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null && spriteRenderer.sprite != null)
            {
                unitIconImage.sprite = spriteRenderer.sprite;
            }
        }

        if (statsText != null)
        {
            statsText.text = string.Format(STATS_FORMAT,
                unit.GetHealth(), unit.unitSO.hp,
                unit.GetShield(), unit.unitSO.shield,
                unit.GetAttack(),
                unit.GetRes(),
                unit.GetCritRate(),
                unit.GetCritMulti());
        }

        UpdateSkillBoxes(unit);

        gameObject.SetActive(true);
        OnUnitSelected?.Invoke(unit);
    }

    private void UpdateSkillBoxes(UnitObject unit)
    {
        if (skillBoxes == null || skillBoxes.Length == 0) return;

        UnitSkillConfig skillConfig = GetSkillConfigForUnit(unit);
        bool hasSkillConfig = (skillConfig != null);

        foreach (var skillBox in skillBoxes)
        {
            if (skillBox != null)
            {
                if (hasSkillConfig)
                {
                    string description = skillConfig.GetDescription(skillBox.GetMatchType());
                    float value = skillConfig.GetValue(skillBox.GetMatchType());
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

    private UnitSkillConfig GetSkillConfigForUnit(UnitObject unit)
    {
        if (unit?.unitSO == null || skillMapping == null) return null;

        return skillMapping.GetSkillConfig(unit.unitSO.name) 
            ?? skillMapping.GetSkillConfig(unit.name);
    }

    public void HidePanel()
    {
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
        OnPanelHidden?.Invoke();
    }

    private void OnDestroy()
    {
        OnUnitSelected = null;
        OnPanelHidden = null;

        if (instance == this)
        {
            instance = null;
        }
    }
} 