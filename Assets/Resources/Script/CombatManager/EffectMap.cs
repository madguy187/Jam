using System.Collections.Generic;

public class EffectMap {
    Dictionary<EffectType, EffectObject> _dictEffect = new Dictionary<EffectType, EffectObject>();

    public void AddEffect(EffectType eType, EffectObject objEffect) {
        if (!_dictEffect.ContainsKey(eType)) {
            _dictEffect.Add(eType, objEffect);
        }
    }

    public void RemoveEffect(EffectType eType) {
        _dictEffect.Remove(eType);
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

    public void Resolve() {
        foreach (EffectObject effect in _dictEffect.Values) {
            effect.Resolve();

            if (effect.IsEmpty()) {
                RemoveEffect(effect.effectType);
            }
        }
    }
}
