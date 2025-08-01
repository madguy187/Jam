using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UIEffect : ToolTipDetails {
    UIEffectGrid _gridParent = null;
    EffectDetailScriptableObject _effectDetail = null;
    [SerializeField] SpriteRenderer spriteComp = null;
    [SerializeField] Image imageComp = null; // optional UI Image for root

    const string KEYWORD_PARAM = "%param%";

    public void SetGridParent(UIEffectGrid _grid) { _gridParent = _grid; }

    public void SetText(EffectType eType) {
        UnitObject unit = _gridParent.GetUnit();
        if (unit == null) {
            Debug.LogError("[UIEffect] Unit is null");
            return;
        }

        _effectDetail = ResourceManager.instance.GetEffectDetail(eType);
        string strDescription = "";
        string strEffectName;
        if (_effectDetail != null)
        {
            spriteComp.sprite = _effectDetail.sprite;
            strEffectName = _effectDetail.strEffectName;
            strDescription = _effectDetail.strDescription;
        }
        else
        {
            strEffectName = eType.ToString();
            spriteComp.sprite = null; // will fallback below
        }

        // Ensure we always have a sprite
        if (spriteComp.sprite == null)
        {
            Debug.LogWarning($"[UIEffect] No sprite found for effect {eType}. Using default.");
            spriteComp.sprite = ResourceManager.instance.GetDefaultEffectSprite();
        }

        // Mirror onto optional Image component for UI canvas visibility
        if (imageComp == null)
        {
            imageComp = GetComponent<Image>();
        }
        if (imageComp != null)
        {
            imageComp.sprite = spriteComp.sprite;
            imageComp.enabled = true;
        }

        if (strDescription.Contains(KEYWORD_PARAM)) {
            if (unit.GetEffectParam(eType, out float param)) {
                strDescription = strDescription.Replace(KEYWORD_PARAM, param.ToString());
            }
        }
        Init(strEffectName, strDescription);
    }

    public void UpdateValue() {
        UnitObject unit = _gridParent.GetUnit();
        if (unit == null) {
            Debug.LogError("[UIEffect] Unit is null");
            return;
        }

        EffectType eType = EffectType.NONE;
        string strDescription = "";
        if (_effectDetail != null) {
            eType = _effectDetail.eEffectType;
            strDescription = _effectDetail.strDescription;
        }
        
        if (strDescription.Contains(KEYWORD_PARAM)) {
            if (unit.GetEffectParam(eType, out float param)) {
                strDescription = strDescription.Replace(KEYWORD_PARAM, param.ToString());
            }
        }
    }
}
