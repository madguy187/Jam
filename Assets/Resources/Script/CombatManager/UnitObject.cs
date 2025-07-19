using System;
using UnityEngine;

public enum eRollType {
    SINGLE,
    DIAGONAL,
    ZIGZAG,
    XSHAPE,
    FULLGRID,
    VERTICAL,
    HORIZONTAL
}

public class UnitObject : MonoBehaviour {
    [field: SerializeField] public UnitScriptableObject unitSO { get; private set; }
    [SerializeField] EffectList _listEffect;
    [SerializeField] EffectList _listSingleEffect;
    [SerializeField] EffectList _listDiagonalEffect;
    [SerializeField] EffectList _listZigZagEffect;
    [SerializeField] EffectList _listXShapeEffect;
    [SerializeField] EffectList _listFullGridEffect;

    public int index { get; set; }

    float _currentHealth = 0.0f;
    float _currentShield = 0.0f;
    eUnitPosition _ePosition = eUnitPosition.NONE;
    public bool IsFrontPosition() { return _ePosition == eUnitPosition.FRONT; }
    public void SetUnitPosition(eUnitPosition position) { _ePosition = position; }

    bool _bIsDead = false;
    public bool IsDead() { return _bIsDead; }

    public Action onDeath { private get; set; } = null;

    public void Init() {
        _currentHealth = unitSO.hp;
        _currentShield = unitSO.shield;
    }

    public float GetHealth() {
        return _currentHealth;
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

        float fFinalDamage = _GetDamageAfterShield(damage);
        _currentHealth -= fFinalDamage;
        if (_currentHealth <= 0.0f) {
            _TriggerDeath();
        }

        Global.DEBUG_PRINT("Final Damage=" + fFinalDamage);
    }

    public float GetHealthPercentage() {
        return _currentHealth / unitSO.hp;
    }

    public EffectList GetEffectList(eRollType eType) {
        switch (eType) {
            case eRollType.SINGLE:
                return _listSingleEffect;
            case eRollType.DIAGONAL:
                return _listDiagonalEffect;
            case eRollType.ZIGZAG:
                return _listZigZagEffect;
            case eRollType.XSHAPE:
                return _listXShapeEffect;
            case eRollType.FULLGRID:
                return _listFullGridEffect;
        }

        return null;
    }

    float _GetDamageAfterShield(float damage) {
        damage -= _currentShield;
        if (damage > 0) {
            _currentShield = 0;
            return damage;
        }

        _currentShield = Mathf.Abs(damage);
        return 0;
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
