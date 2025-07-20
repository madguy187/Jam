using System.Collections.Generic;
using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Object/Unit/RelicScriptableObject")]
public class RelicScriptableObject : ScriptableObject, IEnumerable<EffectScriptableObject> 
{
    [SerializeField] List<EffectScriptableObject> listEffect;
    
    // For IEnumerable<EffectScriptableObject>
    public IEnumerator<EffectScriptableObject> GetEnumerator() { return listEffect.GetEnumerator(); }
    IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
}
