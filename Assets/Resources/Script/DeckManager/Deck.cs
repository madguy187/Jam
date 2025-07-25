using System;
using System.Collections;
using System.Collections.Generic;
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

    public UnitObject AddUnit(GameObject prefab) {
        int nIndex = _GetEmptySlotIndex();
        if (nIndex < 0) {
            return null;
        }

        UnitObject unit = ResourceManager.instance.CreateUnit(prefab);
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
        
        return AddUnit(objUnitPrefab);
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
