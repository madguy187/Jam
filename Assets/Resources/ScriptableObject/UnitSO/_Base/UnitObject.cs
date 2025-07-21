using System;
using UnityEngine;

public class UnitObject : MonoBehaviour {
    [field: SerializeField] public UnitScriptableObject unitSO { get; private set; }
    [SerializeField] EffectList _listSingleEffect;
    [SerializeField] EffectList _listDiagonalEffect;
    [SerializeField] EffectList _listZigZagEffect;
    [SerializeField] EffectList _listXShapeEffect;
    [SerializeField] EffectList _listFullGridEffect;
    EffectList _listRelicEffect = new EffectList();

    EffectMap _listTempEffect = new EffectMap();

    public int index { get; set; }

    float _currentHealth = 0.0f;
    float _currentShield = 0.0f;
    eUnitPosition _ePosition = eUnitPosition.NONE;
    public bool IsFrontPosition() { return _ePosition == eUnitPosition.FRONT; }
    public eUnitPosition GetUnitPosition() { return _ePosition; }
    public void SetUnitPosition(eUnitPosition position) { _ePosition = position; }

    bool _bIsDead = false;
    public bool IsDead() { return _bIsDead; }

    public Action onDeath { private get; set; } = null;

    public void Init() {
        _currentHealth = unitSO.hp;
        _currentShield = unitSO.shield;
    }

    public void AddTempEffect(EffectType eType, float nVal, int nTurn) {
        EffectObject effect = new EffectObject();
        effect.effectType = eType;
        effect.Add(nVal, nTurn);

        _listTempEffect.AddEffect(eType, effect);
    }

    public void RemoveEffect(EffectType eType) {
        _listTempEffect.RemoveEffect(eType);
    }

    public float GetEffectParam(EffectType eType) {
        return _listTempEffect.GetParam(eType);
    }

    public float GetHealth() {
        return _currentHealth;
    }

    public void AddHealth(float health) {
        _currentHealth += health;
    }

    public float GetAttack() {
        return unitSO.attack;
    }

    public float GetShield() {
        return _currentShield;
    }

    public void AddShield(float shield) {
        _currentShield += shield;
    }

    public void SetShield(float shield) {
        _currentShield = shield;
    }

    public float GetRes() {
        return unitSO.res;
    }

    public int GetCritRate() {
        return unitSO.critRate;
    }

    public int GetCritMulti() {
        return unitSO.critMulti;
    }

    public void ReceiveDamage(float damage) {
        if (IsDead()) {
            return;
        }

        _currentHealth -= damage;
        if (_currentHealth <= 0.0f) {
            _TriggerDeath();
        }
    }

    public float GetHealthPercentage() {
        return _currentHealth / unitSO.hp;
    }

    public EffectList GetRollEffectList(MatchType eType) {
        switch (eType) {
            case MatchType.SINGLE:
                return _listSingleEffect;
            case MatchType.DIAGONAL:
                return _listDiagonalEffect;
            case MatchType.ZIGZAG:
                return _listZigZagEffect;
            case MatchType.XSHAPE:
                return _listXShapeEffect;
            case MatchType.FULLGRID:
                return _listFullGridEffect;
        }

        return null;
    }

    public void Resolve() {
        _listTempEffect.Resolve();
    }

    public void LoadRelic(RelicScriptableObject relicSO) {
        _listRelicEffect.LoadFromRelicSO(relicSO);
    }

    public void InitTempEffect() {
        _listTempEffect.Clear();

        if (!_listRelicEffect.IsValid()) {
            return;
        }

        foreach (EffectScriptableObject effect in _listRelicEffect) {
            if (effect.GetExecType() == EffectExecType.START_OF_ROUND) {
                AddTempEffect(effect.GetEffectType(), effect.GetEffectVal(), Global.TEMP_EFFECT_ONLY_THIS_ROUND);
            }
        }

    }

    void _TriggerDeath() {
        if (IsDead()) {
            return;
        }

        if (onDeath != null) {
            onDeath();
        }

        _bIsDead = true;
        Destroy(gameObject, 1.0f);
    }
}
