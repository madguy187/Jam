using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Object/Unit/EffectScriptableObject")]
public class EffectScriptableObject : ScriptableObject {

#if UNITY_EDITOR
    public void SetEffectType(EffectType eType) { _eType = eType; }
    public void SetEffectVal(float fVal) { _fEffectVal = fVal; }
    public void SetEffectTargetType(EffectTargetType eTargetType) { _eTargetType = eTargetType; }
    public void SetEffectTargetCondition(EffectTargetCondition eTargetCondition) { _eTargetCondition = eTargetCondition; }
    public void SetEffectTargetParam(float fVal) { _fTargetParam = fVal; }
    public void SetEffectExecType(EffectExecType eExecType) { _eExecType = eExecType; }
    public void SetEffectAffinityType(EffectAffinityType effectAffinityType) { _effectAffinityType = effectAffinityType; }
    public void SetEffectResolveType(EffectResolveType effectResolveType) { _effectResolveType = effectResolveType; }
    public void SetEffectCount(int fVal) { _fEffectCount = fVal; }
#endif

    [Header("Do not change _strType")]
    [SerializeField] EffectType _eType;
    public EffectType GetEffectType() { return _eType; }

    [SerializeField] float _fEffectVal = 0;

    [Header("Target Param")]
    [SerializeField] EffectTargetType _eTargetType = EffectTargetType.SELF;
    [SerializeField] EffectTargetCondition _eTargetCondition = EffectTargetCondition.NONE;
    [SerializeField] float _fTargetParam = 0.0f;

    public EffectTargetType GetTargetType() { return _eTargetType; }
    public EffectTargetCondition GetTargetCondition() { return _eTargetCondition; }
    public float GetTargetParam() { return _fTargetParam; }

    [Header("Exec Param")]
    [SerializeField] EffectExecType _eExecType = EffectExecType.TRIGGER_ONCE;

    public EffectExecType GetExecType() { return _eExecType; }

    [Header("Affinity")]
    [SerializeField] EffectAffinityType _effectAffinityType = EffectAffinityType.NONE;

    public EffectAffinityType GetEffectAffinityType() { return _effectAffinityType; }

    [Header("Resolution")]
    [SerializeField] EffectResolveType _effectResolveType = EffectResolveType.RESOLVE_TURN;
    [SerializeField] int _fEffectCount = 0;

    public void InitScriptableInstance(EffectType effectType,
                                        float effectVal,
                                        EffectTargetType targetType,
                                        EffectTargetCondition targetCondition,
                                        float targetParam,
                                        EffectExecType execType,
                                        EffectAffinityType affinityType,
                                        EffectResolveType resolveType,
                                        int resolveCount) {
        _eType = effectType;
        _fEffectVal = effectVal;
        _eTargetType = targetType;
        _eTargetCondition = targetCondition;
        _fTargetParam = targetParam;
        _eExecType = execType;
        _effectAffinityType = affinityType;
        _effectResolveType = resolveType;
        _fEffectCount = resolveCount;
    }

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
