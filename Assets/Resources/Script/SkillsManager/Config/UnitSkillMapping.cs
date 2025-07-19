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

    public UnitSkillConfig GetSkillConfig(string unitName)
    {
        unitName = unitName.ToLower();
        foreach (var mapping in mappings)
        {
            if (unitName.Contains(mapping.unitType.ToLower()))
            {
                return mapping.skillConfig;
            }
        }
        return null;
    }
} 