using System.Collections.Generic;
using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Object/Unit/RelicScriptableObject")]
public class RelicScriptableObject : ScriptableObject, IEnumerable<EffectScriptableObject> 
{
#if UNITY_EDITOR
    public void SetRelicName(string relicName) { strRelicName = relicName; }
    public void SetRelicSprite(Sprite sprite) { spriteRelic = sprite; }
    public void SetEffectList(List<EffectScriptableObject> list) { listEffect = list; }
#endif
    
    [SerializeField] string strRelicName = "";
    [SerializeField] Sprite spriteRelic = null;
    [SerializeField] List<EffectScriptableObject> listEffect;

    public string GetRelicName() {
        return strRelicName;
    }
    
    // For IEnumerable<EffectScriptableObject>
    public IEnumerator<EffectScriptableObject> GetEnumerator() { return listEffect.GetEnumerator(); }
    IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
}
