using UnityEngine;

public enum eUnitArchetype {
    NONE,
    HOLY,
    UNDEAD,
    ELF,
    MOB,
}

public enum eUnitTier {
    STAR_1,
    STAR_2,
    STAR_3,
}

[CreateAssetMenu(menuName = "Scriptable Object/Unit/UnitScriptableObject")]
public class UnitScriptableObject : ScriptableObject {

#if UNITY_EDITOR
    public void SetunitName(string _unitName) { unitName = _unitName; }
    public void SetunitTier(eUnitTier _eTier) { eTier = _eTier; }
    public void SetunitArchetype(eUnitArchetype _eUnitArchetype) { eUnitArchetype = _eUnitArchetype; }
    public void Sethp(float _hp) { hp = _hp; }
    public void Setattack(float _attack) { attack = _attack; }
    public void Setshield(float _shield) { shield = _shield; }
    public void Setres(float _res) { res = _res; }
    public void SetcritRate(int _critRate) { critRate = _critRate; }
    public void SetcritMulti(int _critMulti) { critMulti = _critMulti; }
#endif
    [SerializeField] public string unitName = "";
    [SerializeField] public string unitDescription = "This is a unit description";
    // [SerializeField] Sprite spriteRelic = null;  // Can't have this as it unit contains multiple sprites
    

    [Header("Unit Stat")]
    public eUnitTier eTier = eUnitTier.STAR_1;

    [Header("Main Stats")]
    public eUnitArchetype eUnitArchetype = eUnitArchetype.NONE;

    [Header("Main Stats")]
    public float hp;
    public float attack;
    public float shield;

    [Header("Sub Stats")]
    public float res;
    public int critRate;
    public int critMulti;

    [Header("Skill Descriptions")]
    [TextArea(2,3)]
    public string singleMatchDescription = "";
    [TextArea(2,3)]
    public string horizontalMatchDescription = "";
    [TextArea(2,3)]
    public string diagonalMatchDescription = "";
    [TextArea(2,3)]
    public string zigzagMatchDescription = "";
    [TextArea(2,3)]
    public string xShapeMatchDescription = "";
    [TextArea(2,3)]
    public string fullGridMatchDescription = "";

    public string GetUnitName() 
    {
        return unitName;
    }
    
    public string GetUnitDescription()
    {
        return unitDescription;
    }

    public string GetUnitTierString() 
    {
        return eTier.ToString();
    }

    public int GetUnitCost() {
        switch (eTier) {
            case eUnitTier.STAR_1:
                return 1;
            case eUnitTier.STAR_2:
                return 3;
            case eUnitTier.STAR_3:
                return 5;
        }
        
        return 0;
    }

    public string GetSkillDescription(MatchType matchType)
    {
        switch (matchType)
        {
            case MatchType.SINGLE:
                return singleMatchDescription;
            case MatchType.HORIZONTAL:
            case MatchType.VERTICAL:  
                return horizontalMatchDescription;
            case MatchType.DIAGONAL:
                return diagonalMatchDescription;
            case MatchType.ZIGZAG:
                return zigzagMatchDescription;
            case MatchType.XSHAPE:
                return xShapeMatchDescription;
            case MatchType.FULLGRID:
                return fullGridMatchDescription;
            default:
                return "";
        }
    }
}
