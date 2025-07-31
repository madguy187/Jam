using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Deck : IEnumerable<UnitObject> {
    eDeckType _eType = eDeckType.NONE;
    public eDeckType GetDeckType() { return _eType; }

    List<UnitObject> _vecUnit = new List<UnitObject>();
    List<UnitPosition> _vecPosition = new List<UnitPosition>();
    public int GetDeckMaxSize() { return _vecPosition.Count; }

    public void Init(eDeckType eType, List<UnitPosition> vecPos) {
        _eType = eType;
        _vecPosition = vecPos;
        Global.DEBUG_PRINT("[Deck] Init Deck with Size: " + _vecPosition.Count);

        for (int i = 0; i < _vecPosition.Count; i++) {
            _vecUnit.Add(null);
        }
    }

    public void ReInitUnitPosition(List<UnitPosition> vecPos) {
        for (int i = 0; i < _vecPosition.Count; i++) {
            _vecPosition[i].transform.position = vecPos[i].transform.position;
            UnitObject unit = _vecUnit[i];
            if (unit == null) {
                continue;
            }
            Transform pos = _GetPosition(i);
            unit.transform.position = pos.position;
        }
    }

    public void RemoveUnit(UnitObject unit) {
        if (!IsValidUnitIndex(unit.index) || _vecUnit.Contains(unit) == false) {
            return;
        }

        _vecUnit[unit.index].transform.position = new Vector3(0.0f, -170.0f, 0.0f);

        _vecUnit[unit.index] = null;
    }

    public void DestroyAllUnit() {
        for (int i = 0; i < _vecUnit.Count; i++) {
            if (_vecUnit[i] == null) {
                continue;
            }
            GameObject.Destroy(_vecUnit[i].gameObject);
            _vecUnit[i] = null;
        }
    }

    public UnitObject AddUnit(UnitObject unit, int nIndex = -1) {
        if (nIndex < 0) {
            nIndex = _GetEmptySlotIndex();
        }

        if (nIndex < 0) {
            return null;
        }

        unit.index = nIndex;

        Transform pos = _GetPosition(nIndex);
        unit.transform.position = pos.position;

        eUnitPosition eUnitPos = _GetUnitPosition(nIndex);
        unit.SetUnitPosition(eUnitPos);

        _vecUnit[nIndex] = unit;
        return unit;
    }

    public UnitObject AddUnit(GameObject prefab) {
        int nIndex = _GetEmptySlotIndex();
        if (nIndex < 0) {
            return null;
        }

        UnitObject unit = ResourceManager.instance.CreateUnit(prefab, _eType == eDeckType.ENEMY);
        if (unit == null) {
            return null;
        }

        unit.index = nIndex;

        Transform pos = _GetPosition(nIndex);
        unit.transform.position = pos.position;

        eUnitPosition eUnitPos = _GetUnitPosition(nIndex);
        unit.SetUnitPosition(eUnitPos);

        _vecUnit[nIndex] = unit;
        return unit;
    }

    public UnitObject AddUnit(string unitName) {
        if (IsFullDeck()) {
            return null;
        }

        GameObject objUnitPrefab = ResourceManager.instance.GetUnit(unitName);
        if (objUnitPrefab == null) {
            return null;
        }

        // Create and add unit using prefab
        UnitObject unit = AddUnit(objUnitPrefab);

        // Parent under DeckManager so the player unit persists across scene loads
        // Not sure about enemy though, i set for player first.
        if (_eType == eDeckType.PLAYER && unit != null && DeckManager.instance != null) {
            unit.transform.SetParent(DeckManager.instance.transform, true);
        }

        return unit;
    }

    public UnitObject GetUnitObject(int index) {
        if (index < 0 || index >= _vecUnit.Count) {
            return null;
        }

        return _vecUnit[index];
    }

    public List<UnitObject> GetUnitByPredicate(Predicate<UnitObject> predicate) {
        return _vecUnit.FindAll(predicate);
    }

    public bool IsValidUnitIndex(int index) {
        if (index < 0 || index >= _vecUnit.Count) {
            return false;
        }

        if (_vecUnit[index] == null) {
            return false;
        }

        if (_vecUnit[index].IsDead()) {
            return false;
        }

        return true;
    }

    public bool HasFrontUnit() {
        foreach (UnitObject unit in _vecUnit) {
            if (unit == null) {
                continue;
            }

            if (unit.IsDead()) {
                continue;
            }

            if (unit.IsFrontPosition()) {
                return true;
            }
        }

        return false;
    }

    public bool IsFullDeck() {
        for (int i = 0; i < GetDeckMaxSize(); i++) {
            if (_vecUnit[i] == null) {
                return false;
            }
        }

        return true;
    }

    public void ResolveTurn() {
        foreach (UnitObject unit in _vecUnit) {
            if (unit == null) {
                continue;
            }

            if (unit.IsDead()) {
                continue;
            }

            unit.Resolve(EffectResolveType.RESOLVE_TURN);
        }
    }

    public void InitDeck() {
        foreach (UnitObject unit in _vecUnit) {
            if (unit == null) {
                continue;
            }

            if (unit.IsDead()) {
                continue;
            }
            
            unit.LoadRelic();
        }
    }

    int _GetEmptySlotIndex() {
        for (int i = 0; i < GetDeckMaxSize(); i++) {
            if (_vecUnit[i] == null) {
                return i;
            }
        }

        return Global.INVALID_INDEX;
    }

    Transform _GetPosition(int nIndex) {
        if (_vecPosition == null) {
            _vecPosition = DeckManager.instance.GetAllPositionByType(_eType).ToList();
        }
        if (nIndex < 0 || nIndex > _vecPosition.Count) {
            return null;
        }

        return _vecPosition[nIndex].transform;
    }

    eUnitPosition _GetUnitPosition(int nIndex) {
        if (nIndex < 0 || nIndex > _vecPosition.Count) {
            return eUnitPosition.NONE;
        }

        if (_vecPosition[nIndex] == null) {
            return eUnitPosition.NONE;
        }
        
        return _vecPosition[nIndex].GetUnitPosition();
    }

    // For IEnumerable<UnitObject>
    public IEnumerator<UnitObject> GetEnumerator() { return _vecUnit.GetEnumerator(); }
    
    // For IEnumerable
    IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
}
