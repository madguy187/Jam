using UnityEngine;

public enum EffectType {
    EFFECT_STAT_INCREASE_ATK,
    EFFECT_STAT_INCREASE_CRIT_RATE,
    EFFECT_STAT_INCREASE_CRIT_DAMAGE,
    EFFECT_STAT_INCREASE_SHIELD,
    EFFECT_HEAL,
}

[CreateAssetMenu(menuName = "Scriptable Object/Unit/EffectScriptableObject")]
public class EffectScriptableObject : ScriptableObject {
    [SerializeField] EffectType _eType;
    [SerializeField] float _fEffectVal;

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
