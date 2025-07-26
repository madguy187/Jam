using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ResourceManager : MonoBehaviour {
    public static ResourceManager instance;

    Dictionary<string, GameObject> _mapUnitSO = new Dictionary<string, GameObject>();
    Dictionary<string, RelicScriptableObject> _mapRelicSO = new Dictionary<string, RelicScriptableObject>();
    Dictionary<EffectType, EffectDetailScriptableObject> _mapEffectDetailSO = new Dictionary<EffectType, EffectDetailScriptableObject>();

    [SerializeField] Sprite spriteDefault = null;
    public Sprite GetDefaultEffectSprite() { return spriteDefault; }

    [SerializeField] GameObject prefabDynamicText = null;

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

        foreach (EffectDetailScriptableObject obj in Resources.LoadAll("ScriptableObject/EffectDetail", typeof(EffectDetailScriptableObject)).ToList()) {
            EffectDetailScriptableObject effectDetailSO = obj;
            _mapEffectDetailSO.Add(effectDetailSO.eEffectType, effectDetailSO);
        }
        Global.DEBUG_PRINT("[Resources] Loaded Effect Detail: " + _mapRelicSO.Count());
    }

    public string Debug_RandUnit() {
        int rand = Random.Range(0, _mapUnitSO.Count);
        return _mapUnitSO.ElementAt(rand).Key;
    }

    public RelicScriptableObject Debug_RandRelic() {
        int rand = Random.Range(0, _mapRelicSO.Count);
        return _mapRelicSO.ElementAt(rand).Value;
    }

    public UnitObject CreateUnit(GameObject objPrefab) {
        GameObject obj = Instantiate(objPrefab);
        if (obj == null) {
            return null;
        }

        UnitObject unit = obj.GetComponent<UnitObject>();
        if (unit == null) {
            Destroy(obj.gameObject);
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

    public EffectDetailScriptableObject GetEffectDetail(EffectType eType) {
        if (_mapEffectDetailSO.ContainsKey(eType)) {
            return _mapEffectDetailSO[eType];
        }

        return null;
    }

    public void GenerateDynamicText(Vector3 pos, string text, float lifetime = 2.0f) {
        if (prefabDynamicText == null) {
            return;
        }

        GameObject objText = Instantiate(prefabDynamicText);
        if (objText == null) {
            return;
        }

        UIDynamicNumber textComp = objText.GetComponent<UIDynamicNumber>();
        if (textComp == null) {
            return;
        }

        textComp.Init(pos, lifetime, text);
    }
}
