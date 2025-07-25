using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Object/Unit/EffectDetailScriptableObject")]
public class EffectDetailScriptableObject : ScriptableObject {
    public EffectType eEffectType;
    public string strEffectName = "";
    public string strDescription = "";
    public Sprite sprite;

#if UNITY_EDITOR
    [Header("Do not allow changes, this is done to fix the enum from changing")]
    [SerializeField] bool IsLocked = false;

    string _strEditorOriginal_EffectType = "";

    public void OnBeforeSerialize() {
        _strEditorOriginal_EffectType = SaveEnum(eEffectType);
    }

    public void OnAfterDeserialize() {
        if (IsLocked) {
            LoadEnum(_strEditorOriginal_EffectType, ref eEffectType);
        }
    }

    public void LoadEnum<T>(string enumName, ref T enumVal) {
        if (enumName == "") {
            return;
        }

        System.Object obj = (T)Enum.Parse(typeof(T), enumName, true);
        if (obj == null) {
            return;
        }

        enumVal = (T)obj;
    }

    public string SaveEnum<T>(T enumVal) {
        if (enumVal != null) {
            return enumVal.ToString();
        }

        return "";
    }

    public bool GetLock() {
        return IsLocked;
    }
#endif
}
