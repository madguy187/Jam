using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Object/Unit/EffectScriptableObject")]
public class EffectScriptableObject : ScriptableObject {
    [SerializeField] EffectType _eType;
    [SerializeField] EffectTargetType _eTargetType;
    [SerializeField] EffectExecType _eExecType;
    [SerializeField] float _fEffectVal = 0;

    [Header("This will only be used if exec type is turn specified, Default: Start of round")]
    [SerializeField] int _fEffectTurn = 0;

    public EffectType GetEffectType() { return _eType; }
    public EffectTargetType GetTargetType() { return _eTargetType; }
    public EffectExecType GetExecType() { return _eExecType; }
    public int GetEffectTurn() { return _fEffectTurn; }

    public bool IsEffectType(EffectType eEffectType) {
        return _eType == eEffectType;
    }

    public float GetEffectVal() {
        return _fEffectVal;
    }

    public string GetTypeName() {
        return _eType.ToString();
    }
}
