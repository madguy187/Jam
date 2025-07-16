using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager instance;

    Dictionary<string, GameObject> _mapUnitSO = new Dictionary<string, GameObject>();

    void Awake() {
        if (instance != null) {
            Destroy(instance);
        }

        instance = this;
        
        foreach (Object obj in Resources.LoadAll("UnitSO/Unit", typeof(GameObject)).ToList())
        {
            GameObject unitSO = (GameObject)obj;
            _mapUnitSO.Add(obj.name, unitSO);
        }
        Global.DEBUG_PRINT("[Resources] Loaded PlayerUnits: " + _mapUnitSO.Count());
    }
    
    public UnitObject CreateUnit(GameObject objPrefab) {
        GameObject obj = Instantiate(objPrefab);
        if (obj == null) {
            return null;
        }

        UnitObject unit = obj.GetComponent<UnitObject>();
        if (unit == null) {
            Destroy(obj);
            return null;
        }

        unit.Init();

        return unit;
    }

    public GameObject GetUnit(string unitName) {
        if (_mapUnitSO.ContainsKey(unitName)) {
            return _mapUnitSO[unitName];
        }

        return null;
    }
    
}
