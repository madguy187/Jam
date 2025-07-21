using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EffectList : IEnumerable<EffectScriptableObject> {
    [SerializeField] List<EffectScriptableObject> _listScriptableObject;

    public bool IsValid() {
        return _listScriptableObject != null;
    }

    public float GetParam(EffectType eType) {
        float val = 0.0f;

        foreach (EffectScriptableObject effect in _listScriptableObject) {
            if (!effect.IsEffectType(eType)) {
                continue;
            }

            val += effect.GetEffectVal();
        }

        return val;
    }

    public void LoadFromRelicSO(RelicScriptableObject relicSO) {
        if (relicSO == null) {
            return;
        }
        
        _listScriptableObject = new List<EffectScriptableObject>();

        foreach (EffectScriptableObject effect in relicSO) {
            _listScriptableObject.Add(effect);
        }
    }

    // For IEnumerable<EffectScriptableObject>
    public IEnumerator<EffectScriptableObject> GetEnumerator() { return _listScriptableObject.GetEnumerator(); }
    IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
}
