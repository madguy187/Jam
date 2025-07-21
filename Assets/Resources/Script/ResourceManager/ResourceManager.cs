using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ResourceManager : MonoBehaviour {
    public static ResourceManager instance;

    Dictionary<string, GameObject> _mapUnitSO = new Dictionary<string, GameObject>();
    Dictionary<string, RelicScriptableObject> _mapRelicSO = new Dictionary<string, RelicScriptableObject>();

    void Awake() {
        if (instance != null) {
            Destroy(instance);
        }

        instance = this;

        foreach (Object obj in Resources.LoadAll("ScriptableObject/UnitSO/Unit", typeof(GameObject)).ToList()) {
            GameObject prefabUnit = (GameObject)obj;
            _mapUnitSO.Add(obj.name, prefabUnit);
        }
        Global.DEBUG_PRINT("[Resources] Loaded Units: " + _mapUnitSO.Count());

        foreach (RelicScriptableObject obj in Resources.LoadAll("ScriptableObject/RelicSO/Relic", typeof(RelicScriptableObject)).ToList()) {
            RelicScriptableObject relicSO = obj;
            _mapRelicSO.Add(obj.name, relicSO);
        }
        Global.DEBUG_PRINT("[Resources] Loaded Relics: " + _mapRelicSO.Count());
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

        // Add UnitClickHandler component for hovering to work
        if (unit.GetComponent<UnitClickHandler>() == null) {
            unit.gameObject.AddComponent<UnitClickHandler>();
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
    
    public RelicScriptableObject GetRelic(string unitName) {
        if (_mapRelicSO.ContainsKey(unitName)) {
            return _mapRelicSO[unitName];
        }

        return null;
    }
}
