using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
public class CreatePrefabFromMenu {
    const string UNIT_DEFAULT_PREFAB_PATH = "ScriptableObject/UnitSO/_Base/UnitPrefab";
    const string UNIT_DEFAULT_EFFECT_FULL_PATH = "Assets/Resources/" + "ScriptableObject/UnitSO/_Base/EffectDefault.asset";

    const string UNIT_SCRIPTABLE_PATH = "ScriptableObject/UnitSO/UnitEffect";
    const string UNIT_SCRIPTABLE_FULL_PATH = "Assets/Resources/" + UNIT_SCRIPTABLE_PATH;

    const string UNIT_SCRIPTABLE_MOB_PATH = "ScriptableObject/UnitSO/MobEffect";
    const string UNIT_SCRIPTABLE_MOB_FULL_PATH = "Assets/Resources/" + UNIT_SCRIPTABLE_MOB_PATH;

    const string UNIT_PREFAB_PATH = "ScriptableObject/UnitSO/Unit";
    const string UNIT_PREFAB_FULL_PATH = "Assets/Resources/" + UNIT_PREFAB_PATH;

    const string UNIT_PREFAB_MOB_PATH = "ScriptableObject/UnitSO/Mob";
    const string UNIT_PREFAB_MOB_FULL_PATH = "Assets/Resources/" + UNIT_PREFAB_MOB_PATH;

    const string UNIT_RELIC_FULL_PATH = "Assets/Resources/ScriptableObject/RelicSO/Relic";
    const string UNIT_RELIC_EFFECT_PATH = "ScriptableObject/RelicSO/RelicEffect";
    const string UNIT_RELIC_EFFECT_FULL_PATH = "Assets/Resources/" + UNIT_RELIC_EFFECT_PATH;
    const string UNIT_DEFAULT_RELIC_FULL_PATH = "Assets/Resources/" + "ScriptableObject/RelicSO/_Base/RelicDefault.asset";

    const string HEALTH_BAR_PATH = "UI/UIHealthBar/UIHealthBar";
    const string EFFECT_GRID_PATH = "UI/UIEffect/UIEffectGrid";

    const string FOLDER_SEPARATOR = "/";

    [MenuItem("Assets/Create/Scriptable Object/LoadAll", priority = 11)]
    public static void LoadAllSO() {
        string[] folders = AssetDatabase.GetSubFolders(UNIT_SCRIPTABLE_FULL_PATH);
        foreach (string path in folders) {
            CreateUnit(UNIT_PREFAB_PATH, UNIT_PREFAB_FULL_PATH, UNIT_SCRIPTABLE_PATH, Path.GetFileName(path));
        }

        string[] foldersMob = AssetDatabase.GetSubFolders(UNIT_SCRIPTABLE_MOB_FULL_PATH);
        foreach (string path in foldersMob) {
            CreateUnit(UNIT_PREFAB_MOB_PATH, UNIT_PREFAB_MOB_FULL_PATH, UNIT_SCRIPTABLE_MOB_PATH, Path.GetFileName(path));
        }

        AssetDatabase.Refresh();
    }

    [MenuItem("Assets/Create/Scriptable Object/LoadAllRelic", priority = 11)]
    public static void LoadAllRelicSO() {
        string[] folders = AssetDatabase.GetSubFolders(UNIT_RELIC_EFFECT_FULL_PATH);

        foreach (string path in folders) {
            CreateRelic(Path.GetFileName(path));
        }

        AssetDatabase.SaveAssets(); // Ensure changes are saved
        AssetDatabase.Refresh(); // Refresh the Project window to show the new asset
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

    static GameObject GetPrefabUnit(string path) {
        // Create a new GameObject (or use an existing one)
        GameObject prefab = Resources.Load<GameObject>(path);
        return prefab;
    }

    static void AttachHealthBar(ref GameObject obj) {
        GameObject objHealthBar = null;
        if (obj.GetComponentInChildren<UIHealthBar>() != null) {
            objHealthBar = obj.GetComponentInChildren<UIHealthBar>().gameObject;
        } else {
            GameObject prefab = Resources.Load<GameObject>(HEALTH_BAR_PATH);
            objHealthBar = GameObject.Instantiate(prefab);
            objHealthBar.transform.parent = obj.transform;
        }

        UnitObject unitComp = obj.GetComponent<UnitObject>();
        UIHealthBar healthBarComp = objHealthBar.GetComponent<UIHealthBar>();
        healthBarComp.SetUnit(unitComp);

        Vector3 pos = new Vector3(0.0f, obj.transform.position.y + 0.9f, 0.0f);
        RectTransform healthBarTrans = objHealthBar.GetComponent<RectTransform>();
        healthBarTrans.anchoredPosition = pos;
    }

    static void AttachEffectGrid(ref GameObject obj) {
        Transform transOldGrid = obj.transform.Find("UIEffectGrid(Clone)");
        if (transOldGrid != null) {
            GameObject.DestroyImmediate(transOldGrid.gameObject);
        }

        // if (obj.GetComponentInChildren<UIEffectGrid>() != null) {
        //     objGrid = obj.GetComponentInChildren<UIEffectGrid>().gameObject;
        //     GameObject.Destroy(objGrid);
        // }

        GameObject prefab = Resources.Load<GameObject>(EFFECT_GRID_PATH);
        GameObject objGrid = GameObject.Instantiate(prefab);
        objGrid.transform.SetParent(obj.transform);

        UnitObject unitComp = obj.GetComponent<UnitObject>();
        UIEffectGrid effectComp = objGrid.GetComponent<UIEffectGrid>();
        effectComp.SetUnit(unitComp);
        unitComp.SetEffectUIGrid(effectComp);

        Vector3 pos = new Vector3(0.0f, obj.transform.position.y, 0.0f);
        RectTransform healthBarTrans = objGrid.GetComponent<RectTransform>();
        healthBarTrans.anchoredPosition = pos;
    }

    static void CreateUnit(string path, string outputPath, string scriptablePath, string unitName) {
        GameObject prefab = null;

        bool isCreateNew = false;
        if (CheckIfPrefabExist(path, unitName)) {
            prefab = GetPrefabUnit(path + FOLDER_SEPARATOR + unitName);
            //FileUtil.DeleteFileOrDirectory(Application.dataPath + UNIT_PREFAB_DATA_PATH + FOLDER_SEPARATOR + unitName + ".prefab");
            //FileUtil.DeleteFileOrDirectory(Application.dataPath + UNIT_PREFAB_DATA_PATH + FOLDER_SEPARATOR + unitName + ".meta");
        } else {
            // Create a new GameObject
            isCreateNew = true;
            prefab = GetPrefabUnit(UNIT_DEFAULT_PREFAB_PATH);
        }

        GameObject gameObj = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        gameObj.transform.position = Vector3.zero;

        float size = 2.0f;
        gameObj.transform.localScale = new Vector3(size, size, size);

        UnitObject comp = gameObj.GetComponent<UnitObject>();
        if (comp == null) {
            comp = gameObj.AddComponent<UnitObject>();
        }

        AttachHealthBar(ref gameObj);
        AttachEffectGrid(ref gameObj);

        string infoPath = scriptablePath + FOLDER_SEPARATOR + unitName;
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
        string assetPath = outputPath + FOLDER_SEPARATOR + unitName + ".prefab"; // Define the asset path
        bool prefabSuccess;
        PrefabUtility.SaveAsPrefabAsset(gameObj, assetPath, out prefabSuccess);

        if (prefabSuccess) {
            if (isCreateNew) {
                Debug.Log("Prefab created successfully at: " + assetPath);
            } else {
                Debug.Log("Prefab updated successfully at: " + assetPath);
            }
        } else {
            Debug.LogError("Failed to create prefab.");
        }

        // Optionally destroy the temporary GameObject
        GameObject.DestroyImmediate(gameObj);
    }

    static public void CreateRelic(string relicName) {
        string relic_path = UNIT_RELIC_FULL_PATH + FOLDER_SEPARATOR + relicName;
        RelicScriptableObject relicSO = AssetDatabase.LoadAssetAtPath<RelicScriptableObject>(relic_path + ".asset");
        if (relicSO == null) {
            AssetDatabase.CopyAsset(UNIT_DEFAULT_RELIC_FULL_PATH, relic_path + ".asset");
            relicSO = AssetDatabase.LoadAssetAtPath<RelicScriptableObject>(relic_path + ".asset");
        }

        string relic_effect_path = UNIT_RELIC_EFFECT_PATH + FOLDER_SEPARATOR + relicName;
        List<EffectScriptableObject> effects = Resources.LoadAll<EffectScriptableObject>(relic_effect_path).ToList();
        relicSO.SetEffectList(effects);

        EditorUtility.SetDirty(relicSO);
    }

    static bool CheckIfPrefabExist(string prefabPath, string unitName) {
        GameObject prefab = Resources.Load<GameObject>(prefabPath + FOLDER_SEPARATOR + unitName);
        if (prefab == null) {
            return false;
        }

        return true;
    }
}
#endif