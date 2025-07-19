using System.Collections.Generic;
using UnityEngine;

public enum eDeckType {
    NONE,
    PLAYER,
    ENEMY,
}

public class DeckManager : MonoBehaviour {
    public static DeckManager instance;

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
    }

    void Start() {
        {
            List<Transform> vecPos = new List<Transform>();
            Transform transPlayerPos = transform.Find("DeckPosition/Player");
            foreach (Transform playerPos in transPlayerPos) {
                vecPos.Add(playerPos);
            }
            cPlayerDeck.Init(vecPos);
            Global.DEBUG_PRINT("[Deck] Loaded PlayerPos: " + vecPos.Count);
        }

        {
            List<Transform> vecPos = new List<Transform>();
            Transform transEnemyPos = transform.Find("DeckPosition/Enemy");
            foreach (Transform enemyPos in transEnemyPos) {
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
}
