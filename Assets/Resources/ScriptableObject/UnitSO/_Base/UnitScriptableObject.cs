using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Object/Unit/UnitScriptableObject")]
public class UnitScriptableObject : ScriptableObject {
    public string unitName;

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
