using System;
using UnityEngine;

using UnityEditor;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Scriptable Object/Unit/EffectScriptableObject")]
public class EffectScriptableObject : ScriptableObject, ISerializationCallbackReceiver {

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

#if UNITY_EDITOR
    [Header("Do not allow changes, this is done to fix the enum from changing")]
    [SerializeField] bool IsLocked = false;

    string _strEditorOriginal_EffectType;
    string _strEditorOriginal_EffectTargetType;
    string _strEditorOriginal_EffectTargetCondition;
    string _strEditorOriginal_EffectExecType;
    string _strEditorOriginal_EffectAffinityType;
    string _strEditorOriginal_EffectResolveType;

    public void OnBeforeSerialize() {
        _strEditorOriginal_EffectType = _eType.ToString();
        _strEditorOriginal_EffectTargetType = _eTargetType.ToString();
        _strEditorOriginal_EffectTargetCondition = _eTargetCondition.ToString();
        _strEditorOriginal_EffectExecType = _eExecType.ToString();
        _strEditorOriginal_EffectAffinityType = _effectAffinityType.ToString();
        _strEditorOriginal_EffectResolveType = _effectResolveType.ToString();
    }

    public void OnAfterDeserialize() {
        if (IsLocked) {
            _eType = (EffectType)Enum.Parse(typeof(EffectType), _strEditorOriginal_EffectType, true);
            _eTargetType = (EffectTargetType)Enum.Parse(typeof(EffectTargetType), _strEditorOriginal_EffectTargetType, true);
            _eTargetCondition = (EffectTargetCondition)Enum.Parse(typeof(EffectTargetCondition), _strEditorOriginal_EffectTargetCondition, true);
            _eExecType = (EffectExecType)Enum.Parse(typeof(EffectExecType), _strEditorOriginal_EffectExecType, true);
            _effectAffinityType = (EffectAffinityType)Enum.Parse(typeof(EffectAffinityType), _strEditorOriginal_EffectAffinityType, true);
            _effectResolveType = (EffectResolveType)Enum.Parse(typeof(EffectResolveType), _strEditorOriginal_EffectResolveType, true);
        }
    }

    public bool GetLock() {
        return IsLocked;
    }
#endif
}
