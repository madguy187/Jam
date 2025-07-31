using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
public class CreateEffectSO_Editor {
    const string DATA_PATH = "/Resources/ScriptableObject/UnitSO/Data/";
    const string EFFECT_PATH = "Assets/Resources/ScriptableObject/UnitSO/UnitEffect";
    const string EFFECT_PATH_MOB = "Assets/Resources/ScriptableObject/UnitSO/MobEffect";
    const string RELIC_EFFECT_PATH = "Assets/Resources/ScriptableObject/RelicSO/RelicEffect";
    const string EFFECT_DESCRIPTION_PATH = "Assets/Resources/ScriptableObject/EffectDetail/EffectDescription";
    const string FOLDER_SEPARATOR = "/";

    const string UNIT_RELIC_FULL_PATH = "Assets/Resources/ScriptableObject/RelicSO/Relic";
    const string UNIT_RELIC_EFFECT_PATH = "ScriptableObject/RelicSO/RelicEffect";
    const string UNIT_RELIC_EFFECT_FULL_PATH = "Assets/Resources/" + UNIT_RELIC_EFFECT_PATH;
    const string UNIT_DEFAULT_RELIC_FULL_PATH = "Assets/Resources/" + "ScriptableObject/RelicSO/_Base/RelicDefault.asset";

    const string EFFECT_DATA_FILE_NAME = "effect_output.csv";
    const string UNIT_DATA_FILE_NAME = "unit_output.csv";
    const string EFFECT_DESCRIPTION_DATA_FILE_NAME = "effect_description_output.csv";

    const char SEPARATOR = ',';

    const string HEADER_EFFECT_NAME = "name";
    const string HEADER_EFFECT_OUTPUT_TYPE = "output_type";
    const string HEADER_EFFECT_ENUM = "enum";
    const string HEADER_EFFECT_VAL = "val";
    const string HEADER_EFFECT_TARGET_TYPE = "target_type";
    const string HEADER_EFFECT_TARGET_CONDITION = "target_condition";
    const string HEADER_EFFECT_TARGET_PARAM = "target_param";
    const string HEADER_EFFECT_EXEC_TYPE = "exec_type";
    const string HEADER_EFFECT_AFFINITY_TYPE = "affinity_type";
    const string HEADER_EFFECT_RESOLUTION_TYPE = "resolution_type";
    const string HEADER_EFFECT_COUNT = "count";
    static List<string> _listHeader_Effect = new List<string> {
        HEADER_EFFECT_NAME,
        HEADER_EFFECT_OUTPUT_TYPE,
        HEADER_EFFECT_ENUM,
        HEADER_EFFECT_VAL,
        HEADER_EFFECT_TARGET_TYPE,
        HEADER_EFFECT_TARGET_CONDITION,
        HEADER_EFFECT_TARGET_PARAM,
        HEADER_EFFECT_EXEC_TYPE,
        HEADER_EFFECT_AFFINITY_TYPE,
        HEADER_EFFECT_RESOLUTION_TYPE,
        HEADER_EFFECT_COUNT,
    };

    const string HEADER_UNIT_NAME = "name";
    const string HEADER_UNIT_ARCHETYPE = "unit_archetype";
    const string HEADER_UNIT_TIER = "tier";
    const string HEADER_UNIT_HP = "hp";
    const string HEADER_UNIT_SHIELD = "shield";
    const string HEADER_UNIT_ATK = "atk";
    const string HEADER_UNIT_RES = "res";
    const string HEADER_UNIT_CRIT_RATE = "crit_rate";
    const string HEADER_UNIT_CRIT_MULTI = "crit_multi";
    static List<string> _listHeader_Unit = new List<string> {
        HEADER_UNIT_NAME,
        HEADER_UNIT_ARCHETYPE,
        HEADER_UNIT_TIER,
        HEADER_UNIT_HP,
        HEADER_UNIT_SHIELD,
        HEADER_UNIT_ATK,
        HEADER_UNIT_RES,
        HEADER_UNIT_CRIT_RATE,
        HEADER_UNIT_CRIT_MULTI,
    };

    const string HEADER_EFFECT_DESCRIPTION_ENUM = "enum";
    const string HEADER_EFFECT_DESCRIPTION_NAME = "name";
    const string HEADER_EFFECT_DESCRIPTION_DESCRIPTION = "desc";
    static List<string> _listHeader_EffectDescription = new List<string> {
        HEADER_EFFECT_DESCRIPTION_ENUM,
        HEADER_EFFECT_DESCRIPTION_NAME,
        HEADER_EFFECT_DESCRIPTION_DESCRIPTION,
    };

    [MenuItem("Assets/Create/Scriptable Object/LoadSOByCsv", priority = 11)]
    public static void LoadSOByCsv() {
        ClearAllParentFolder();
        LoadUnitSO();
        LoadEffectSO();
        //LoadEffectDescription();

        AssetDatabase.SaveAssets(); // Ensure changes are saved
        AssetDatabase.Refresh(); // Refresh the Project window to show the new asset
        
        CreatePrefabFromMenu.LoadAllSO();
        CreatePrefabFromMenu.LoadAllRelicSO();
        Debug.Log("Loaded");

        AssetDatabase.SaveAssets(); // Ensure changes are saved
        AssetDatabase.Refresh(); // Refresh the Project window to show the new asset
    }

    public static void LoadEffectDescription() {
        string path = Application.dataPath + FOLDER_SEPARATOR + DATA_PATH + EFFECT_DESCRIPTION_DATA_FILE_NAME;
        //Pass the file path and file name to the StreamReader constructor
        StreamReader sr = new StreamReader(path);
        //Read the first line of text
        string line = sr.ReadLine();
        if (!CheckHeaderEffect(_listHeader_EffectDescription, line)) {
            Debug.Log("Header do not match");
            return;
        }

        //Continue to read until you reach end of file
        while (true) {
            line = sr.ReadLine();
            if (line == null) {
                break;
            }

            if (LoadData(_listHeader_EffectDescription, line, out Dictionary<string, string> dictData)) {
                EffectDetailScriptableObject newAsset = ScriptableObject.CreateInstance<EffectDetailScriptableObject>();
                newAsset.eEffectType = (EffectType)GetEnumFromName<EffectType>(dictData[HEADER_EFFECT_DESCRIPTION_ENUM]);
                newAsset.strEffectName = dictData[HEADER_EFFECT_DESCRIPTION_NAME];
                newAsset.strDescription = dictData[HEADER_EFFECT_DESCRIPTION_DESCRIPTION];
                SaveEffectDescription(newAsset.eEffectType.ToString(), newAsset);
            }

        }
    }

    public static void LoadEffectSO() {
        string path = Application.dataPath + FOLDER_SEPARATOR + DATA_PATH + EFFECT_DATA_FILE_NAME;
        //Pass the file path and file name to the StreamReader constructor
        StreamReader sr = new StreamReader(path);
        //Read the first line of text
        string line = sr.ReadLine();
        if (!CheckHeaderEffect(_listHeader_Effect, line)) {
            Debug.Log("Header do not match");
            return;
        }

        //Continue to read until you reach end of file
        while (true) {
            line = sr.ReadLine();
            if (line == null) {
                break;
            }

            if (LoadData(_listHeader_Effect, line, out Dictionary<string, string> dictData)) {
                EffectScriptableObject newAsset = ScriptableObject.CreateInstance<EffectScriptableObject>();
                newAsset.SetEffectType((EffectType)GetEnumFromName<EffectType>(dictData[HEADER_EFFECT_ENUM]));
                newAsset.SetEffectVal(float.Parse(dictData[HEADER_EFFECT_VAL]));
                newAsset.SetEffectTargetType((EffectTargetType)GetEnumFromName<EffectTargetType>(dictData[HEADER_EFFECT_TARGET_TYPE]));
                newAsset.SetEffectTargetCondition((EffectTargetCondition)GetEnumFromName<EffectTargetCondition>(dictData[HEADER_EFFECT_TARGET_CONDITION]));
                newAsset.SetEffectTargetParam(float.Parse(dictData[HEADER_EFFECT_TARGET_PARAM]));
                newAsset.SetEffectExecType((EffectExecType)GetEnumFromName<EffectExecType>(dictData[HEADER_EFFECT_EXEC_TYPE]));
                newAsset.SetEffectAffinityType((EffectAffinityType)GetEnumFromName<EffectAffinityType>(dictData[HEADER_EFFECT_AFFINITY_TYPE]));
                newAsset.SetEffectResolveType((EffectResolveType)GetEnumFromName<EffectResolveType>(dictData[HEADER_EFFECT_RESOLUTION_TYPE]));
                newAsset.SetEffectCount(int.Parse(dictData[HEADER_EFFECT_COUNT]));

                string outputName = dictData[HEADER_EFFECT_NAME];
                string outputType = dictData[HEADER_EFFECT_OUTPUT_TYPE];


                if (outputType.Contains("roll")) {
                    outputType = outputType.Replace("roll_", "");
                    if (outputType.Contains("mob")) {
                        outputType = outputType.Replace("mob_", "");
                        CreateFolder(EFFECT_PATH_MOB, outputName, true);
                        SaveRollObjectMob(outputType, outputName, newAsset);
                    } else {
                        CreateFolder(EFFECT_PATH, outputName, true);
                        SaveRollObject(outputType, outputName, newAsset);
                    }
                }

                if (outputType.Contains("relic")) {
                    CreateFolder(RELIC_EFFECT_PATH, outputName, true);
                    SaveRelicEffectObject(outputName, newAsset);
                }
            }
        }
        //close the file
        sr.Close();
    }

    public static void LoadUnitSO() {
        string path = Application.dataPath + FOLDER_SEPARATOR + DATA_PATH + UNIT_DATA_FILE_NAME;
        //Pass the file path and file name to the StreamReader constructor
        StreamReader sr = new StreamReader(path);
        //Read the first line of text
        string line = sr.ReadLine();
        if (!CheckHeaderEffect(_listHeader_Unit, line)) {
            Debug.Log("Header do not match");
            return;
        }

        //Continue to read until you reach end of file
        while (true) {
            line = sr.ReadLine();
            if (line == null) {
                break;
            }

            if (LoadData(_listHeader_Unit, line, out Dictionary<string, string> dictData)) {
                UnitScriptableObject newAsset = ScriptableObject.CreateInstance<UnitScriptableObject>();
                eUnitArchetype eArchetype = (eUnitArchetype)GetEnumFromName<eUnitArchetype>(dictData[HEADER_UNIT_ARCHETYPE]);
                newAsset.SetunitName(dictData[HEADER_UNIT_NAME]);
                newAsset.SetunitArchetype(eArchetype);
                newAsset.SetunitTier((eUnitTier)GetEnumFromName<eUnitTier>(dictData[HEADER_UNIT_TIER]));
                newAsset.Sethp(float.Parse(dictData[HEADER_UNIT_HP]));
                newAsset.Setattack(float.Parse(dictData[HEADER_UNIT_ATK]));
                newAsset.Setshield(float.Parse(dictData[HEADER_UNIT_SHIELD]));
                newAsset.SetcritRate(int.Parse(dictData[HEADER_UNIT_CRIT_RATE]));
                newAsset.SetcritMulti(int.Parse(dictData[HEADER_UNIT_CRIT_MULTI]));
                newAsset.Setres(float.Parse(dictData[HEADER_UNIT_RES]));

                string outputName = dictData[HEADER_UNIT_NAME];


                if (eArchetype == eUnitArchetype.MOB) {
                    CreateFolder(EFFECT_PATH_MOB, outputName, true);
                    SaveRollObjectMob(outputName, newAsset);
                } else {
                    CreateFolder(EFFECT_PATH, outputName, true);
                    SaveRollObject(outputName, newAsset);
                }
            }
        }
        //close the file
        sr.Close();
    }

    public static void CreateFolder(string path, string folderName, bool bCheckIfExist = false) {
        if (bCheckIfExist) {
            if (AssetDatabase.IsValidFolder(path + FOLDER_SEPARATOR + folderName)) {
                return;
            }
        }
        AssetDatabase.CreateFolder(path, folderName);
    }

    public static void ClearAllParentFolder() {
        string effect_parent_path = EFFECT_PATH.Substring(0, EFFECT_PATH.LastIndexOf("/"));
        string effect_folder_name = EFFECT_PATH.Substring(EFFECT_PATH.LastIndexOf("/") + 1);
        string effect_mob_parent_path = EFFECT_PATH_MOB.Substring(0, EFFECT_PATH_MOB.LastIndexOf("/"));
        string effect_mob_folder_name = EFFECT_PATH_MOB.Substring(EFFECT_PATH_MOB.LastIndexOf("/") + 1);
        string relic_parent_path = RELIC_EFFECT_PATH.Substring(0, RELIC_EFFECT_PATH.LastIndexOf("/"));
        string relic_folder_name = RELIC_EFFECT_PATH.Substring(RELIC_EFFECT_PATH.LastIndexOf("/") + 1);
        string effect_description_path = EFFECT_DESCRIPTION_PATH.Substring(0, EFFECT_DESCRIPTION_PATH.LastIndexOf("/"));
        string effect_description_folder_name = EFFECT_DESCRIPTION_PATH.Substring(EFFECT_DESCRIPTION_PATH.LastIndexOf("/") + 1);

        if (AssetDatabase.IsValidFolder(EFFECT_PATH)) {
            AssetDatabase.DeleteAsset(EFFECT_PATH);
        }

        if (AssetDatabase.IsValidFolder(EFFECT_PATH_MOB)) {
            AssetDatabase.DeleteAsset(EFFECT_PATH_MOB);
        }

        if (AssetDatabase.IsValidFolder(RELIC_EFFECT_PATH)) {
            AssetDatabase.DeleteAsset(RELIC_EFFECT_PATH);
        }

        // Keep EffectDescription folder to preserve GUIDs
        // if (AssetDatabase.IsValidFolder(EFFECT_DESCRIPTION_PATH)) {
        //     AssetDatabase.DeleteAsset(EFFECT_DESCRIPTION_PATH);
        // }

        CreateFolder(effect_parent_path, effect_folder_name);
        CreateFolder(relic_parent_path, relic_folder_name);
        CreateFolder(effect_mob_parent_path, effect_mob_folder_name);
        CreateFolder(effect_description_path, effect_description_folder_name);
    }

    public static void SaveRollObject(string strRollType, string strOutputName, EffectScriptableObject effectSO) {
        string unitName = strOutputName;
        strOutputName = unitName + "_" + strRollType + "_" + effectSO.GetEffectType();
        AssetDatabase.CreateAsset(effectSO, EFFECT_PATH + FOLDER_SEPARATOR + unitName + FOLDER_SEPARATOR + strOutputName + ".asset");
    }


    public static void SaveRollObject(string strOutputName, UnitScriptableObject effectSO) {
        string unitName = strOutputName;
        strOutputName = unitName + "UnitSO";
        AssetDatabase.CreateAsset(effectSO, EFFECT_PATH + FOLDER_SEPARATOR + unitName + FOLDER_SEPARATOR + strOutputName + ".asset");
    }

    public static void SaveRollObjectMob(string strRollType, string strOutputName, EffectScriptableObject effectSO) {
        string unitName = strOutputName;
        strOutputName = unitName + "_" + strRollType + "_" + effectSO.GetEffectType();
        AssetDatabase.CreateAsset(effectSO, EFFECT_PATH_MOB + FOLDER_SEPARATOR + unitName + FOLDER_SEPARATOR + strOutputName + ".asset");
    }

    public static void SaveRollObjectMob(string strOutputName, UnitScriptableObject effectSO) {
        string unitName = strOutputName;
        strOutputName = unitName + "UnitSO";
        AssetDatabase.CreateAsset(effectSO, EFFECT_PATH_MOB + FOLDER_SEPARATOR + unitName + FOLDER_SEPARATOR + strOutputName + ".asset");
    }

    public static void SaveRelicEffectObject(string strOutputName, EffectScriptableObject effectSO) {
        string relicName = strOutputName;
        strOutputName = relicName + "_" + effectSO.GetEffectType();
        AssetDatabase.CreateAsset(effectSO, RELIC_EFFECT_PATH + FOLDER_SEPARATOR + relicName + FOLDER_SEPARATOR + strOutputName + ".asset");
    }

    public static void SaveEffectDescription(string strOutputName, EffectDetailScriptableObject effectSO) {
        AssetDatabase.CreateAsset(effectSO, EFFECT_DESCRIPTION_PATH + FOLDER_SEPARATOR + strOutputName + ".asset");
    }

    public static bool CheckHeaderEffect(List<string> listHeader, string text) {
        List<string> dataHeader = text.Split(SEPARATOR).ToList();
        if (!dataHeader.SequenceEqual(listHeader)) {
            return false;
        }

        return true;
    }

    public static bool LoadData(List<string> listHeader, string text, out Dictionary<string, string> dictData) {
        dictData = new Dictionary<string, string>();
        List<string> data = text.Split(SEPARATOR).ToList();

        if (data.Count != listHeader.Count) {
            return false;
        }

        for (int i = 0; i < listHeader.Count; i++) {
            string key = listHeader[i];
            string value = data[i];
            dictData.Add(key, value);
        }

        return true;
    }

    public static System.Object GetEnumFromName<T>(string strVal) {
        System.Object obj = Enum.Parse(typeof(T), strVal, true);
        return obj;
    }
}

#endif