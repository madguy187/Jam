using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public class UnitStat {
    [SerializeField] float val = 0.0f;
    [SerializeField] float max = 0.0f;
    float tempMax = 0.0f;

    public UnitStat(float _val) { val = _val; }

    public void SetMax(float _max) { max = _max; }
    public void AddTempMax(float _tempMax) { tempMax = _tempMax; }

    public void Reset() {
        val = max;
        tempMax = 0.0f;
    }

    public void SetVal(float _val) {
        float _max = GetMax();
        if (_max > 0 && _val > _max) {
            _val = _max;
        }

        if (_val < 0) {
            _val = 0;
        }

        val = _val;
    }

    public float GetVal() { return val; }
    public float GetMax() { return max + tempMax; }

    public void AddVal(float _val) {
        float _max = GetMax();
        if (_max > 0 && val + _val > _max) {
            _val = _max - val;
        }
        val += _val;
    }
    public void MinusVal(float _val) {
        if (val - _val < 0) {
            _val = _val - val;
        }
        val -= _val;
    }
}

public class UnitObject : MonoBehaviour {
#if UNITY_EDITOR
    public void SetUnitSO(UnitScriptableObject _unitSO) { unitSO = _unitSO; }
    public void SetEffectList_Single(EffectList _list) { _listSingleEffect = _list; }
    public void SetEffectList_Horizontal(EffectList _list) { _listHorizontalEffect = _list; }
    public void SetEffectList_XShape(EffectList _list) { _listXShapeEffect = _list; }
    public void SetEffectList_Diagonal(EffectList _list) { _listDiagonalEffect = _list; }
    public void SetEffectList_ZigZag(EffectList _list) { _listZigZagEffect = _list; }
    public void SetEffectList_FullGrid(EffectList _list) { _listFullGridEffect = _list; }
#endif


    [field: SerializeField] public UnitScriptableObject unitSO { get; private set; }
    [SerializeField] EffectList _listSingleEffect;
    [SerializeField] EffectList _listHorizontalEffect;
    [SerializeField] EffectList _listDiagonalEffect;
    [SerializeField] EffectList _listZigZagEffect;
    [SerializeField] EffectList _listXShapeEffect;
    [SerializeField] EffectList _listFullGridEffect;
    EffectList _listRelicEffect = new EffectList();

    Dictionary<string, RelicScriptableObject> _listRelicSO = new Dictionary<string, RelicScriptableObject>();
    public void AddRelic(RelicScriptableObject relicSO) {
        if (_listRelicSO.ContainsKey(relicSO.GetRelicName())) {
            return;
        }

        _listRelicSO.Add(relicSO.GetRelicName(), relicSO);
    }

    public bool RemoveRelic(RelicScriptableObject relicSO) {
        if (_listRelicSO.ContainsKey(relicSO.GetRelicName())) {
            _listRelicSO.Remove(relicSO.GetRelicName());
            return true;
        }

        return false;
    }

    public List<RelicScriptableObject> GetRelic() {
        return _listRelicSO.Values.ToList();
    }

    [SerializeField] EffectMap _listTempEffect = new EffectMap();

    public int index { get; set; }
    public int death_count { get; set; } = 0;

    public UnitStat _currentHealth = new UnitStat(0.0f);
    public UnitStat _currentAttack = new UnitStat(0.0f);
    public UnitStat _currentShield = new UnitStat(0.0f);
    public UnitStat _currentRes = new UnitStat(0.0f);
    public UnitStat _currentCritRate = new UnitStat(0.0f);
    public UnitStat _currentCritMulti = new UnitStat(0.0f);

    eUnitPosition _ePosition = eUnitPosition.NONE;
    public bool IsFrontPosition() { return _ePosition == eUnitPosition.FRONT; }
    public eUnitPosition GetUnitPosition() { return _ePosition; }
    public void SetUnitPosition(eUnitPosition position) { _ePosition = position; }

    bool _bIsDead = false;
    public bool IsDead() { return _bIsDead; }

    public Action onDeath { private get; set; } = null;

    SPUM_Prefabs _prefabs = null;

    public void Awake() {
        _prefabs = GetComponent<SPUM_Prefabs>();
        _prefabs.OverrideControllerInit();

        DontDestroyOnLoad(gameObject);
    }

    public void Update() {

    }

    public void Init(bool isEnemy = false) {
        float hp = unitSO.hp;
        if (isEnemy) {
            hp *= EnemyManager.instance.GetDifficultyHealthPercent(IsBoss());
        }
        _currentHealth.SetVal(hp);
        _currentHealth.SetMax(hp);

        float atk = unitSO.attack;
        if (isEnemy) {
            atk += EnemyManager.instance.GetDifficultyAtk();
        }
        _currentAttack.SetVal(atk);
        _currentShield.SetVal(unitSO.shield);
        _currentRes.SetVal(unitSO.res);

        int critRate = unitSO.critRate;
        if (isEnemy) {
            critRate += EnemyManager.instance.GetDifficultyCritRate();
        }
        _currentCritRate.SetVal(critRate);

        _currentCritMulti.SetVal(unitSO.critMulti);
    }

    public void AddTempEffect(EffectScriptableObject effectSO) {
        EffectObject effect = new EffectObject();
        effect.effectSO = effectSO;
        effect.Add(effectSO.GetEffectVal(), effectSO.GetEffectCount());

        _listTempEffect.AddEffect(effectSO.GetEffectType(), effect);
    }

    public void AddTempEffect(EffectType effectType, float effectVal, EffectTargetType eTarget, EffectTargetCondition eCondition, float eParam, EffectAffinityType eAffinity, EffectResolveType eResolve, int nCount) {
        EffectScriptableObject effectSO = ScriptableObject.CreateInstance<EffectScriptableObject>();
        effectSO.InitScriptableInstance(effectType, effectVal, eTarget, eCondition, eParam, EffectExecType.COUNT_SPECIFIED, eAffinity, eResolve, nCount);
        AddTempEffect(effectSO);
    }

    public void RemoveEffect(EffectType eType) {
        _listTempEffect.RemoveEffect(eType);
    }

    public void RemoveTempEffectByPredicate(Predicate<EffectObject> predicate) {
        _listTempEffect.RemoveEffectByPredicate(predicate);
    }

    public EffectMap GetAllTempEffect() {
        return _listTempEffect;
    }

    public bool GetEffectParam(EffectType eType, out float val) {
        val = _listTempEffect.GetParam(eType);
        if (val > 0) {
            return true;
        }

        return false;
    }

    public bool GetEffectSO(EffectType eType, out EffectScriptableObject val) {
        val = _listTempEffect.GetEffectSO(eType);
        if (val != null) {
            return true;
        }

        return false;
    }

    public bool HasEffectParam(EffectType eType, bool bShouldResolveIfTrue = false) {
        float val = _listTempEffect.GetParam(eType);
        if (val > 0) {
            if (bShouldResolveIfTrue) {
                _listTempEffect.ResolveOne(eType);
            }
            return true;
        }

        return false;
    }

    public float GetHealth() {
        return _currentHealth.GetVal();
    }

    public float GetAttack() {
        return unitSO.attack;
    }

    public float GetShield() {
        return _currentShield.GetVal();
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

        _currentHealth.MinusVal(damage);
        if (_currentHealth.GetVal() <= Mathf.Epsilon) {
            _TriggerDeath();
        }
    }

    public float GetHealthPercentage() {
        return _currentHealth.GetVal() / unitSO.hp;
    }

    public EffectList GetRollEffectList(MatchType eType) {
        switch (eType) {
            case MatchType.SINGLE:
                return _listSingleEffect;
            case MatchType.HORIZONTAL:
                return _listHorizontalEffect;
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

    public EffectList GetRelicEffectList() {
        return _listRelicEffect;
    }

    public void Resolve(EffectResolveType eResolveType) {
        _listTempEffect.Resolve(eResolveType);
    }

    public void LoadRelic() {
        _listRelicEffect.Clear();
        foreach (KeyValuePair<string, RelicScriptableObject> pair in _listRelicSO) {
            RelicScriptableObject relicSO = pair.Value;
            _listRelicEffect.LoadFromRelicSO(relicSO);
        }
    }

    public void Revive() {
        _bIsDead = false;
        PlayStateAnimation(PlayerState.IDLE);
    }

    void _TriggerDeath() {
        if (IsDead()) {
            return;
        }

        if (onDeath != null) {
            onDeath();
        }

        PlayStateAnimation(PlayerState.DEATH);
        _bIsDead = true;
    }

    public void PlayStateAnimation(PlayerState state) {
        if (_prefabs == null) {
            return;
        }

        _prefabs.PlayAnimation(state, 0);
    }

    public bool IsBoss() {
        return unitSO.eUnitArchetype == eUnitArchetype.MOB && unitSO.eTier == eUnitTier.STAR_3; 
    }
}
