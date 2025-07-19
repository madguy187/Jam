using System.Collections.Generic;

public class EffectMap {
    const int NO_REMOVE = -1;
    Dictionary<EffectTempType, EffectObject> _dictEffect = new Dictionary<EffectTempType, EffectObject>();

    public void AddEffect(EffectTempType eType, EffectObject objEffect) {
        if (!_dictEffect.ContainsKey(eType)) {
            _dictEffect.Add(eType, objEffect);
        }
    }

    public void RemoveEffect(EffectTempType eType) {
        _dictEffect.Remove(eType);
    }

    public float GetParam(EffectTempType eType) {
        if (_dictEffect.ContainsKey(eType)) {
            return _dictEffect[eType].val;
        }

        return 0.0f;
    }

    public void Clear() {
        _dictEffect.Clear();
    }

    public void Resolve() {
        foreach (EffectObject effect in _dictEffect.Values) {
            if (effect.turn == NO_REMOVE) {
                continue;
            }
            
            effect.turn--;

            if (effect.turn < 0) {
                RemoveEffect(effect.effectType);
            }
        }
    }
}
