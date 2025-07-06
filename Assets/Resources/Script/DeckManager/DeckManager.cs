using System.Collections.Generic;
using UnityEngine;

public enum UnitRelation {
    PLAYER,
    ENEMY,
}

public class DeckManager : MonoBehaviour {
    public static DeckManager instance;

    List<UnitObject> _vecPlayerUnit = new List<UnitObject>();
    List<UnitObject> _vecEnemyUnit = new List<UnitObject>();

    List<Transform> _vecPlayerPosition = new List<Transform>();
    List<Transform> _vecEnemyPosition = new List<Transform>();

    [Header("Unit Prefab")]
    [SerializeField] private GameObject _objUnitPrefab;

    void Awake() {
        if (instance != null) {
            Destroy(instance);
        }
        instance = this;
    }

    void Start() {
        Transform transPlayerPos = transform.Find("DeckPosition/Player");
        foreach (Transform playerPos in transPlayerPos) {
            _vecPlayerPosition.Add(playerPos);
        }
        Global.DEBUG_PRINT("[Deck] Loaded PlayerPos: " + _vecPlayerPosition.Count);

        Transform transEnemyPos = transform.Find("DeckPosition/Enemy");
        foreach (Transform playerPos in transEnemyPos) {
            _vecEnemyPosition.Add(playerPos);
        }
        Global.DEBUG_PRINT("[Deck] Loaded EnemyPos: " + _vecEnemyPosition.Count);
    }

    public UnitObject AddUnit(UnitRelation relation, string unitName) {
        UnitScriptableObject unitSO = ResourceManager.instance.GetUnit(unitName);
        if (unitSO == null) {
            return null;
        }

        UnitObject unit = _CreateUnit(unitSO);
        if (unit == null) {
            return null;
        }

        switch (relation) {
            case UnitRelation.PLAYER:
                _vecPlayerUnit.Add(unit);
                break;
            case UnitRelation.ENEMY:
                _vecEnemyUnit.Add(unit);
                break;
            default:
                Global.DEBUG_PRINT("[AddUnit] Undefined UnitRelation");
                break;
        }

        Transform pos = _GetPosition(relation);
        unit.transform.position = pos.position;

        return unit;
    }

    Transform _GetPosition(UnitRelation relation) {
        switch (relation) {
            case UnitRelation.PLAYER: {
                    int nIndex = _vecPlayerUnit.Count;
                    int nCount = _vecPlayerPosition.Count;
                    if (nIndex > nCount) {
                        return null;
                    }

                    return _vecPlayerPosition[nIndex - 1];
                }
            case UnitRelation.ENEMY: {
                    int nIndex = _vecEnemyUnit.Count;
                    int nCount = _vecEnemyPosition.Count;
                    if (nIndex > nCount) {
                        return null;
                    }

                    return _vecEnemyPosition[nIndex - 1];
                }
            default:
                Global.DEBUG_PRINT("[_GetPosition] Undefined UnitRelation");
                break;
        }

        return null;
    }

    UnitObject _CreateUnit(UnitScriptableObject unitSO) {
        GameObject obj = Instantiate(_objUnitPrefab);
        if (obj == null) {
            return null;
        }

        UnitObject unit = obj.GetComponent<UnitObject>();
        if (unit == null) {
            Destroy(obj);
            return null;
        }

        unit.SetUnitSO(unitSO);

        return unit;
    }
}
