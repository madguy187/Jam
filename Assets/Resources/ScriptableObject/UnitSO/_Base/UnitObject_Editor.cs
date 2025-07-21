using UnityEngine;
using UnityEditor;

[CustomEditor (typeof(UnitObject))]
public class UnitObject_Editor : Editor {
    void OnEnable()
    {
        var castedTarget = (target as UnitObject);

        castedTarget.GetComponent<RectTransform>().hideFlags = HideFlags.HideInInspector;
        castedTarget.GetComponent<SPUM_Prefabs>().hideFlags = HideFlags.HideInInspector;
    }
}
