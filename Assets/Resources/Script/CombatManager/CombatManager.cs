using System.Collections.Generic;
using UnityEngine;

public class CombatManager : MonoBehaviour {
    enum eCombatState {
        WAIT,
        ATTACK,
        PAUSE,
        FINISH
    }

    public static CombatManager instance;

    CustomTimer _timer = null;
    [SerializeField] float fTimeBetweenEachUnitAttack = 1.0f;

    eCombatState _state = eCombatState.WAIT;
    eDeckType _eAttackerDeck = eDeckType.NONE;
    int _nAttackerIndex = Global.INVALID_INDEX;

    List<Match> _listMatch = null;

    void Awake() {
        if (instance != null) {
            Destroy(instance);
        }
        instance = this;
    }

    void Start() {
        _timer = new CustomTimer();
        _timer.SetFunc(_UnPause);
        _timer.SetTime(fTimeBetweenEachUnitAttack);
    }

    public void InitRound() {
        DeckManager.instance.InitDeckEffect();
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Alpha5)) {
            InitRound();
            StartBattleLoop(null);
        }

        switch (_state) {
            case eCombatState.WAIT:
                return;
            case eCombatState.ATTACK:
                _AttackUpdate();
                break;
            case eCombatState.PAUSE:
                _timer.Update();
                break;
            case eCombatState.FINISH:
                _state = eCombatState.WAIT;
                _listMatch = null;
                DeckManager.instance.ResolveTurnTempEffect();
                break;
        }
    }

    void _AttackUpdate() {
        if (_eAttackerDeck == eDeckType.NONE) {
            _state = eCombatState.FINISH;
            _timer.Reset();
            return;
        }

        Deck cDeck = DeckManager.instance.GetDeckByType(_eAttackerDeck);
        if (cDeck.IsValidUnitIndex(_nAttackerIndex)) {
            ExecBattle(_eAttackerDeck, _nAttackerIndex);
            _state = eCombatState.PAUSE;
            _timer.Reset();
        }

        if (!_UpdateAttackerIndex(cDeck)) {
            _UpdateAttackOrder();
        }
    }

    void _UpdateAttackOrder() {
        switch (_eAttackerDeck) {
            case eDeckType.NONE:
                _eAttackerDeck = eDeckType.PLAYER;
                break;
            case eDeckType.PLAYER:
                _eAttackerDeck = eDeckType.ENEMY;
                break;
            case eDeckType.ENEMY:
                _eAttackerDeck = eDeckType.NONE;
                break;
        }
        _nAttackerIndex = 0;
    }

    bool _UpdateAttackerIndex(Deck cDeck) {
        _nAttackerIndex++;

        int nMaxSize = cDeck.GetDeckMaxSize();
        if (_nAttackerIndex > nMaxSize) {
            return false;
        }

        for (int i = _nAttackerIndex; i < nMaxSize; i++) {
            if (cDeck.IsValidUnitIndex(i)) {
                _nAttackerIndex = i;
                return true;
            }
        }

        return false;
    }

    public void StartBattleLoop(List<Match> matches) {
        if (!_CanBattle()) {
            return;
        }

        _listMatch = matches;
        _state = eCombatState.ATTACK;
        _eAttackerDeck = eDeckType.PLAYER;
        _nAttackerIndex = 0;
    }

    void _UnPause() {
        if (_eAttackerDeck == eDeckType.NONE) {
            _state = eCombatState.FINISH;
        } else {
            _state = eCombatState.ATTACK;
        }
    }

    bool _CanBattle() {
        return _state == eCombatState.WAIT;
    }

    public void ExecBattle(eDeckType eType, int nAttackerIndex) {
        eDeckType eOtherType = eType == eDeckType.PLAYER ? eDeckType.ENEMY : eDeckType.PLAYER;
        Deck cDeck = DeckManager.instance.GetDeckByType(eType);
        Deck cOtherDeck = DeckManager.instance.GetDeckByType(eOtherType);

        int nTarget = GetLowestHealth(cOtherDeck);
        if (nTarget <= Global.INVALID_INDEX) {
            Global.DEBUG_PRINT("[ExecBattle] Cannot find target");
            return;
        }

        UnitObject cDefenderUnit = cOtherDeck.GetUnitObject(nTarget);
        if (cDefenderUnit == null || cDefenderUnit.IsDead()) {
            Global.DEBUG_PRINT("[ExecBattle] Defender deck is empty target=" + nTarget);
            return;
        }

        UnitObject cAttackerUnit = cDeck.GetUnitObject(nAttackerIndex);
        if (cAttackerUnit == null || cAttackerUnit.IsDead()) {
            Global.DEBUG_PRINT("[ExecBattle] Attacker deck is empty target=" + nAttackerIndex);
            return;
        }

        bool bCanAttack = true;
        if (cAttackerUnit.HasEffectParam(EffectType.EFFECT_FREEZE)) {
            bCanAttack = false;
        }

        cAttackerUnit.Resolve(EffectResolveType.RESOLVE_PRE_ATTACK);
        if (!bCanAttack) {
            return;
        }

        Global.DEBUG_PRINT("attacker_deck=" + eType + " attacker_index=" + nAttackerIndex + " defender_index=" + nTarget);
        _ExecBattleOne(cAttackerUnit, cDefenderUnit, _GetRollType(cAttackerUnit.unitSO.unitName));

        cAttackerUnit.Resolve(EffectResolveType.RESOLVE_ATTACK);
        cDefenderUnit.Resolve(EffectResolveType.RESOLVE_ATTACK);
    }

    List<MatchType> _GetRollType(string unitName) {
        List<MatchType> listResult = new List<MatchType>();
        if (_listMatch == null) {
            return listResult;
        }

        foreach (Match match in _listMatch) {
            if (match.GetUnitName() == unitName) {
                listResult.Add(match.GetMatchType());
            }
        }

        return listResult;
    }

    void _ExecBattleOne(UnitObject cAttackerUnit, UnitObject cDefenderUnit, List<MatchType> listRoll) {
        if (cAttackerUnit == null || cAttackerUnit.IsDead()) {
            return;
        }

        if (cDefenderUnit == null || cDefenderUnit.IsDead()) {
            return;
        }

        foreach (MatchType eRoll in listRoll) {
            EffectList effects = cAttackerUnit.GetRollEffectList(eRoll);
            if (effects != null) {
                foreach (EffectScriptableObject effect in effects) {
                    _ActivateRollEffect(effect);
                }
            }
        }

        float fAttack = cAttackerUnit.GetAttack();
        if (_IsCrit(cAttackerUnit)) {
            fAttack *= _GetCritRatio(cAttackerUnit);
        }

        fAttack *= _GetResRatio(cDefenderUnit);

        fAttack = _GetDamageAfterShield(cDefenderUnit, fAttack);

        if (cDefenderUnit.GetEffectParam(EffectType.EFFECT_REFLECT, out float val)) {
            float fReflect = _GetDamageAfterShield(cAttackerUnit, fAttack);
            cAttackerUnit.ReceiveDamage(fReflect * val / Global.PERCENTAGE_CONSTANT);
        }

        fAttack = Mathf.Floor(fAttack);
        cDefenderUnit.ReceiveDamage(fAttack);

        _ActivatePostEffect(cAttackerUnit, fAttack);
        
        Global.DEBUG_PRINT("Final Damage=" + fAttack);
    }

    float _GetDamageAfterShield(UnitObject cUnit, float damage) {
        float cCurrentShield = cUnit.GetShield();
        damage -= cCurrentShield;
        if (damage > 0) {
            cUnit._currentShield.SetVal(0);
            return damage;
        }

        cUnit._currentShield.SetVal(Mathf.Abs(damage));

        return 0;
    }

    public int GetLowestHealth(Deck cDeck) {
        int nIndex = Global.INVALID_INDEX;
        float nLowestHealth = Mathf.Infinity;
        bool bHasFrontUnit = cDeck.HasFrontUnit();

        foreach (UnitObject unit in cDeck) {
            if (unit == null) {
                continue;
            }

            if (unit.IsDead()) {
                continue;
            }

            float nHealth = unit.GetHealth();
            if (nHealth > nLowestHealth) {
                continue;
            }

            if (unit.HasEffectParam(EffectType.EFFECT_TAUNT)) {
                nIndex = unit.index;
                continue;
            }

            if (unit.HasEffectParam(EffectType.EFFECT_INVISIBLITY)) {
                continue;
            }

            if (bHasFrontUnit && !unit.IsFrontPosition()) {
                continue;
            }

            nIndex = unit.index;
            nLowestHealth = nHealth;
        }

        return nIndex;
    }

    bool _IsCrit(UnitObject cAttackerUnit) {
        int nCritRate = (int)cAttackerUnit._currentCritRate.GetVal();
        if (cAttackerUnit.GetEffectParam(EffectType.EFFECT_STAT_INCREASE_CRIT_RATE_TURN, out float val)) {
            nCritRate += (int)val;
        }
        
        if (cAttackerUnit.HasEffectParam(EffectType.EFFECT_DAMAGE_CRIT_HIT)) {
            return true;
        }
        
        int nRand = Random.Range(0, Global.PERCENTAGE_CONSTANT + 1); // +1 coz exclusive
        if (nRand < nCritRate) {
            return true;
        }

        return false;
    }

    float _GetCritRatio(UnitObject cAttackerUnit) {
        int critMulti = cAttackerUnit.GetCritMulti();
        if (cAttackerUnit.GetEffectParam(EffectType.EFFECT_STAT_INCREASE_CRIT_MULTI_TURN, out float val)) {
            critMulti += (int)val;
        }

        return critMulti / Global.PERCENTAGE_CONSTANT;
    }

    float _GetResRatio(UnitObject cDefenderUnit) {
        return Global.PERCENTAGE_CONSTANT / (Global.PERCENTAGE_CONSTANT + cDefenderUnit.GetRes() * Global.RESISTANCE_PERCENTAGE_CONSTANT);
    }

    void _ActivateRollEffect(EffectScriptableObject cEffect) {
        EffectTargetType eTargetType = cEffect.GetTargetType();
        List<UnitObject> arrTargetUnit = _GetTargetUnitBasedOnTargetType(eTargetType, cEffect);

        foreach (UnitObject unit in arrTargetUnit) {
            if (unit == null) {
                continue;
            }

            if (cEffect.IsEffectType(EffectType.EFFECT_STAT_INCREASE_ATK)) {
                unit._currentAttack.AddVal(cEffect.GetEffectVal());
            }
            if (cEffect.IsEffectType(EffectType.EFFECT_STAT_REDUCE_ATK)) {
                unit._currentAttack.MinusVal(cEffect.GetEffectVal());
            }

            if (cEffect.IsEffectType(EffectType.EFFECT_STAT_INCREASE_SHIELD)) {
                unit._currentShield.AddVal(cEffect.GetEffectVal());
            }

            if (cEffect.IsEffectType(EffectType.EFFECT_STAT_INCREASE_RES)) {
                unit._currentRes.AddVal(cEffect.GetEffectVal());
            }

            if (cEffect.IsEffectType(EffectType.EFFECT_DAMAGE_IGNORE_SHIELD)) {
                unit._currentHealth.MinusVal(cEffect.GetEffectVal());
            }

            if (cEffect.IsEffectType(EffectType.EFFECT_HEAL)) {
                unit._currentHealth.AddVal(cEffect.GetEffectVal());
            }

            if (cEffect.IsEffectType(EffectType.EFFECT_STAT_INCREASE_CRIT_RATE)) {
                unit._currentCritRate.AddVal(cEffect.GetEffectVal());
            }
            if (cEffect.IsEffectType(EffectType.EFFECT_STAT_REDUCE_CRIT_RATE)) {
                unit._currentCritRate.MinusVal(cEffect.GetEffectVal());
            }

            if (cEffect.IsEffectType(EffectType.EFFECT_REVIVE)) {
                unit.Revive();
                unit._currentHealth.SetVal(cEffect.GetEffectVal());
            }

            if (cEffect.IsEffectType(EffectType.EFFECT_CLEANSE)) {
                unit.RemoveTempEffectByPredicate(delegate (EffectObject obj) {
                    if (obj.GetEffectAffinityType() == EffectAffinityType.NEGATIVE) {
                        return true;
                    }

                    return false;
                });
            }

            if (cEffect.GetExecType() == EffectExecType.TURN_SPECIFIED) {
                unit.AddTempEffect(cEffect);
            }

            Global.DEBUG_PRINT("[Effect] Triggered " + cEffect.GetTypeName() +
                                " val=" + cEffect.GetEffectVal() +
                                " unit_index=" + unit.index);
        }
    }

    void _ActivatePostEffect(UnitObject unitObj, float fAttack) {
        EffectMap mapTempEffect = unitObj.GetAllTempEffect();
        foreach (EffectObject effectObj in mapTempEffect) {
            EffectTargetType eTargetType = effectObj.GetEffectTargetType();
            List<UnitObject> arrTargetUnit = _GetTargetUnitBasedOnTargetType(eTargetType, effectObj.effectSO);

            if (unitObj == null) {
                return;
            }

            foreach (UnitObject target in arrTargetUnit) {
                if (target == null) {
                    continue;
                }

                if (unitObj.HasEffectParam(EffectType.EFFECT_ATTACK_HEAL)) {
                    target._currentHealth.AddVal(fAttack);
                }

                if (unitObj.HasEffectParam(EffectType.EFFECT_DOUBLE_DAMAGE)) {
                    target._currentHealth.MinusVal(fAttack);
                }
            }
        }

        unitObj.Resolve(EffectResolveType.RESOLVE_POST_ATTACK);
    }

    List<UnitObject> _GetTargetUnitBasedOnTargetType(EffectTargetType eTargetType, EffectScriptableObject cEffect) {
        List<UnitObject> arrTarget = new List<UnitObject>();
        Deck cDeck = DeckManager.instance.GetDeckByType(_eAttackerDeck);

        eDeckType eOtherType = _eAttackerDeck == eDeckType.PLAYER ? eDeckType.ENEMY : eDeckType.PLAYER;
        Deck cOtherDeck = DeckManager.instance.GetDeckByType(eOtherType);

        if (cDeck == null || cOtherDeck == null) {
            return arrTarget;
        }

        switch (eTargetType) {
            case EffectTargetType.SELF:
                arrTarget.Add(cDeck.GetUnitObject(_nAttackerIndex));
                break;
            case EffectTargetType.TEAM_LOWEST_HP: {
                    int nLowestHealthIndex = GetLowestHealth(cDeck);
                    arrTarget.Add(cDeck.GetUnitObject(nLowestHealthIndex));
                    break;
                }
            case EffectTargetType.TEAM_HOLY:
                arrTarget = _GetUnitByArchetype(cDeck, eUnitArchetype.HOLY);
                break;
            case EffectTargetType.TEAM_DECK:
                arrTarget = cDeck.GetAllAliveUnit();
                break;
            case EffectTargetType.TEAM_DEAD:
                arrTarget = _GetAllDeadUnit(cDeck);
                break;
            case EffectTargetType.ENEMY_DECK:
                arrTarget = cOtherDeck.GetAllAliveUnit();
                break;
            case EffectTargetType.ENEMY_LOWEST_HP: {
                    int nLowestHealthIndex = GetLowestHealth(cOtherDeck);
                    arrTarget.Add(cOtherDeck.GetUnitObject(nLowestHealthIndex));
                    break;
                }
            case EffectTargetType.ENEMY_FRONTLINE:
                arrTarget = _GetAllUnitFrontLine(cOtherDeck);
                break;
            case EffectTargetType.ENEMY_RANDOM:
                arrTarget = _GetRandomEnemy(cOtherDeck, 1);
                break;
            case EffectTargetType.ENEMY_RANDOM_3:
                arrTarget = _GetRandomEnemy(cOtherDeck, 3);
                break;
            case EffectTargetType.ENEMY_BELOW_HP:
                arrTarget = _GetAllUnitBelowHp(cOtherDeck, cEffect.GetEffectVal());
                break;

        }

        return arrTarget;
    }

    List<UnitObject> _GetUnitByArchetype(Deck cDeck, eUnitArchetype eUnitArchetype) {
        return cDeck.GetUnitByPredicate(delegate (UnitObject unit) {
            if (unit == null) {
                return false;
            }

            if (unit.IsDead()) {
                return false;
            }

            if (unit.unitSO.eUnitArchetype != eUnitArchetype) {
                return false;
            }

            return true;
        });
    }

    List<UnitObject> _GetAllDeadUnit(Deck cDeck) {
        return cDeck.GetUnitByPredicate(delegate (UnitObject unit) {
            if (unit == null) {
                return false;
            }

            if (!unit.IsDead()) {
                return false;
            }

            return true;
        });
    }

    List<UnitObject> _GetAllUnitFrontLine(Deck cDeck) {
        return cDeck.GetUnitByPredicate(delegate (UnitObject unit) {
            if (unit == null) {
                return false;
            }

            if (!unit.IsDead()) {
                return false;
            }

            if (!unit.IsFrontPosition()) {
                return false;
            }

            return true;
        });
    }

    List<UnitObject> _GetAllUnitBelowHp(Deck cDeck, float hp) {
        return cDeck.GetUnitByPredicate(delegate (UnitObject unit) {
            if (unit == null) {
                return false;
            }

            if (!unit.IsDead()) {
                return false;
            }

            if (unit._currentHealth.GetVal() > hp) {
                return false;
            }

            return true;
        });
    }

    List<UnitObject> _GetRandomEnemy(Deck cDeck, int count) {
        int i = 0;
        return cDeck.GetUnitByPredicate(delegate (UnitObject unit) {
            if (unit == null) {
                return false;
            }

            if (!unit.IsDead()) {
                return false;
            }

            if (i == count) {
                return false;
            }

            i++;

            return true;
        });
    }
}
