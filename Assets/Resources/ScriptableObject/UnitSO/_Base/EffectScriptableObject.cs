using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Object/Unit/EffectScriptableObject")]
public class EffectScriptableObject : ScriptableObject {

    [SerializeField] EffectType _eType;
    public EffectType GetEffectType() { return _eType; }
    [SerializeField] float _fEffectVal = 0;

    [Header("Target Param")]
    [SerializeField] EffectTargetType _eTargetType = EffectTargetType.SELF;
    [SerializeField] EffectTargetCondition _eTargetCondition = EffectTargetCondition.NONE;
    [SerializeField] float _eTargetParam = 0.0f;

    public EffectTargetType GetTargetType() { return _eTargetType; }
    public EffectTargetCondition GetTargetCondition() { return _eTargetCondition; }
    public float GetTargetParam() { return _eTargetParam; }

    [Header("Exec Param")]
    [SerializeField] EffectExecType _eExecType = EffectExecType.TRIGGER_ONCE;

    public EffectExecType GetExecType() { return _eExecType; }

    [Header("Affinity")]
    [SerializeField] EffectAffinityType _effectAffinityType = EffectAffinityType.NONE;

    public EffectAffinityType GetEffectAffinityType() { return _effectAffinityType; }

    [Header("Resolution")]
    [SerializeField] EffectResolveType _effectResolveType = EffectResolveType.RESOLVE_TURN;
    [SerializeField] int _fEffectCount = 0;

    public EffectResolveType GetEffectResolveType() { return _effectResolveType; }
    public int GetEffectCount() { return _fEffectCount; }

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
