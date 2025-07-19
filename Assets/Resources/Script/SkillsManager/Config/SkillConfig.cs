using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewUnitSkills", menuName = "Game/Unit Skills")]
public class UnitSkillConfig : ScriptableObject
{
    [System.Serializable]
    public struct SkillEntry
    {
        public MatchType rollType;
        public string skillName;
        public string description;
        public float value;
    }

    public string unitType = "Paladin";
    public List<SkillEntry> skills = new List<SkillEntry>();

    private Dictionary<MatchType, SkillEntry> skillCache;

    private void OnEnable()
    {
        InitializeCache();
    }

    private void InitializeCache()
    {
        skillCache = new Dictionary<MatchType, SkillEntry>();
        foreach (var skill in skills)
        {
            skillCache[skill.rollType] = skill;
        }
    }

    public SkillEntry GetSkillData(MatchType type)
    {
        if (skillCache == null)
        {
            InitializeCache();
        }

        if (skillCache.TryGetValue(type, out SkillEntry skill))
        {
            return skill;
        }

        return default; 
    }

    public string GetDescription(MatchType type)
    {
        var skill = GetSkillData(type);
        if (skill.Equals(default(SkillEntry))) return string.Empty;
        
        return $"{skill.description} ({skill.value})";
    }

    public float GetValue(MatchType type)
    {
        var skill = GetSkillData(type);
        return skill.value;
    }
} 