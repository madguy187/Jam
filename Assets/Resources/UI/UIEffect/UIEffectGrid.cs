using System.Collections.Generic;
using UnityEngine;

public class UIEffectGrid : MonoBehaviour {
    [SerializeField] Transform transParentCanvas;
    [SerializeField] UnitObject unit;

    Dictionary<EffectType, UIEffect> _mapEffectObj = new Dictionary<EffectType, UIEffect>();

    public UnitObject GetUnit() { return unit; }

#if UNITY_EDITOR
    public void SetCanvasParent(Transform trans) {
        transParentCanvas = trans;
    }

    public void SetUnit(UnitObject _unit) {
        unit = _unit;
    }
#endif

    public void AddEffectUI(EffectType eType) {
        if (!_mapEffectObj.ContainsKey(eType)) {
            UIEffect compEffect = CreateUIEffectObject();
            compEffect.SetGridParent(this);
            compEffect.SetText(eType);
            compEffect.transform.SetParent(transParentCanvas);
            _mapEffectObj.Add(eType, compEffect);
        } else {
            _mapEffectObj[eType].UpdateValue();
        }
    }

    public void RemoveEffectUI(EffectType eType) {
        if (_mapEffectObj.ContainsKey(eType)) {
            Destroy(_mapEffectObj[eType].gameObject);
            _mapEffectObj.Remove(eType);
        }
    }

    public bool HasEffectUI(EffectType eType) {
        if (_mapEffectObj.ContainsKey(eType)) {
            return true;
        }

        return false;
    }

    public static UIEffect CreateUIEffectObject() {
        string path = "UI/UIEffect/UIEffectPrefab";
        GameObject prefabEffect = Resources.Load<GameObject>(path);
        GameObject objEffect = Instantiate(prefabEffect);
        UIEffect compEffect = objEffect.GetComponent<UIEffect>();

        return compEffect;
    }
}
