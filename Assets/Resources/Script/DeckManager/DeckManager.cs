using System.Collections.Generic;
using UnityEngine;

public enum eDeckType {
    NONE,
    PLAYER,
    ENEMY,
}

public class DeckManager : MonoBehaviour {
    public static DeckManager instance;

    [SerializeField] Transform _transPlayerPos;
    [SerializeField] Transform _transEnemyPos;

    Deck cPlayerDeck = new Deck();
    Deck cEnemyDeck = new Deck();

    void Awake() {
        if (instance != null) {
            Destroy(instance);
        }
        instance = this;
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            cPlayerDeck.AddUnit("Paladin");
        }

        if (Input.GetKeyDown(KeyCode.Alpha2)) {
            cEnemyDeck.AddUnit("Paladin");
        }

        if (Input.GetKeyDown(KeyCode.Alpha4)) {
            cEnemyDeck.AddUnit("Cleric");
        }
    }

    void Start() {
        {
            List<UnitPosition> vecPos = new List<UnitPosition>();
            UnitPosition[] vecPlayerUnitPos = _transPlayerPos.GetComponentsInChildren<UnitPosition>();
            foreach (UnitPosition playerPos in vecPlayerUnitPos) {
                vecPos.Add(playerPos);
            }
            cPlayerDeck.Init(vecPos);
            Global.DEBUG_PRINT("[Deck] Loaded PlayerPos: " + vecPos.Count);
        }

        {
            List<UnitPosition> vecPos = new List<UnitPosition>();
            UnitPosition[] vecEnemyUnitPos = _transEnemyPos.GetComponentsInChildren<UnitPosition>();
            foreach (UnitPosition enemyPos in vecEnemyUnitPos) {
                vecPos.Add(enemyPos);
            }
            cEnemyDeck.Init(vecPos);
            Global.DEBUG_PRINT("[Deck] Loaded EnemyPos: " + vecPos.Count);
        }
    }

    public UnitObject AddUnit(eDeckType eType, string strUnitName) {
        Deck cDeck = GetDeckByType(eType);
        UnitObject unit = cDeck.AddUnit(strUnitName);

        return unit;
    }

    public Deck GetDeckByType(eDeckType eType) {
        Deck cDeck = null;

        switch (eType) {
            case eDeckType.PLAYER:
                cDeck = cPlayerDeck;
                break;
            case eDeckType.ENEMY:
                cDeck = cEnemyDeck;
                break;
            default:
                break;
        }

        return cDeck;
    }

    public void ResolveTempEffect() {
        cPlayerDeck.Resolve();
        cEnemyDeck.Resolve();
    }
}
