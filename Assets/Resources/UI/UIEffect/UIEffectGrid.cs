using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UIEffectGrid : MonoBehaviour {
    [SerializeField] Transform transParentCanvas;

    Dictionary<EffectType, UIEffect> _mapEffectObj = new Dictionary<EffectType, UIEffect>();

    public UnitObject GetUnit() { return PanelManager.instance.GetCurrentUnit(); }

    void Update() {
        UnitObject unit = GetUnit();
        if (unit == null) {
            return;
        }

        List<EffectType> listKey = _mapEffectObj.Keys.ToList();

        EffectMap effectMap = unit.GetAllTempEffect();
        foreach (EffectObject effectObj in effectMap) {
            EffectType eType = effectObj.GetEffectType();

            if (listKey.Contains(eType)) {
                listKey.Remove(eType);
            }

            if (HasEffectUI(eType)) {
                continue;
            }

            AddEffectUI(eType);
        }

        foreach (EffectType eTypeRemove in listKey) {
            UIEffect uiEffect = _mapEffectObj[eTypeRemove];
            if (uiEffect != null) {
                Destroy(uiEffect.gameObject);
            }
            _mapEffectObj.Remove(eTypeRemove);
        }
    }

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
