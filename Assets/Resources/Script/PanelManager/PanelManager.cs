using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic; 

public class PanelManager : MonoBehaviour
{
    public static PanelManager instance { get; private set; }

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
            InitializePanel();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializePanel()
    {
        ValidateReferences();
        InitializeRelics();
        HidePanel(); 
    }

    private void ValidateReferences()
    {
        if (unitIconImage == null) 
        {
            Debug.LogError("[PanelManager] Unit icon image not assigned!");
        }
        
        if (statsText == null) 
        {
            Debug.LogError("[PanelManager] Stats text not assigned!");
        }
        
        if (skillBoxes == null || skillBoxes.Length == 0) 
        {
            Debug.LogError("[PanelManager] No skill boxes assigned!");
        }

        if (relicBoxes == null || relicBoxes.Length == 0)
        {
            Debug.LogError("[PanelManager] No relic boxes assigned!");
        }

        if (relicConfig == null)
        {
            Debug.LogError("[PanelManager] Relic config not assigned!");
        }
    }

    private void InitializeRelics()
    {
        if (relicsInitialized) return;
        if (relicBoxes == null || relicConfig == null) return;

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

    private void Update()
    {
        // Only update if panel is visible and we have a unit
        if (gameObject.activeSelf && currentUnit != null)
        {
            if (currentUnit.IsDead())
            {
                // If current unit died, hide the panel
                HidePanel();
            }
            else
            {
                // Update stats for the currently selected unit
                UpdateUnitStats();
                UpdateRelicBoxes();
            }
        }
    }

    private void UpdateUnitStats()
    {
        if (statsText != null && currentUnit != null)
        {
            statsText.text = string.Format(STATS_FORMAT,
                currentUnit.GetHealth(), currentUnit.unitSO.hp,
                currentUnit.GetShield(), currentUnit.unitSO.shield,
                currentUnit.GetAttack(),
                currentUnit.GetRes(),
                currentUnit.GetCritRate(),
                currentUnit.GetCritMulti());
        }
    }

    private void UpdateRelicBoxes()
    {
        if (relicBoxes == null || relicBoxes.Length == 0) return;

        // Hide all first
        foreach (var rb in relicBoxes)
            if (rb != null) rb.gameObject.SetActive(false);

        if (currentUnit == null) return;

        List<RelicScriptableObject> relicList = currentUnit.GetRelic();
        for (int i = 0; i < relicBoxes.Length && i < relicList.Count; i++)
        {
            RelicBoxUI box = relicBoxes[i];
            if (box == null) continue;
            box.SetupRelicSO(relicList[i]);
            box.gameObject.SetActive(true);
        }
    }

    public void ShowUnitInfo(UnitObject unit)
    {
        if (unit == null || unit.IsDead()) 
        {
            HidePanel();
            return;
        }
        
        currentUnit = unit;
        
        if (unitIconImage != null)
        {
            Sprite iconSprite = RenderUtilities.RenderUnitHeadSprite(unit);
            if (iconSprite != null)
            {
                unitIconImage.sprite = iconSprite;
            }
            else
            {
                SpriteRenderer sr = unit.GetComponent<SpriteRenderer>();
                if (sr != null && sr.sprite != null)
                {
                    unitIconImage.sprite = sr.sprite;
                }
            }
        }

        UpdateUnitStats();
        UpdateSkillBoxes(unit);
        UpdateRelicBoxes();

        gameObject.SetActive(true);
        OnUnitSelected?.Invoke(unit);
    }

    private void UpdateSkillBoxes(UnitObject unit)
    {
        if (skillBoxes == null || skillBoxes.Length == 0) return;
        if (unit?.unitSO == null) return;

        foreach (var skillBox in skillBoxes)
        {
            if (skillBox != null)
            {
                string description = unit.unitSO.GetSkillDescription(skillBox.GetMatchType());
                if (!string.IsNullOrEmpty(description))
                {
                    skillBox.SetupForUnit(description, 0);
                    skillBox.gameObject.SetActive(true);
                }
                else
                {
                    skillBox.gameObject.SetActive(false);
                }
            }
        }
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