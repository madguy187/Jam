using UnityEngine;

public class CombatManager : MonoBehaviour {
    public static CombatManager instance;

    const int INVALID_INDEX = -1;

    int _nTargetIndex = INVALID_INDEX;

    void Awake() {
        if (instance != null) {
            Destroy(instance);
        }
        instance = this;
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Alpha3)) {
            ExecBattle(eDeckType.PLAYER);
        }
    }

    public void ExecBattle(eDeckType eType) {
        eDeckType eOtherType = eType == eDeckType.PLAYER ? eDeckType.ENEMY : eDeckType.PLAYER;
        Deck cDeck = DeckManager.instance.GetDeckByType(eType);
        Deck cOtherDeck = DeckManager.instance.GetDeckByType(eOtherType);

        int nTarget = INVALID_INDEX;
        if (_nTargetIndex <= INVALID_INDEX) {
            nTarget = GetLowestHealth(cOtherDeck);
        } else {
            nTarget = _nTargetIndex;
        }

        if (nTarget <= INVALID_INDEX) {
            Global.DEBUG_PRINT("[ExecBattle] Cannot find target");
            return;
        }

        UnitObject cDefenderUnit = cOtherDeck.GetUnitObject(nTarget);
        if (cDefenderUnit == null) {
            Global.DEBUG_PRINT("[ExecBattle] Deck is empty");
            return;
        }

        foreach (UnitObject cAttackerUnit in cDeck) {
            if (cDefenderUnit.IsDead()) {
                int nNewTarget = GetLowestHealth(cOtherDeck);
                if (nNewTarget < 0) {
                    return;
                }
                cDefenderUnit = cOtherDeck.GetUnitObject(nNewTarget);
            }

            _ExecBattleOne(cAttackerUnit, cDefenderUnit);
        }
    }

    void _ExecBattleOne(UnitObject cAttackerUnit, UnitObject cDefenderUnit) {
        if (cAttackerUnit == null || cDefenderUnit == null) {
            return;
        }

        float fAttack = cAttackerUnit.GetAttack();
        if (_IsCrit(cAttackerUnit.GetCritRate())) {
            fAttack *= _GetCritRatio(cAttackerUnit);
        }

        fAttack *= _GetResRatio(cDefenderUnit);
        cDefenderUnit.ReceiveDamage(fAttack);
    }

    public int GetLowestHealth(Deck cDeck) {
        int nIndex = INVALID_INDEX;

        float nLowestHealth = Mathf.Infinity;

        int nCount = 0;
        foreach (UnitObject unit in cDeck) {
            float nHealth = unit.GetHealth();
            if (nHealth > nLowestHealth) {
                continue;
            }

            nIndex = nCount;
            nLowestHealth = nHealth;
            nCount++;
        }

        return nIndex;
    }

    bool _IsCrit(int nCritRate) {
        int nRand = Random.Range(0, 101);
        if (nRand < nCritRate) {
            return true;
        }

        return false;
    }

    float _GetCritRatio(UnitObject cAttackerUnit) {
        return cAttackerUnit.GetCritMulti() / 100;
    }
    
    float _GetResRatio(UnitObject cDefenderUnit) {
        return 100 / (100 + cDefenderUnit.GetRes() * 10);
    }
}
