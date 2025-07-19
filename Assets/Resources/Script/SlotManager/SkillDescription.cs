using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "UnitSkillDescriptions", menuName = "Game/Unit Skill Descriptions")]
public class UnitSkillDescriptions : ScriptableObject
{
    [System.Serializable]
    public struct SkillEntry
    {
        public eRollType matchType;
        public string description;
        public float value;
    }

    [SerializeField]
    public List<SkillEntry> skillDescriptions = new List<SkillEntry>();

    public string GetDescription(eRollType matchType)
    {
        var skill = skillDescriptions.Find(s => s.matchType == matchType);
        return $"{skill.description} ({skill.value})";
    }

    public float GetValue(eRollType matchType)
    {
        var skill = skillDescriptions.Find(s => s.matchType == matchType);
        return skill.value;
    }
} 