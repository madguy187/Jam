using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class CreatePrefabFromMenu {
    const string UNIT_DEFAULT_PREFAB_PATH = "ScriptableObject/UnitSO/_Base/UnitPrefab";
    const string UNIT_DEFAULT_EFFECT_FULL_PATH = "Assets/Resources/" + "ScriptableObject/UnitSO/_Base/EffectDefault.asset";

    const string UNIT_SCRIPTABLE_PATH = "ScriptableObject/UnitSO/UnitEffect";
    const string UNIT_SCRIPTABLE_FULL_PATH = "Assets/Resources/" + UNIT_SCRIPTABLE_PATH;

    const string UNIT_PREFAB_PATH = "ScriptableObject/UnitSO/Unit";
    const string UNIT_PREFAB_FULL_PATH = "Assets/Resources/" + UNIT_PREFAB_PATH;

    const string UNIT_PREFAB_DATA_PATH = "/Resources/" + UNIT_PREFAB_PATH;

    const string FOLDER_SEPARATOR = "/";

    [MenuItem("Assets/Create/Scriptable Object/LoadAll", priority = 11)]
    public static void LoadAllSO() {
        string[] folders = AssetDatabase.GetSubFolders(UNIT_SCRIPTABLE_FULL_PATH);
        
        foreach (string path in folders) {
            CreateUnit(Path.GetFileName(path));
        }

        AssetDatabase.Refresh();
    }

    [MenuItem("Assets/Create/Scriptable Object/CreateSO", priority = 11)]
    public static void CreateSO() {
        string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        string unitName = Path.GetFileName(path);
        string outputPath = UNIT_SCRIPTABLE_FULL_PATH + FOLDER_SEPARATOR + unitName + FOLDER_SEPARATOR;
        AssetDatabase.CopyAsset(UNIT_DEFAULT_EFFECT_FULL_PATH, outputPath + unitName + "_Single_.asset");
        AssetDatabase.CopyAsset(UNIT_DEFAULT_EFFECT_FULL_PATH, outputPath + unitName + "_Horizontal_.asset");
        AssetDatabase.CopyAsset(UNIT_DEFAULT_EFFECT_FULL_PATH, outputPath + unitName + "_Diagonal_.asset");
        AssetDatabase.CopyAsset(UNIT_DEFAULT_EFFECT_FULL_PATH, outputPath + unitName + "_XShape_.asset");
        AssetDatabase.CopyAsset(UNIT_DEFAULT_EFFECT_FULL_PATH, outputPath + unitName + "_ZigZag_.asset");
        AssetDatabase.CopyAsset(UNIT_DEFAULT_EFFECT_FULL_PATH, outputPath + unitName + "_FullGrid_.asset");

        AssetDatabase.Refresh();
    }

    static GameObject GetDefaultPrefabUnit() {
        // Create a new GameObject (or use an existing one)
        string prefabPath = UNIT_DEFAULT_PREFAB_PATH;
        GameObject prefab = Resources.Load<GameObject>(prefabPath);
        return prefab;
    }

    static void CreateUnit(string unitName) {
        if (CheckIfPrefabExist(unitName)) {
            FileUtil.DeleteFileOrDirectory(Application.dataPath + UNIT_PREFAB_DATA_PATH + FOLDER_SEPARATOR + unitName + ".prefab");
            FileUtil.DeleteFileOrDirectory(Application.dataPath + UNIT_PREFAB_DATA_PATH + FOLDER_SEPARATOR + unitName + ".meta");
        }

        // Create a new GameObject (or use an existing one)
        GameObject prefab = GetDefaultPrefabUnit();
        GameObject gameObject = (GameObject)PrefabUtility.InstantiatePrefab(prefab);

        UnitObject comp = gameObject.GetComponent<UnitObject>();

        string infoPath = UNIT_SCRIPTABLE_PATH + FOLDER_SEPARATOR + unitName;
        List<UnitScriptableObject> unitSO = Resources.LoadAll<UnitScriptableObject>(infoPath).ToList();
        foreach (UnitScriptableObject unit in unitSO) {
            comp.SetUnitSO(unit);
        }

        List<EffectScriptableObject> effects = Resources.LoadAll<EffectScriptableObject>(infoPath).ToList();
        EffectList single = new EffectList();
        EffectList horizontal = new EffectList();
        EffectList diagonal = new EffectList();
        EffectList xshape = new EffectList();
        EffectList zigzag = new EffectList();
        EffectList fullgrid = new EffectList();
        foreach (EffectScriptableObject effect in effects) {
            if (effect.name.Contains("Single")) {
                single.AddEffect(effect);
            }
            if (effect.name.Contains("Horizontal")) {
                horizontal.AddEffect(effect);
            }
            if (effect.name.Contains("Diagonal")) {
                diagonal.AddEffect(effect);
            }
            if (effect.name.Contains("XShape")) {
                xshape.AddEffect(effect);
            }
            if (effect.name.Contains("ZigZag")) {
                zigzag.AddEffect(effect);
            }
            if (effect.name.Contains("FullGrid")) {
                fullgrid.AddEffect(effect);
            }
        }

        comp.SetEffectList_Single(single);
        comp.SetEffectList_Horizontal(horizontal);
        comp.SetEffectList_Diagonal(diagonal);
        comp.SetEffectList_XShape(xshape);
        comp.SetEffectList_ZigZag(zigzag);
        comp.SetEffectList_FullGrid(fullgrid);

        // Add components, modify the GameObject as needed

        // Save the GameObject as a prefab
        string assetPath = UNIT_PREFAB_FULL_PATH + FOLDER_SEPARATOR + unitName + ".prefab"; // Define the asset path
        bool prefabSuccess;
        PrefabUtility.SaveAsPrefabAsset(gameObject, assetPath, out prefabSuccess);

        if (prefabSuccess) {
            Debug.Log("Prefab created successfully at: " + assetPath);
        } else {
            Debug.LogError("Failed to create prefab.");
        }

        // Optionally destroy the temporary GameObject
        GameObject.DestroyImmediate(gameObject);
    }

    static bool CheckIfPrefabExist(string unitName) {
        string prefabPath = UNIT_PREFAB_PATH;
        GameObject prefab = Resources.Load<GameObject>(prefabPath + FOLDER_SEPARATOR + unitName);
        if (prefab == null) {
            return false;
        }

        return true;
    }
}