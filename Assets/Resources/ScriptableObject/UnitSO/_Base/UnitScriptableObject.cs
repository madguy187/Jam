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
    
}
