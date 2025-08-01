using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using UnityEngine.SceneManagement;

public enum eDeckType {
    NONE,
    PLAYER,
    ENEMY,
}

public class DeckManager : MonoBehaviour {
    public static DeckManager instance;

    [SerializeField] Transform _transPlayerPos;
    [SerializeField] Transform _transEnemyPos;

    Deck cPlayerDeck = new Deck();
    Deck cEnemyDeck = new Deck();

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void Clear() 
    {
        cPlayerDeck.DestroyAllUnit();
        cEnemyDeck.DestroyAllUnit();
    }
    
    void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        // This function will be called every time a scene is loaded.
        // You can add logic here based on the loaded scene's name or build index.
        Debug.Log("Scene " + scene.name + " loaded in mode: " + mode);

        cEnemyDeck.DestroyAllUnit();

        // Example: Perform actions only for a specific scene
        if (scene.name == "Game") {
            AudioManager.instance.StopEssential(AudioManager.EssentialAudio.Main);
            AudioManager.instance.PlayEssential(AudioManager.EssentialAudio.Combat);
            EnemyManager.instance.PopulateMobBasedOnNodeType();
        } else {
            AudioManager.instance.StopEssential(AudioManager.EssentialAudio.Combat);
            AudioManager.instance.PlayEssential(AudioManager.EssentialAudio.Main);
        }
    }

    void Update() {

    }

    void Awake() {
        if (instance != null) {
            instance.SetAllPositionByType(eDeckType.PLAYER, GetAllPositionByType(eDeckType.PLAYER));
            instance.SetAllPositionByType(eDeckType.ENEMY, GetAllPositionByType(eDeckType.ENEMY));
            Destroy(gameObject);
        } else {
            instance = this;
            if (transform.parent != null) {
                Global.DEBUG_PRINT("Detach from parent to persist scene loads");
                transform.SetParent(null);
            }
            DontDestroyOnLoad(gameObject);
        }
    }

    void Start() {
        {
            List<UnitPosition> vecPos = new List<UnitPosition>();
            UnitPosition[] vecPlayerUnitPos = _transPlayerPos.GetComponentsInChildren<UnitPosition>();
            foreach (UnitPosition playerPos in vecPlayerUnitPos) {
                vecPos.Add(playerPos);
            }
            cPlayerDeck.Init(eDeckType.PLAYER, vecPos);
            Global.DEBUG_PRINT("[Deck] Loaded PlayerPos: " + vecPos.Count);
        }

        {
            List<UnitPosition> vecPos = new List<UnitPosition>();
            UnitPosition[] vecEnemyUnitPos = _transEnemyPos.GetComponentsInChildren<UnitPosition>();
            foreach (UnitPosition enemyPos in vecEnemyUnitPos) {
                vecPos.Add(enemyPos);
            }
            cEnemyDeck.Init(eDeckType.ENEMY, vecPos);
            Global.DEBUG_PRINT("[Deck] Loaded EnemyPos: " + vecPos.Count);
        }
    }

    public UnitPosition[] GetAllPositionByType(eDeckType eType) {
        Transform trans = eType == eDeckType.PLAYER ? _transPlayerPos : _transEnemyPos;
        return trans.GetComponentsInChildren<UnitPosition>();
    }

    public void SetAllPositionByType(eDeckType eType, UnitPosition[] listPos) {
        Deck cDeck = instance.GetDeckByType(eType);
        //Deck cDeck = eType == eDeckType.PLAYER ? cPlayerDeck : cEnemyDeck;
        cDeck.ReInitUnitPosition(listPos.ToList());
    }

    public UnitObject AddUnit(eDeckType eType, string strUnitName) {
        Deck cDeck = GetDeckByType(eType);
        UnitObject unit = cDeck.AddUnit(strUnitName);

        return unit;
    }

    public Deck GetDeckByType(eDeckType eType) {
        Deck cDeck = null;

        switch (eType) {
            case eDeckType.PLAYER:
                cDeck = cPlayerDeck;
                break;
            case eDeckType.ENEMY:
                cDeck = cEnemyDeck;
                break;
            default:
                break;
        }

        return cDeck;
    }

    public void ResolveTurnTempEffect() {
        cPlayerDeck.ResolveTurn();
        cEnemyDeck.ResolveTurn();
    }

    public void InitDeckEffect() {
        cPlayerDeck.InitDeck();
        cEnemyDeck.InitDeck();
    }

#if UNITY_EDITOR
    [Header("Debug")]
    [SerializeField] bool bShowDebug = false;
    string strDebugUnit = "";
    string strDebugRelic = "";
    eDeckType eDebugDeckType = eDeckType.NONE;
    int nDebugUnitIndex = 0;
    bool bDebugSingle = false;
    bool bDebugHorizontal = false;
    bool bDebugDiagonal = false;
    bool bDebugZigZag = false;
    bool bDebugXShape = false;
    bool bDebugFullGrid = false;

    void OnGUI() {
        if (!bShowDebug) {
            return;
        }
        
        Array enumValues = Enum.GetValues(typeof(eDeckType));
        string[] enumNames = new string[enumValues.Length];

        // Convert enum values to string names for the toolbar
        for (int i = 0; i < enumValues.Length; i++) {
            enumNames[i] = enumValues.GetValue(i).ToString();
        }
        // Display the enum as a toolbar
        int newSelection = GUILayout.Toolbar((int)eDebugDeckType, enumNames);

        // Update the enum value if a new option is selected
        if (newSelection != (int)eDebugDeckType) {
            eDebugDeckType = (eDeckType)enumValues.GetValue(newSelection);
        }

        if (GUILayout.Button("Add Random Unit To Deck")) {
            string strPrefab = ResourceManager.instance.Debug_RandUnit();
            if (strPrefab == null) {
                Debug.Log("Error loading");
                return;
            }

            AddUnit(eDebugDeckType, strPrefab);

            Debug.Log("Loaded " + strPrefab + " into " + eDebugDeckType + " Team");
        }

        // Create a header label
        GUILayout.Space(20);
        GUILayout.Label("Adding specified unit", EditorStyles.boldLabel);
        strDebugUnit = GUILayout.TextField(strDebugUnit);
        if (GUILayout.Button("Add Unit To Enemy Deck")) {
            GameObject prefab = ResourceManager.instance.GetUnit(strDebugUnit);
            if (prefab == null) {
                return;
            }
            Deck cDeck = GetDeckByType(eDebugDeckType);

            cDeck.AddUnit(prefab);
            Debug.Log("Loaded " + prefab.name + " into " + eDebugDeckType + " Team");
        }

        GUILayout.Space(20);
        GUILayout.Label("Adding relic to unit", EditorStyles.boldLabel);
        string[] unitNum = { "1", "2", "3", "4", "5" };
        // Display the enum as a toolbar
        int newSelectionIndex = GUILayout.Toolbar(nDebugUnitIndex, unitNum);
        // Update the enum value if a new option is selected
        if (newSelectionIndex != nDebugUnitIndex) {
            nDebugUnitIndex = newSelectionIndex;
        }
        strDebugRelic = GUILayout.TextField(strDebugRelic);
        if (GUILayout.Button("Add Relic")) {
            RelicScriptableObject relicSO = ResourceManager.instance.GetRelic(strDebugRelic);
            if (relicSO == null) {
                return;
            }
            Deck cDeck = GetDeckByType(eDebugDeckType);
            UnitObject unit = cDeck.GetUnitObject(nDebugUnitIndex);
            unit.AddRelic(relicSO);

            Debug.Log("Loaded " + relicSO + " into " + eDebugDeckType + " Team");
        }

        GUILayout.Space(20);
        GUILayout.Label("Combat Debug", EditorStyles.boldLabel);
        if (GUILayout.Button("Init Round")) {
            CombatManager.instance.InitRound();
        }
        bDebugSingle = GUILayout.Toggle(bDebugSingle, "Single");
        bDebugHorizontal = GUILayout.Toggle(bDebugHorizontal, "Horizontal");
        bDebugDiagonal = GUILayout.Toggle(bDebugDiagonal, "Diagonal");
        bDebugZigZag = GUILayout.Toggle(bDebugZigZag, "ZigZag");
        bDebugXShape = GUILayout.Toggle(bDebugXShape, "XShape");
        bDebugFullGrid = GUILayout.Toggle(bDebugFullGrid, "FullGrid");
        if (GUILayout.Button("Exec 1 turn")) {
            List<Match> listMatch = new List<Match>();
            List<UnitObject> listUnit = DeckHelperFunc.GetAllAliveUnit(GetDeckByType(eDeckType.PLAYER));
            foreach (UnitObject unit in listUnit) {
                if (bDebugSingle) {
                    Match match = new Match(MatchType.SINGLE);
                    match.SetUnitName(unit.unitSO.unitName);
                    listMatch.Add(match);
                }
                if (bDebugHorizontal) {
                    Match match = new Match(MatchType.HORIZONTAL);
                    match.SetUnitName(unit.unitSO.unitName);
                    listMatch.Add(match);
                }
                if (bDebugDiagonal) {
                    Match match = new Match(MatchType.DIAGONAL);
                    match.SetUnitName(unit.unitSO.unitName);
                    listMatch.Add(match);
                }
                if (bDebugZigZag) {
                    Match match = new Match(MatchType.ZIGZAG);
                    match.SetUnitName(unit.unitSO.unitName);
                    listMatch.Add(match);
                }
                if (bDebugXShape) {
                    Match match = new Match(MatchType.XSHAPE);
                    match.SetUnitName(unit.unitSO.unitName);
                    listMatch.Add(match);
                }
                if (bDebugFullGrid) {
                    Match match = new Match(MatchType.FULLGRID);
                    match.SetUnitName(unit.unitSO.unitName);
                    listMatch.Add(match);
                }
            }
            CombatManager.instance.StartBattleLoop(listMatch);
        }
    }
#endif
}