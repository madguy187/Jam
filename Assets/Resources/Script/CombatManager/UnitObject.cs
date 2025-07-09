using System;
using UnityEngine;

public class UnitObject : MonoBehaviour {
    [field: SerializeField] public UnitScriptableObject unitSO { get; private set; }

    public int index { get; set; }

    float _currentHealth = 0.0f;
    float _currentShield = 0.0f;

    bool _bIsDead = false;
    public bool IsDead() { return _bIsDead; }

    public Action onDeath { private get; set; } = null;

    public void Init() {
        _currentHealth = unitSO.hp;
        _currentShield = unitSO.shield;
    }

    public void SetUnitSO(UnitScriptableObject _unitSO) {
        unitSO = _unitSO;
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

        float fDamageAfterShield = damage - _currentShield;
        if (fDamageAfterShield < 0) {
            _currentShield = Mathf.Abs(fDamageAfterShield);
            return;
        }

        _currentHealth -= fDamageAfterShield;
        if (_currentHealth <= 0.0f) {
            _TriggerDeath();
        }
    }

    public float GetHealthPercentage() {
        return _currentHealth / unitSO.hp;
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
