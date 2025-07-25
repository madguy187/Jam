using System.Collections.Generic;
using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Object/Unit/RelicScriptableObject")]
public class RelicScriptableObject : ScriptableObject, IEnumerable<EffectScriptableObject> 
{
    [SerializeField] string strRelicName = "";
    [SerializeField] List<EffectScriptableObject> listEffect;

    public string GetRelicName() {
        return strRelicName;
    }
    
    // For IEnumerable<EffectScriptableObject>
    public IEnumerator<EffectScriptableObject> GetEnumerator() { return listEffect.GetEnumerator(); }
    IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
}
