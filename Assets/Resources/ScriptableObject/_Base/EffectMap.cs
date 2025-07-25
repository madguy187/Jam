using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EffectMap : IEnumerable<EffectObject> {
    Dictionary<EffectType, EffectObject> _dictEffect = new Dictionary<EffectType, EffectObject>();
    [SerializeField] UIEffectGrid _effectGridUI = null;

#if UNITY_EDITOR
    public void SetGrid(UIEffectGrid grid) { _effectGridUI = grid; }
#endif

    public void AddEffect(EffectType eType, EffectObject objEffect) {
        if (!_dictEffect.ContainsKey(eType)) {
            _dictEffect.Add(eType, objEffect);
            _effectGridUI.AddEffectUI(eType);
        }
    }

    public void RemoveEffect(EffectType eType) {
        _dictEffect.Remove(eType);
        _effectGridUI.RemoveEffectUI(eType);
    }

    public void RemoveEffectByPredicate(Predicate<EffectObject> predicate) {
        List<EffectType> listRemove = new List<EffectType>();
        foreach (KeyValuePair<EffectType, EffectObject> pair in _dictEffect) {
            if (predicate(pair.Value)) {
                listRemove.Add(pair.Key);
            }
        }

        foreach (EffectType type in listRemove) {
            RemoveEffect(type);
        }
    }

    public float GetParam(EffectType eType) {
        if (_dictEffect.ContainsKey(eType)) {
            return _dictEffect[eType].GetEffectVal();
        }

        return 0.0f;
    }

    public EffectScriptableObject GetEffectSO(EffectType eType) {
        if (_dictEffect.ContainsKey(eType)) {
            return _dictEffect[eType].effectSO;
        }

        return null;
    }

    public void Clear() {
        _dictEffect.Clear();
    }

    public void Resolve(EffectResolveType eResolveType) {
        List<EffectType> listRemove = new List<EffectType>();
        foreach (EffectObject effect in _dictEffect.Values) {
            if (effect.GetEffectResolveType() != eResolveType) {
                continue;
            }

            effect.Resolve();

            if (effect.IsEmpty()) {
                listRemove.Add(effect.GetEffectType());
            }
        }

        foreach (EffectType type in listRemove) {
            RemoveEffect(type);
        }
    }

    public void ResolveOne(EffectType eType) {
        if (!_dictEffect.ContainsKey(eType)) {
            return;
        }

        EffectObject effect = _dictEffect[eType];
        effect.Resolve();
        if (effect.IsEmpty()) {
            RemoveEffect(eType);
        }
    }

    // For IEnumerable<EffectObject>
    public IEnumerator<EffectObject> GetEnumerator() { return _dictEffect.Values.GetEnumerator(); }
    IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
}
