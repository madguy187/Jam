using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Object/Unit/EffectDetailScriptableObject")]
public class EffectDetailScriptableObject : ScriptableObject {
    public EffectType eEffectType;
    public string strEffectName = "";
    public string strDescription = "";
    public Sprite sprite;
}
