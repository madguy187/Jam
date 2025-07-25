using System.Collections.Generic;
using Mono.Cecil;
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
            GameObject prefab = ResourceManager.instance.Debug_RandUnit();
            if (prefab == null) {
                Global.DEBUG_PRINT("Error loading");
                return;
            }

            prefab = ResourceManager.instance.GetUnit("ForestWarden");
            
            cPlayerDeck.AddUnit(prefab);
            Global.DEBUG_PRINT("Loaded " + prefab.name + " into Player Team");
        }

        if (Input.GetKeyDown(KeyCode.Alpha2)) {
            GameObject prefab = ResourceManager.instance.Debug_RandUnit();
            if (prefab == null) {
                Global.DEBUG_PRINT("Error loading");
                return;
            }
            cEnemyDeck.AddUnit(prefab);
            Global.DEBUG_PRINT("Loaded " + prefab.name + " into Enemy Team");
        }

        if (Input.GetKeyDown(KeyCode.Alpha3)) {
            List<UnitObject> listPlayer = DeckHelperFunc.GetAllAliveUnit(cPlayerDeck);
            int rand = Random.Range(0, listPlayer.Count);
            RelicScriptableObject relic = ResourceManager.instance.Debug_RandRelic();
            listPlayer[rand].AddRelic(relic);
            Global.DEBUG_PRINT("Loaded " + relic.name + " into " + listPlayer[rand].name + " index:" + rand + " team: Player");
        }

        if (Input.GetKeyDown(KeyCode.Alpha4)) {
            List<UnitObject> listEnemy = DeckHelperFunc.GetAllAliveUnit(cEnemyDeck);
            int rand = Random.Range(0, listEnemy.Count);
            RelicScriptableObject relic = ResourceManager.instance.Debug_RandRelic();
            listEnemy[rand].AddRelic(relic);
            Global.DEBUG_PRINT("Loaded " + relic.name + " into " + listEnemy[rand].name + " index:" + rand + " team: Enemy");
        }
    }

    void Start() {
        {
            List<UnitPosition> vecPos = new List<UnitPosition>();
            UnitPosition[] vecPlayerUnitPos = _transPlayerPos.GetComponentsInChildren<UnitPosition>();
            foreach (UnitPosition playerPos in vecPlayerUnitPos) {
                vecPos.Add(playerPos);
            }
            cPlayerDeck.Init(eDeckType.PLAYER, vecPos);
            Global.DEBUG_PRINT("[Deck] Loaded PlayerPos: " + vecPos.Count);
        }

        {
            List<UnitPosition> vecPos = new List<UnitPosition>();
            UnitPosition[] vecEnemyUnitPos = _transEnemyPos.GetComponentsInChildren<UnitPosition>();
            foreach (UnitPosition enemyPos in vecEnemyUnitPos) {
                vecPos.Add(enemyPos);
            }
            cEnemyDeck.Init(eDeckType.ENEMY, vecPos);
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

    public void ResolveTurnTempEffect() {
        cPlayerDeck.ResolveTurn();
        cEnemyDeck.ResolveTurn();
    }

    public void InitDeckEffect() {
        cPlayerDeck.InitDeck();
        cEnemyDeck.InitDeck();
    }
}