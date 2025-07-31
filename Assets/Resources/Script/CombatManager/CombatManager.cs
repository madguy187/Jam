using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    string sceneNameOnFinish = "";

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

        Deck cPlayerDeck = DeckManager.instance.GetDeckByType(eDeckType.PLAYER);
        List<UnitObject> listObjectPlayer = DeckHelperFunc.GetAllAliveUnit(cPlayerDeck);
        foreach (UnitObject unit in listObjectPlayer) {
            EffectList listEffect = unit.GetRelicEffectList();
            foreach (EffectScriptableObject effectSO in listEffect) {
                _ActivateEffect(cPlayerDeck, effectSO);
            }
        }

        Deck cEnemyDeck = DeckManager.instance.GetDeckByType(eDeckType.ENEMY);
        List<UnitObject> listObjectEnemy = DeckHelperFunc.GetAllAliveUnit(cEnemyDeck);
        foreach (UnitObject unit in listObjectEnemy) {
            EffectList listEffect = unit.GetRelicEffectList();
            foreach (EffectScriptableObject effectSO in listEffect) {
                _ActivateEffect(cPlayerDeck, effectSO);
            }
        }
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

    void Update() {
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
                _CheckEnd();
                break;
        }
    }

    void _CheckEnd() {
        List<UnitObject> listPlayerUnit = DeckHelperFunc.GetAllAliveUnit(DeckManager.instance.GetDeckByType(eDeckType.PLAYER));
        List<UnitObject> listEnemyUnit = DeckHelperFunc.GetAllAliveUnit(DeckManager.instance.GetDeckByType(eDeckType.ENEMY));
        Deck cPlayerDeck = DeckManager.instance.GetDeckByType(eDeckType.PLAYER);      
        if (listPlayerUnit.Count <= 0) {
            // All player units dead – show defeat popup.
            ResultPopup.instance.ShowDefeat(cPlayerDeck);
            cPlayerDeck.DestroyAllUnit();
            Deck cEnemyDeck = DeckManager.instance.GetDeckByType(eDeckType.ENEMY);
            cEnemyDeck.DestroyAllUnit();
        }
        else if (listEnemyUnit.Count <= 0) {
            Deck cEnemyDeck = DeckManager.instance.GetDeckByType(eDeckType.ENEMY);
            ResultPopup.instance.ShowVictory(cPlayerDeck);
            cEnemyDeck.DestroyAllUnit();
            sceneNameOnFinish = "Game_Map";
            UIFade.instance.SetOnFadeFinish(_SceneChangeFunc);
        }
    }

    void _SceneChangeFunc() {
        UIFade.instance.FadeIn(2.0f);
        SceneManager.LoadScene(sceneNameOnFinish);
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
        eDeckType[] arrType = (eDeckType[])Enum.GetValues(typeof(eDeckType));
        int nTypeCount = arrType.Count();
        _eAttackerDeck++;

        if ((int)_eAttackerDeck >= nTypeCount) {
            _eAttackerDeck = eDeckType.NONE;
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

    void _UnPause() {
        if (_eAttackerDeck == eDeckType.NONE) {
            _state = eCombatState.FINISH;
        } else {
            _state = eCombatState.ATTACK;
        }
    }

    public bool IsRunning() {
        return _state != eCombatState.WAIT;
    }

    bool _CanBattle() {
        return _state == eCombatState.WAIT;
    }

    public void ExecBattle(eDeckType eType, int nAttackerIndex) {
        Deck cDeck = DeckManager.instance.GetDeckByType(eType);

        UnitObject cAttackerUnit = cDeck.GetUnitObject(nAttackerIndex);
        if (cAttackerUnit == null || cAttackerUnit.IsDead()) {
            Global.DEBUG_PRINT("[ExecBattle] Attacker deck is empty target=" + nAttackerIndex);
            return;
        }

        bool bCanAttack = true;
        if (cAttackerUnit.HasEffectParam(EffectType.EFFECT_FREEZE)) {
            bCanAttack = false;
        }
        if (cAttackerUnit.HasEffectParam(EffectType.EFFECT_STUN)) {
            bCanAttack = false;
        }

        cAttackerUnit.Resolve(EffectResolveType.RESOLVE_PRE_ATTACK);
        if (!bCanAttack) {
            return;
        }

        _ExecRoll(cDeck, cAttackerUnit, _GetRollType(cAttackerUnit.unitSO.unitName));
        _ExecTempEffect(cDeck, cAttackerUnit);

        int nAttackCount = 1;
        if (cAttackerUnit.GetEffectParam(EffectType.EFFECT_ATTACK_TIMES, out float fAttackCount)) {
            nAttackCount = (int)fAttackCount;
        }

        bool bBreak = false;
        if (cAttackerUnit.GetEffectParam(EffectType.EFFECT_ATTACK_MULTIPLE_IF_DEAD, out float fAttackCountIfDead)) {
            nAttackCount = (int)fAttackCountIfDead;
            bBreak = true;
        }

        for (int i = 0; i < nAttackCount; i++) {
            // Normal attack target type
            // using EffectTargetType coz i lazy change already
            int nAttackCount_SameTarget = 1;
            int nTargetCount = 1;

            EffectTargetCondition eTargetCondition = EffectTargetCondition.LOWEST_HP;
            if (cAttackerUnit.HasEffectParam(EffectType.EFFECT_ATTACK_MULTIPLE)) {
                eTargetCondition = _GetTargetConditionFromTempEffect(cAttackerUnit, EffectType.EFFECT_ATTACK_MULTIPLE);
                if (cAttackerUnit.GetEffectParam(EffectType.EFFECT_ATTACK_MULTIPLE, out float nAttackTargetNum)) {
                    nTargetCount = (int)nAttackTargetNum;
                }
            }
            if (cAttackerUnit.HasEffectParam(EffectType.EFFECT_ATTACK_MULTIPLE_IF_DEAD)) {
                eTargetCondition = _GetTargetConditionFromTempEffect(cAttackerUnit, EffectType.EFFECT_ATTACK_MULTIPLE_IF_DEAD);
                if (cAttackerUnit.GetEffectParam(EffectType.EFFECT_ATTACK_MULTIPLE_IF_DEAD, out float nAttackCount_SameTargetEffect)) {
                    nAttackCount_SameTarget = (int)nAttackCount_SameTargetEffect;
                }
            }

            List<UnitObject> listTarget = _GetTargetUnitBasedOnTargetType(cDeck, EffectTargetType.ENEMY, eTargetCondition);

            bool bDefenderDied = false;
            foreach (UnitObject cDefenderUnit in listTarget) {
                if (nTargetCount <= 0) {
                    break;
                }

                if (cDefenderUnit == null || cDefenderUnit.IsDead()) {
                    Global.DEBUG_PRINT("[ExecBattle] Defender deck is empty");
                    continue;
                }

                Global.DEBUG_PRINT("attacker_deck=" + eType + " attacker_index=" + nAttackerIndex + " defender_index=" + cDefenderUnit.index);
                for (int j = 0; j < nAttackCount_SameTarget; j++) {
                    _ExecBattleOne(cAttackerUnit, cDefenderUnit);
                }

                if (cDefenderUnit.IsDead()) {
                    bDefenderDied = true;
                }

                cAttackerUnit.Resolve(EffectResolveType.RESOLVE_ATTACK);
                nTargetCount--;
            }

            if (bBreak && !bDefenderDied) {
                break;
            }
        }

        cAttackerUnit.PlayStateAnimation(PlayerState.ATTACK);
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

    void _ExecRoll(Deck cAttackerDeck, UnitObject cAttackerUnit, List<MatchType> listRoll) {
        if (cAttackerUnit == null || cAttackerUnit.IsDead()) {
            return;
        }

        foreach (MatchType eRoll in listRoll) {
            EffectList effects = cAttackerUnit.GetRollEffectList(eRoll);
            if (effects != null) {
                foreach (EffectScriptableObject effect in effects) {
                    _ActivateEffect(cAttackerDeck, effect);
                }
            }
        }
    }

    void _ExecTempEffect(Deck cDeck, UnitObject cUnit) {
        if (cUnit == null || cUnit.IsDead()) {
            return;
        }

        EffectMap effects = cUnit.GetAllTempEffect();
        foreach (EffectObject effectObj in effects) {
            _ActivateTempEffect(cDeck, effectObj);
        }
    }

    void _ExecBattleOne(UnitObject cAttackerUnit, UnitObject cDefenderUnit) {
        float fAttack = cAttackerUnit.GetAttack();
        if (cAttackerUnit.GetEffectParam(EffectType.EFFECT_STAT_INCREASE_ATK_TURN, out float fBonus)) {
            fAttack += fBonus;
        }

        bool bIsCrit = false;
        if (_IsCrit(cAttackerUnit)) {
            fAttack *= _GetCritRatio(cAttackerUnit);
            bIsCrit = true;
        }

        if (bIsCrit) {
            if (cAttackerUnit.HasEffectParam(EffectType.EFFECT_DAMAGE_CRIT_FLAT_HEAL)) {
                if (cDefenderUnit.GetEffectParam(EffectType.EFFECT_DAMAGE_CRIT_FLAT_HEAL, out float val)) {
                    cAttackerUnit._currentHealth.AddVal(val);
                }   
            }
        }

        bool bIsIgnoreRes = false;
        if (cAttackerUnit.HasEffectParam(EffectType.EFFECT_IGNORE_RES)) {
            bIsIgnoreRes = true;
        }

        if (!bIsIgnoreRes) {
            fAttack *= _GetResRatio(cDefenderUnit);
        }

        bool bIgnoreShield = false;
        if (cAttackerUnit.HasEffectParam(EffectType.EFFECT_IGNORE_SHIELD)) {
            bIgnoreShield = true;
        }

        if (!bIgnoreShield) {
            fAttack = _GetDamageAfterShield(cDefenderUnit, fAttack);
        }

        if (cAttackerUnit.GetEffectParam(EffectType.EFFECT_HEX, out float fReducePercent)) {
            fAttack *= fReducePercent * Global.PERCENTAGE_CONSTANT;
        }

        if (cAttackerUnit.GetEffectParam(EffectType.EFFECT_SHADOW_BLADE_BLEED, out float fApplyCount)) {
            if (cDefenderUnit.HasEffectParam(EffectType.EFFECT_BLEED)) {
                fAttack += 5;
                cDefenderUnit.AddTempEffect(EffectType.EFFECT_BLEED, 2.0f, EffectTargetType.SELF, EffectTargetCondition.NONE, 0.0f, EffectAffinityType.NONE, EffectResolveType.RESOLVE_TURN, 2);
            } else {
                fAttack += 2;
                cDefenderUnit.AddTempEffect(EffectType.EFFECT_BLEED, 2.0f, EffectTargetType.SELF, EffectTargetCondition.NONE, 0.0f, EffectAffinityType.NONE, EffectResolveType.RESOLVE_TURN, 3);
            }
        }

        if (cDefenderUnit.HasEffectParam(EffectType.EFFECT_DODGE)) {
            if (cDefenderUnit.GetEffectParam(EffectType.EFFECT_DODGE, out float val)) {
                int rand = UnityEngine.Random.Range(0, Global.PERCENTAGE_CONSTANT + 1);
                if (rand < val / Global.PERCENTAGE_CONSTANT) {
                    fAttack = 0.0f;
                }
            }
        }

        fAttack = Mathf.Floor(fAttack);

        if (cDefenderUnit.HasEffectParam(EffectType.EFFECT_REFLECT)) {
            if (cDefenderUnit.GetEffectParam(EffectType.EFFECT_REFLECT, out float val)) {
                float fReflect = _GetDamageAfterShield(cAttackerUnit, fAttack);
                cAttackerUnit.ReceiveDamage(fReflect * val / Global.PERCENTAGE_CONSTANT);
            }
        }
        
        if (cAttackerUnit.GetEffectParam(EffectType.EFFECT_DAMAGE_MULTIPLY, out float damageMulti)) {
            fAttack *= damageMulti;
        }

        if (cDefenderUnit.HasEffectParam(EffectType.EFFECT_ZERO_DAMAGE)) {
            fAttack = 0.0f;
        }

        if (cAttackerUnit.HasEffectParam(EffectType.EFFECT_DAMAGE_LIFESTEAL)) {
            if (cAttackerUnit.GetEffectParam(EffectType.EFFECT_DAMAGE_LIFESTEAL, out float val)) {
                cAttackerUnit._currentHealth.AddVal(fAttack * val / Global.PERCENTAGE_CONSTANT);
            }
        }

        if (cDefenderUnit.HasEffectParam(EffectType.EFFECT_CANNOT_DIE)) {
            if (cDefenderUnit._currentHealth.GetVal() <= fAttack) {
                fAttack = cDefenderUnit._currentHealth.GetVal() - 1;
            }
        }

        cDefenderUnit.ReceiveDamage(fAttack);

        _ActivatePostEffect(cAttackerUnit, fAttack);

        ResourceManager.instance.GenerateDynamicText(cDefenderUnit.transform.position, fAttack.ToString());
        AudioManager.instance.Play("Attack");

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

        int nRand = UnityEngine.Random.Range(0, Global.PERCENTAGE_CONSTANT + 1); // +1 coz exclusive
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

    void _ActivateEffect(Deck cDeck, EffectScriptableObject cEffect) {
        List<UnitObject> arrTargetUnit = _GetTargetUnitBasedOnTargetType(cDeck, cEffect);

        foreach (UnitObject unit in arrTargetUnit) {
            if (unit == null) {
                continue;
            }

            if (unit.IsDead()) {
                continue;
            }

            if (cEffect.GetExecType() == EffectExecType.COUNT_SPECIFIED) {
                unit.AddTempEffect(cEffect);
                return;
            }

            if (cEffect.IsEffectType(EffectType.EFFECT_STAT_INCREASE_BASE_HP)) {
                unit._currentHealth.AddTempMax(cEffect.GetEffectVal());
                unit._currentHealth.AddVal(cEffect.GetEffectVal());
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

            if (cEffect.IsEffectType(EffectType.EFFECT_SHIELD_REMOVE)) {
                unit._currentShield.MinusVal(unit._currentShield.GetVal());
            }

            if (cEffect.IsEffectType(EffectType.EFFECT_STAT_INCREASE_RES)) {
                unit._currentRes.AddVal(cEffect.GetEffectVal());
            }
            if (cEffect.IsEffectType(EffectType.EFFECT_STAT_REDUCE_RES_PERCENT)) {
                float fReduceVal = unit._currentRes.GetVal() * cEffect.GetEffectVal() * Global.PERCENTAGE_CONSTANT;
                unit._currentRes.AddVal(fReduceVal);
            }
            if (cEffect.IsEffectType(EffectType.EFFECT_STAT_REDUCE_RES)) {
                unit._currentRes.MinusVal(cEffect.GetEffectVal());
            }

            if (cEffect.IsEffectType(EffectType.EFFECT_DAMAGE_IGNORE_SHIELD)) {
                unit.ReceiveDamage(cEffect.GetEffectVal());
            }

            if (cEffect.IsEffectType(EffectType.EFFECT_HEAL)) {
                unit._currentHealth.AddVal(cEffect.GetEffectVal());
            }

            if (cEffect.IsEffectType(EffectType.EFFECT_STAT_INCREASE_CRIT_RATE)) {
                unit._currentCritRate.AddVal(cEffect.GetEffectVal());
            }
            if (cEffect.IsEffectType(EffectType.EFFECT_STAT_REDUCE_CRIT_RATE_PERCENT)) {
                float fReduceVal = unit._currentCritRate.GetVal() * cEffect.GetEffectVal() * Global.PERCENTAGE_CONSTANT;
                unit._currentCritRate.AddVal(fReduceVal);
            }
            if (cEffect.IsEffectType(EffectType.EFFECT_STAT_REDUCE_CRIT_RATE)) {
                unit._currentCritRate.MinusVal(cEffect.GetEffectVal());
            }

            if (cEffect.IsEffectType(EffectType.EFFECT_REVIVE)) {
                unit.Revive();
                unit._currentHealth.SetVal(cEffect.GetEffectVal());
            }
            if (cEffect.IsEffectType(EffectType.EFFECT_REVIVE_FULL)) {
                unit.Revive();
                unit._currentHealth.SetVal(unit._currentHealth.GetMax());
            }
            if (cEffect.IsEffectType(EffectType.EFFECT_DIE_REVIVE)) {
                unit.death_count++;
                unit._currentHealth.SetVal(unit._currentHealth.GetMax());
            }

            if (cEffect.IsEffectType(EffectType.EFFECT_INSTANT_KILL)) {
                unit.ReceiveDamage(unit._currentHealth.GetVal());
            }

            if (cEffect.IsEffectType(EffectType.EFFECT_TRIGGER_ALL_SINGLE)) {
                EffectList listEffect = unit.GetRollEffectList(MatchType.SINGLE);
                foreach (EffectScriptableObject singleEffect in listEffect) {
                    _ActivateEffect(cDeck, singleEffect);
                }
            }

            if (cEffect.IsEffectType(EffectType.EFFECT_DAMAGE_FLAT_DAMAGE)) {
                unit.ReceiveDamage(cEffect.GetEffectVal());
            }

            if (cEffect.IsEffectType(EffectType.EFFECT_CLEANSE)) {
                unit.RemoveTempEffectByPredicate(delegate (EffectObject obj) {
                    if (obj.GetEffectAffinityType() == EffectAffinityType.NEGATIVE) {
                        return true;
                    }

                    return false;
                });
            }

            Global.DEBUG_PRINT("[Effect] Triggered " + cEffect.GetTypeName() +
                                " val=" + cEffect.GetEffectVal() +
                                " unit_index=" + unit.index);
        }
    }

    void _ActivateTempEffect(Deck cDeck, EffectObject cEffect) {
        List<UnitObject> arrTargetUnit = _GetTargetUnitBasedOnTargetType(cDeck, cEffect.effectSO);

        float val = cEffect.GetEffectVal();
        EffectType eType = cEffect.effectSO.GetEffectType();
        foreach (UnitObject unit in arrTargetUnit) {
            if (eType == EffectType.EFFECT_HEAL_TURN) {
                unit._currentHealth.AddVal(val);
            }

            if (eType == EffectType.EFFECT_STAT_INCREASE_RES_TURN) {
                unit._currentRes.AddVal(val);
            }

            if (eType == EffectType.EFFECT_BLEED) {
                unit._currentHealth.MinusVal(val);
            }
        }
    }

    void _ActivatePostEffect(UnitObject unitObj, float fAttack) {
        EffectMap mapTempEffect = unitObj.GetAllTempEffect();
        foreach (EffectObject effectObj in mapTempEffect) {
            EffectTargetType eTargetType = effectObj.GetEffectTargetType();
            Deck cDeck = DeckManager.instance.GetDeckByType(_eAttackerDeck);
            List<UnitObject> arrTargetUnit = _GetTargetUnitBasedOnTargetType(cDeck, effectObj.effectSO);

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

                if (unitObj.HasEffectParam(EffectType.EFFECT_DAMAGE_MULTIPLY_POST)) {
                    if (unitObj.GetEffectParam(EffectType.EFFECT_DAMAGE_MULTIPLY_POST, out float val)) {
                        target._currentHealth.MinusVal(fAttack * val);
                    }
                }
            }
        }

        unitObj.Resolve(EffectResolveType.RESOLVE_POST_ATTACK);
    }

    List<UnitObject> _GetTargetUnitBasedOnTargetType(Deck cDeck, EffectScriptableObject effectSO) {
        return _GetTargetUnitBasedOnTargetType(cDeck, effectSO.GetTargetType(), effectSO.GetTargetCondition(), effectSO.GetTargetParam());
    }

    List<UnitObject> _GetTargetUnitBasedOnTargetType(Deck cDeck, EffectTargetType eTargetType, EffectTargetCondition eTargetCondition = EffectTargetCondition.NONE, float eTargetParam = 0.0f) {
        List<UnitObject> arrTarget = new List<UnitObject>();
        if (eTargetType == EffectTargetType.SELF) {
            arrTarget.Add(cDeck.GetUnitObject(_nAttackerIndex));
            return arrTarget;
        }

        eDeckType eOtherType = cDeck.GetDeckType() == eDeckType.PLAYER ? eDeckType.ENEMY : eDeckType.PLAYER;
        Deck cOtherDeck = DeckManager.instance.GetDeckByType(eOtherType);

        Deck cTargetDeck = eTargetType == EffectTargetType.TEAM ? cDeck : cOtherDeck;

        if (cDeck == null || cOtherDeck == null) {
            return arrTarget;
        }

        switch (eTargetCondition) {
            case EffectTargetCondition.LOWEST_HP: {
                    int nLowestHealthIndex = GetLowestHealth(cTargetDeck);
                    arrTarget.Add(cTargetDeck.GetUnitObject(nLowestHealthIndex));
                    break;
                }
            case EffectTargetCondition.HOLY:
                arrTarget = DeckHelperFunc.GetUnitByArchetype(cTargetDeck, eUnitArchetype.HOLY);
                break;
            case EffectTargetCondition.UNDEAD:
                arrTarget = DeckHelperFunc.GetUnitByArchetype(cTargetDeck, eUnitArchetype.UNDEAD);
                break;
            case EffectTargetCondition.ELF:
                arrTarget = DeckHelperFunc.GetUnitByArchetype(cTargetDeck, eUnitArchetype.ELF);
                break;
            case EffectTargetCondition.DECK:
                arrTarget = DeckHelperFunc.GetAllAliveUnit(cTargetDeck);
                break;
            case EffectTargetCondition.DEAD:
                arrTarget = DeckHelperFunc.GetAllDeadUnit(cTargetDeck);
                break;
            case EffectTargetCondition.RANDOM:
                arrTarget = DeckHelperFunc.GetRandomAliveUnit(cTargetDeck, (int)eTargetParam);
                break;
            case EffectTargetCondition.RANDOM_UNDEAD:
                arrTarget = DeckHelperFunc.GetRandomAliveUnit(cTargetDeck, (int)eTargetParam, eUnitArchetype.UNDEAD);
                break;
            case EffectTargetCondition.RANDOM_BELOW_HP_PERCENT_1:
                arrTarget = DeckHelperFunc.GetAllUnitBelowHpPercent(cTargetDeck, eTargetParam);
                arrTarget = DeckHelperFunc.PickRandomFromList(arrTarget, 1);
                break;
            case EffectTargetCondition.FRONTLINE:
                arrTarget = DeckHelperFunc.GetAllUnitFrontLine(cTargetDeck);
                break;
            case EffectTargetCondition.ALL_UNIT_BELOW_HP_PERCENT:
                arrTarget = DeckHelperFunc.GetAllUnitBelowHpPercent(cTargetDeck, eTargetParam);
                break;

        }

        return arrTarget;
    }
    
    EffectTargetCondition _GetTargetConditionFromTempEffect(UnitObject cUnit, EffectType eType) {
        EffectTargetCondition eTargetCondition = EffectTargetCondition.LOWEST_HP;
        if (cUnit.GetEffectSO(eType, out EffectScriptableObject effectSO)) {
            eTargetCondition = effectSO.GetTargetCondition();
        }
        return eTargetCondition;
    }
}
