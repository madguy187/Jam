using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager instance;

    Dictionary<string, UnitScriptableObject> _mapUnitSO = new Dictionary<string, UnitScriptableObject>();

    void Awake() {
        if (instance != null) {
            Destroy(instance);
        }

        instance = this;
        
        foreach (Object obj in Resources.LoadAll("UnitSO/Unit", typeof(UnitScriptableObject)).ToList())
        {
            UnitScriptableObject unitSO = (UnitScriptableObject)obj;
            _mapUnitSO.Add(obj.name, unitSO);
        }
        Global.DEBUG_PRINT("[Resources] Loaded PlayerUnits: " + _mapUnitSO.Count());
    }

    public UnitScriptableObject GetUnit(string unitName)
    {
        if (_mapUnitSO.ContainsKey(unitName)) {
            return _mapUnitSO[unitName];
        }

        return null;
    }
}
