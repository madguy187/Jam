using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Deck : IEnumerable<UnitObject> {
    List<UnitObject> _vecUnit = new List<UnitObject>();
    List<Transform> _vecPosition = new List<Transform>();

    public void Init(List<Transform> vecPos) {
        _vecPosition = vecPos;
    }

    public UnitObject AddUnit(string unitName) {
        GameObject objUnitPrefab = ResourceManager.instance.GetUnit(unitName);
        if (objUnitPrefab == null) {
            return null;
        }

        UnitObject unit = ResourceManager.instance.CreateUnit(objUnitPrefab);
        if (unit == null) {
            return null;
        }

        if (_vecUnit.Count == _vecPosition.Count) {
            return null;
        }

        unit.index = _vecUnit.Count;
        unit.onDeath = () => {
            _vecUnit.RemoveAt(unit.index);
        };
        _vecUnit.Add(unit);

        Transform pos = _GetPosition();
        unit.transform.position = pos.position;

        return unit;
    }

    public UnitObject GetUnitObject(int index) {
        if (index < 0 || index >= _vecUnit.Count) {
            return null;
        }

        return _vecUnit[index];
    }

    Transform _GetPosition() {
        int nIndex = _vecUnit.Count;
        int nCount = _vecPosition.Count;
        if (nIndex > nCount) {
            return null;
        }

        return _vecPosition[nIndex - 1];
    }

    // For IEnumerable<UnitObject>
    public IEnumerator<UnitObject> GetEnumerator() { return _vecUnit.GetEnumerator(); }
    
    // For IEnumerable
    IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
}
