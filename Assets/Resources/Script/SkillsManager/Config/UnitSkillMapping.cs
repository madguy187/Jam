using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "UnitSkillMapping", menuName = "Game/Unit Skill Mapping")]
public class UnitSkillMapping : ScriptableObject
{
    [System.Serializable]
    public struct Mapping
    {
        public string unitType;
        public UnitSkillConfig skillConfig;
    }

    public List<Mapping> mappings = new List<Mapping>();
    private Dictionary<string, UnitSkillConfig> skillConfigCache;

    private void OnEnable()
    {
        InitializeCache();
    }

    private void InitializeCache()
    {
        skillConfigCache = new Dictionary<string, UnitSkillConfig>();
        
        foreach (var mapping in mappings)
        {
            if (!string.IsNullOrEmpty(mapping.unitType) && mapping.skillConfig != null)
            {
                string key = mapping.unitType.ToLower();
                skillConfigCache[key] = mapping.skillConfig;
            }
        }
    }

    public UnitSkillConfig GetSkillConfig(string unitName)
    {
        if (skillConfigCache == null)
        {
            InitializeCache();
        }

        string key = unitName.ToLower();
        
        if (skillConfigCache.TryGetValue(key, out UnitSkillConfig config))
        {
            return config;
        }

        foreach (var cachedKey in skillConfigCache.Keys)
        {
            if (key.Contains(cachedKey))
            {
                return skillConfigCache[cachedKey];
            }
        }

        return null;
    }
} 