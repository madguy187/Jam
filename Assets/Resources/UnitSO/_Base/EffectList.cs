using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EffectList : IEnumerable<EffectScriptableObject> {
    [SerializeField] List<EffectScriptableObject> _listScriptableObject;

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

    // For IEnumerable<EffectScriptableObject>
    public IEnumerator<EffectScriptableObject> GetEnumerator() { return _listScriptableObject.GetEnumerator(); }
    IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
}
