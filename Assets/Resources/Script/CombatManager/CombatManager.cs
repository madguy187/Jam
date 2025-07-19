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

    void Update() {
        if (Input.GetKeyDown(KeyCode.Alpha3)) {
            StartBattleLoop();
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

    public void StartBattleLoop() {
        if (!_CanBattle()) {
            return;
        }
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
        if (cDefenderUnit == null) {
            Global.DEBUG_PRINT("[ExecBattle] Defender deck is empty target=" + nTarget);
            return;
        }

        UnitObject cAttackerUnit = cDeck.GetUnitObject(nAttackerIndex);
        if (cAttackerUnit == null) {
            Global.DEBUG_PRINT("[ExecBattle] Attacker deck is empty target=" + nAttackerIndex);
            return;
        }

        Global.DEBUG_PRINT("attacker_deck=" + eType + " attacker_index=" + nAttackerIndex + " defender_index=" + nTarget);
        _ExecBattleOne(cAttackerUnit, cDefenderUnit, eRollType.SINGLE);
    }

    void _ExecBattleOne(UnitObject cAttackerUnit, UnitObject cDefenderUnit, eRollType eRoll) {
        if (cAttackerUnit == null || cDefenderUnit == null) {
            return;
        }

        EffectList effects = cAttackerUnit.GetEffectList(eRoll);
        foreach (EffectScriptableObject effect in effects) {
            _ActivateEffect(effect, ref cAttackerUnit);
        }

        float fAttack = cAttackerUnit.GetAttack();
        if (_IsCrit(cAttackerUnit.GetCritRate())) {
            fAttack *= _GetCritRatio(cAttackerUnit);
        }

        fAttack *= _GetResRatio(cDefenderUnit);
        cDefenderUnit.ReceiveDamage(fAttack);
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

            if (bHasFrontUnit && !unit.IsFrontPosition()) {
                continue;
            }

            nIndex = unit.index;
            nLowestHealth = nHealth;
        }

        return nIndex;
    }

    bool _IsCrit(int nCritRate) {
        int nRand = Random.Range(0, Global.PERCENTAGE_CONSTANT + 1); // +1 coz exclusive
        if (nRand < nCritRate) {
            return true;
        }

        return false;
    }

    float _GetCritRatio(UnitObject cAttackerUnit) {
        return cAttackerUnit.GetCritMulti() / Global.PERCENTAGE_CONSTANT;
    }

    float _GetResRatio(UnitObject cDefenderUnit) {
        return Global.PERCENTAGE_CONSTANT / (Global.PERCENTAGE_CONSTANT + cDefenderUnit.GetRes() * Global.RESISTANCE_PERCENTAGE_CONSTANT);
    }

    void _ActivateEffect(EffectScriptableObject cEffect, ref UnitObject cAttackerUnit) {
        if (cEffect.IsEffectType(EffectType.EFFECT_STAT_INCREASE_SHIELD)) {
            cAttackerUnit.AddShield(cEffect.GetEffectVal());
        }

        Global.DEBUG_PRINT("[Effect] Triggered " + cEffect.GetTypeName() + " val=" + cEffect.GetEffectVal());
    }
}
