using System;
using System.Collections;
using System.Collections.Generic;

public class EffectMap : IEnumerable<EffectObject> {
    Dictionary<EffectType, EffectObject> _dictEffect = new Dictionary<EffectType, EffectObject>();

    public void AddEffect(EffectType eType, EffectObject objEffect) {
        if (!_dictEffect.ContainsKey(eType)) {
            _dictEffect.Add(eType, objEffect);
        }
    }

    public void RemoveEffect(EffectType eType) {
        _dictEffect.Remove(eType);
    }

    public void RemoveEffectByPredicate(Predicate<EffectObject> predicate) {
        List<EffectType> listRemove = new List<EffectType>();
        foreach (KeyValuePair<EffectType, EffectObject> pair in _dictEffect) {
            if (predicate(pair.Value)) {
                listRemove.Add(pair.Key);
            }
        }

        foreach (EffectType type in listRemove) {
            _dictEffect.Remove(type);
        }
    }

    public float GetParam(EffectType eType) {
        if (_dictEffect.ContainsKey(eType)) {
            return _dictEffect[eType].GetEffectVal();
        }

        return 0.0f;
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
            _dictEffect.Remove(type);
        }
    }

    public void ResolveOne(EffectType eType) {
        if (!_dictEffect.ContainsKey(eType)) {
            return;
        }

        EffectObject effect = _dictEffect[eType];
        effect.Resolve();
        if (effect.IsEmpty()) {
            _dictEffect.Remove(eType);
        }
    }

    // For IEnumerable<EffectObject>
    public IEnumerator<EffectObject> GetEnumerator() { return _dictEffect.Values.GetEnumerator(); }
    IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
}
