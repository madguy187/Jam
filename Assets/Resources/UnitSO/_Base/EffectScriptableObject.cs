using UnityEngine;

public enum EffectTargetType {
    SELF,
    TEAM_DECK,
    ENEMY_DECK,
}

public enum EffectType {
    EFFECT_STAT_INCREASE_SHIELD,
    EFFECT_TAUNT,
    EFFECT_HEAL,
}

public enum EffectTempType {
    EFFECT_TEMP_TAUNT,
}

[CreateAssetMenu(menuName = "Scriptable Object/Unit/EffectScriptableObject")]
public class EffectScriptableObject : ScriptableObject {
    [SerializeField] EffectType _eType;
    [SerializeField] EffectTargetType _eTargetType;
    [SerializeField] float _fEffectVal = 0;
    [SerializeField] int _fEffectTurn = 0;

    public EffectTargetType GetTargetType() { return _eTargetType; }
    public int GetEffectTurn() { return _fEffectTurn; }

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
