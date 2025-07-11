using UnityEditor;
using UnityEngine;
using Map; // namespace Map

[CustomEditor(typeof(MapManager))]
public class MapManagerEditor : Editor {
    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        MapManager mapManager = (MapManager)target;
        if (GUILayout.Button("Generate New Map")) {
            mapManager.GenerateNewMap();
        }
    }
}