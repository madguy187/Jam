using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic; 

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

    [Header("Relic UI")]
    [SerializeField] private RelicBoxUI[] relicBoxes;
    [SerializeField] private RelicConfig relicConfig;

    private Dictionary<string, RelicBoxUI> relicBoxMap;
    private Dictionary<RelicTier, RelicConfig.RelicData[]> relicsByTier;
    private bool relicsInitialized;

    [Header("Unit Skill Descriptions")]
    [Tooltip("Assign the UnitSkillMapping asset here")]
    [SerializeField] private UnitSkillMapping skillMapping;
    
    private UnitObject currentUnit;
    
    public UnitObject GetCurrentUnit()
    {
        return currentUnit;
    }

    public event Action<UnitObject> OnUnitSelected;
    public event Action OnPanelHidden;
    private const string STATS_FORMAT = "HP: {0}/{1}\nShield: {2}/{3}\nAttack: {4}\nResistance: {5}\nCrit Rate: {6}%\nCrit Multi: {7}%";

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            InitializeRelics();  
            ValidateReferences();
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void InitializeRelics()
    {
        if (relicsInitialized) return;

        if (relicBoxes == null || relicBoxes.Length == 0)
        {
            Debug.LogError("[PanelManager] No relic boxes assigned!");
            return;
        }

        if (relicConfig == null)
        {
            Debug.LogError("[PanelManager] Relic config not assigned!");
            return;
        }

        relicBoxMap = new Dictionary<string, RelicBoxUI>();
        foreach (var relicBox in relicBoxes)
        {
            if (relicBox != null)
            {
                relicBoxMap[relicBox.GetRelicName()] = relicBox;
            }
        }

        relicsByTier = new Dictionary<RelicTier, RelicConfig.RelicData[]>
        {
            { RelicTier.Basic, relicConfig.basicRelics },
            { RelicTier.AdvancedSelf, relicConfig.advancedSelfRelics },
            { RelicTier.AdvancedMixed, relicConfig.advancedMixedRelics },
            { RelicTier.Legendary, relicConfig.legendaryRelics }
        };

        relicsInitialized = true;
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

    private void UpdateRelicBoxes()
    {
        if (!relicsInitialized) return;

        if (relicsByTier.TryGetValue(RelicTier.Basic, out RelicConfig.RelicData[] basicRelics))
        {
            foreach (var relic in basicRelics)
            {
                if (relicBoxMap.TryGetValue(relic.name, out RelicBoxUI relicBox))
                {
                    relicBox.SetupRelic(relic);
                    relicBox.gameObject.SetActive(true); 
                }
            }
        }

        foreach (var relicBox in relicBoxes)
        {
            if (relicBox != null && !relicBox.gameObject.activeSelf)
            {
                relicBox.gameObject.SetActive(true);
            }
        }
    }

    public void ShowUnitInfo(UnitObject unit)
    {
        if (unit == null) 
        {
            HidePanel();
            return;
        }
        
        currentUnit = unit;
        
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
        UpdateRelicBoxes();

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

        if (relicBoxes != null)
        {
            foreach (var relicBox in relicBoxes)
            {
                if (relicBox != null)
                {
                    relicBox.gameObject.SetActive(false);
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