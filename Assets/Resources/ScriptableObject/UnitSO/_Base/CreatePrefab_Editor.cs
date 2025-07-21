using UnityEditor;
using UnityEngine;

public class CreatePrefabFromMenu
{
    [MenuItem("Assets/Create/Scriptable Object/UnitPrefab", priority = 10)]
    public static void CreatePrefab()
    {
        // Create a new GameObject (or use an existing one)
        string prefabPath = "ScriptableObject/UnitSO/_Base/UnitPrefab";
        GameObject prefab = Resources.Load<GameObject>(prefabPath);
        GameObject gameObject = (GameObject)PrefabUtility.InstantiatePrefab(prefab);

        // Add components, modify the GameObject as needed

        // Save the GameObject as a prefab
        string assetPath = "Assets/Resources/ScriptableObject/UnitSO/Unit/NewUnit.prefab"; // Define the asset path
        bool prefabSuccess;
        PrefabUtility.SaveAsPrefabAsset(gameObject, assetPath, out prefabSuccess);

        if (prefabSuccess) {
            Debug.Log("Prefab created successfully at: " + assetPath);
        } else {
            Debug.LogError("Failed to create prefab.");
        }

        // Optionally destroy the temporary GameObject
        Object.DestroyImmediate(gameObject);
    }
}