using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Object/Unit/UnitScriptableObject")]
public class UnitScriptableObject : ScriptableObject
{
    public string unitName;
    public float hp;
    public float damage;
}
