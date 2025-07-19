using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewUnitSkills", menuName = "Game/Unit Skills")]
public class UnitSkillConfig : ScriptableObject
{
    [System.Serializable]
    public struct SkillEntry
    {
        public eRollType rollType;
        public string skillName;
        public string description;
        public float value;
    }

    public string unitType = "Paladin";
    public List<SkillEntry> skills = new List<SkillEntry>();

    public SkillEntry GetSkillData(eRollType type)
    {
        return skills.Find(s => s.rollType == type);
    }

    public string GetDescription(eRollType type)
    {
        var skill = GetSkillData(type);
        return $"{skill.description} ({skill.value})";
    }

    public float GetValue(eRollType type)
    {
        var skill = GetSkillData(type);
        return skill.value;
    }
} 