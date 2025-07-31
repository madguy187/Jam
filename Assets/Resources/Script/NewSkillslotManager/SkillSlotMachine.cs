using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillSlotMachine : MonoBehaviour
{
    public static SkillSlotMachine instance { get; private set; }

    [Header("Columns (Left → Right)")]
    [SerializeField] private SkillSlotGrid leftColumn;
    [SerializeField] private SkillSlotGrid centreColumn;
    [SerializeField] private SkillSlotGrid rightColumn;

    [Header("Roll Settings")]
    [SerializeField] private float columnStartDelay = 0f;

    [Header("Spin Cost Settings")]
    [SerializeField] private int baseSpinCost = 2;
    private int spinsThisTurn = 0;
    private bool hasSpunThisTurn = false;
    public bool CanAttack() => hasSpunThisTurn && !IsRolling();
    private SlotGrid detectorGrid;
    private readonly List<SkillSlotGrid> columns = new List<SkillSlotGrid>();
    private SpinResult lastSpinResult;
    private bool victoryProcessed = false;

    [Header("UI References")]
    [SerializeField] private UIRerollButton rerollButton;
    [SerializeField] private UIAttack       attackButton;

    // Turn or Combat thinGS
    private bool isEnemyTurn = false;
    public static bool IsEnemyTurnStatic => instance != null && instance.isEnemyTurn;

    public enum SpinMode
    {
        // rolls ONLY – NO combat
        PreviewOnly,       
        // player rolls, immediate player-side combat 
        PlayerCombat,       
        // player rolls + combat, then enemy auto-rolls + combat
        PlayerAndEnemy      
    }

    [SerializeField] public SpinMode spinMode = SpinMode.PlayerAndEnemy;
    [Header("Individual Roll Settings")]
    [SerializeField] private bool previewSpin;
    [SerializeField] private bool playerCombatSpin;
    [SerializeField] private bool fullCombatSpin;
    private bool victoryPopupShown = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }

        InitColumns();
        InitUI();
        InitState();
        // Initial button state – must spin first
        if (rerollButton != null) rerollButton.SetInteractable(true);
        if (attackButton != null) attackButton.SetInteractable(false);
        Global.DEBUG_PRINT($"[SkillSlotMachine] Awake: attack interactable after disable = {attackButton?.IsInteractable()}");
    }

    private void Start()
    {
        // Initial button state – must spin first
        Global.DEBUG_PRINT("[SkillSlotMachine] Start called");
        if (rerollButton != null) rerollButton.SetInteractable(true);
        if (attackButton != null) attackButton.SetInteractable(false);
    }

    private void Update()
    {
        if (previewSpin)
        {
            previewSpin = false;
            spinMode = SpinMode.PreviewOnly;
            Spin();
        }

        if (playerCombatSpin)
        {
            playerCombatSpin = false;
            spinMode = SpinMode.PlayerCombat;
            Spin();
        }

        if (fullCombatSpin)
        {
            fullCombatSpin = false;
            spinMode = SpinMode.PlayerAndEnemy;
            Spin();
        }

        if (!victoryProcessed && CheckAllEnemiesDead())
        {
            Global.DEBUG_PRINT("[SkillSlotMachine] All enemies dead, player wins!");
            GoldManager.instance.OnVictory();
            victoryProcessed = true;
            SetButtonsInteractable(false);
            return;
        }
    }

    private void InitColumns()
    {
        columns.Clear();
        if (leftColumn   != null) 
        {
            columns.Add(leftColumn);
        }

        if (centreColumn != null) 
        {
            columns.Add(centreColumn);
        }

        if (rightColumn  != null) 
        {
            columns.Add(rightColumn);
        }
    }

    private void InitUI()
    {
        if (rerollButton == null || attackButton == null)
        {
            Debug.LogError("[SkillSlotMachine] UI button references not set in Inspector.");
        }
    }

    private void InitState()
    {
        detectorGrid = new SlotGrid(3,3);
    }

    public bool IsRolling()
    {
        foreach (var col in columns)
        {
            if (col != null && col.IsRolling())
            {
                return true;
            }
        }
        return false;
    }

    public void Spin()
    {
        int cost = GetCurrentSpinCost();
        if (cost > 0)
        {
            if (!GoldManager.instance.SpendGold(cost))
            {
                Global.DEBUG_PRINT("[SkillSlotMachine] Not enough gold to spin. Cost: " + cost);
                return;
            }
        }

        // Every spin (including previews) counts toward escalating cost
        spinsThisTurn++;

        // Disable buttons during spin
        SetButtonsInteractable(false);

        if (IsRolling()) return;

        StartCoroutine(SpinRoutine());
    }

    private IEnumerator SpinRoutine()
    {
        // Trigger columns sequentially with optional delay
        foreach (var col in columns)
        {
            if (col != null)
            {
                col.Spin();
                if (columnStartDelay > 0f)
                {
                    yield return new WaitForSeconds(columnStartDelay);
                }
            }
        }

        // Wait until all columns finish
        while (IsRolling())
        {
            yield return null;
        }

        // Build resulting 3×3 archetype grid (rows: top→bottom, cols: left→right)
        eUnitArchetype[,] archetypeGrid = new eUnitArchetype[3, 3];
        var columnRefs = new[] { leftColumn, centreColumn, rightColumn };
        for (int c = 0; c < 3; c++)
        {
            var col = columnRefs[c];
            if (col == null) continue;

            List<eUnitArchetype> visibles = col.GetVisibleArchetypes();
            for (int r = 0; r < visibles.Count && r < 3; r++)
            {
                archetypeGrid[r, c] = visibles[r];
            }
        }

        // Convert to SymbolType array for MatchDetector / SlotController
        SymbolType[] symbols = new SymbolType[9];
        for (int r = 0; r < 3; r++)
        {
            for (int c = 0; c < 3; c++)
            {
                symbols[r * 3 + c] = ArchetypeToSymbol(archetypeGrid[r, c]);
            }
        }

        Global.DEBUG_PRINT("[SkillSlotMachine] Spin complete – symbols: " + string.Join(",", symbols));

        ProcessSpinResults(symbols);

    }

    // Helper that executes player-side combat using existing logic
    private void RunPlayerCombat()
    {
        ProcessPlayerTurnCombat();
    }

    // Enemy/preset spin support
    public IEnumerator SpinWithPresetSymbols(SymbolType[] presetSymbols)
    {
        if (IsRolling()) yield break;

        // Trigger columns visually (no gold spend / spin count)
        foreach (var col in columns)
        {
            if (col != null)
            {
                col.Spin();
                if (columnStartDelay > 0f)
                {
                    yield return new WaitForSeconds(columnStartDelay);
                }
            }
        }

        while (IsRolling())
        {
            yield return null;
        }

        // Process results for enemy spin
        ProcessSpinResults(presetSymbols);
    }

    private SymbolType ArchetypeToSymbol(eUnitArchetype archetype)
    {
        switch (archetype)
        {
            case eUnitArchetype.HOLY:
                return SymbolType.HOLY;
            case eUnitArchetype.UNDEAD:
                return SymbolType.UNDEAD;
            case eUnitArchetype.ELF:
                return SymbolType.ELF;
            case eUnitArchetype.MOB:
                return SymbolType.MOB;               
            default:
                return SymbolType.EMPTY;
        }
    }

    private int GetCurrentSpinCost()
    {
        if (spinsThisTurn == 0)
        {
            return 0;
        }
        return baseSpinCost + ((spinsThisTurn - 1) * 2);
    }

    // Called by SlotController when turn changes (optional hook)
    public void ResetSpinCounter()
    {
        spinsThisTurn = 0;
        hasSpunThisTurn = false;
        if (attackButton != null) attackButton.SetInteractable(false);
        if (rerollButton != null) rerollButton.SetInteractable(true);
    }

    // Match detection & rewards 
    private void ProcessSpinResults(SymbolType[] symbols)
    {
        // Reuse persistent detector grid (no allocation each spin)
        detectorGrid.ClearGrid();
        hasSpunThisTurn = true;
        for (int i = 0; i < 9; i++)
        {
            int row = i / 3;
            int col = i % 3;
            detectorGrid.SetSlot(row, col, symbols[i]);
        }

        MatchDetector detector = new MatchDetector(detectorGrid);
        List<Match> matches = detector.DetectMatches();

        // Map symbol → archetype so rewards & debug show correct value
        foreach (Match m in matches)
        {
            m.SetArchetype(SymbolGenerator.GetArchetypeForSymbol(m.GetSymbol()));
        }

        int totalGold = 0;
        foreach (Match match in matches)
        {
            if (match.GetMatchType() != MatchType.SINGLE)
            {
                int reward = GoldManager.instance.GetGoldRewardForMatch(match.GetMatchType());
                totalGold += reward;
                GoldManager.instance.AddGold(reward);
            }
        }

        lastSpinResult = new SpinResult(matches, totalGold);

        // Debug output - , can be removed
        if (matches.Count > 0)
        {
            Global.DEBUG_PRINT("=== MATCHES FOUND ===");
            foreach (Match m in matches)
            {
                string posStr = string.Join(", ", m.GetPositions().ConvertAll(PosName));
                Global.DEBUG_PRINT($"{m.GetMatchType()} : {m.GetArchetype()} @ {posStr}");
            }
            Global.DEBUG_PRINT("=====================");
        }

        switch (spinMode)
        {
            // no combat, no gold
            case SpinMode.PreviewOnly:
                SetButtonsInteractable(true);      
                return;

            // player side only
            case SpinMode.PlayerCombat:
                RunPlayerCombat();                 
                SetButtonsInteractable(true);
                return;

            // player roll -> attack -> enemy roll -> attack
            case SpinMode.PlayerAndEnemy:
                RunPlayerCombat();                 
                EndPlayerTurn();                   
                break;
        }
    }

    private void SetButtonsInteractable(bool state)
    {
        Global.DEBUG_PRINT($"[SkillSlotMachine] SetButtonsInteractable({state}) called. hasSpunThisTurn={hasSpunThisTurn}");
        if (rerollButton != null)
        {
            rerollButton.SetInteractable(state);
        }

        if (attackButton != null)
        {
            bool attackState = state && hasSpunThisTurn;
            attackButton.SetInteractable(attackState);
            Global.DEBUG_PRINT($"[SkillSlotMachine] Attack button interactable set to {attackState}");
        }
    }

    private string PosName(Vector2Int p)
    {
        return (p.x, p.y) switch
        {
            (0,0) => "TOPLEFT",
            (0,1) => "TOPMID",
            (0,2) => "TOPRIGHT",
            (1,0) => "MIDLEFT",
            (1,1) => "CENTER",
            (1,2) => "MIDRIGHT",
            (2,0) => "BOTLEFT",
            (2,1) => "BOTMID",
            (2,2) => "BOTRIGHT",
            _      => p.ToString()
        };
    }

    // Turn or Combat things

    public void EndPlayerTurn()
    {
        Global.DEBUG_PRINT("=== PLAYER TURN COMBAT EXECUTION ===");

        ProcessPlayerTurnCombat();

        GoldManager.instance.CalculateInterest();

        isEnemyTurn = true;
        SetButtonsInteractable(false);
        StartCoroutine(ExecuteFullCombatRoutine());
    }

    private void ProcessPlayerTurnCombat()
    {
        if (lastSpinResult == null) return;
        List<Match> matches = lastSpinResult.GetAllMatches();
        if (matches.Count == 0) return;
        CombatManager.instance.StartBattleLoop(matches);
    }

    public void ExecuteFullCombat()
    {
        if (isEnemyTurn || IsRolling()) return;
        // lock UI immediately
        SetButtonsInteractable(false);
        StartCoroutine(ExecuteFullCombatRoutine());
    }

    // ---------------- NEW Combat coroutine ----------------
    private IEnumerator ExecuteFullCombatRoutine()
    {
        // 1. capture player matches from previous spin
        if (lastSpinResult == null)
        {
            yield break;
        }
        List<Match> playerMatches = new List<Match>(lastSpinResult.GetAllMatches());
        
        // update help panel with player matches for this attack
        if (MatchHelpScreen.instance != null)
        {
            MatchHelpScreen.instance.DisplaySpinResults(new SpinResult(playerMatches,0));
        }

        // 2. generate enemy symbols & matches immediately
        Deck enemyDeck = DeckManager.instance.GetDeckByType(eDeckType.ENEMY);
        SymbolType[] enemySymbols = SymbolGenerator.instance.GenerateSymbolsForDeck(enemyDeck);
        List<Match> enemyMatches = BuildMatchesFromSymbols(enemySymbols);

        // create a new SpinResult so pop-ups use enemy matches
        lastSpinResult = new SpinResult(enemyMatches, 0);
        ShowEnemyRollPopups(enemyMatches, enemyDeck);

        // 3. assign UnitName for roll resolution
        AssignMatchUnitNames(playerMatches, eDeckType.PLAYER);
        AssignMatchUnitNames(enemyMatches,  eDeckType.ENEMY);

        // 4. merge lists and run single battle loop
        List<Match> all = new List<Match>(playerMatches.Count + enemyMatches.Count);
        all.AddRange(playerMatches);
        all.AddRange(enemyMatches);

        CombatManager.instance.StartBattleLoop(all);

        // 5. wait until combat manager finishes
        while (CombatManager.instance.IsRunning())
            yield return null;

        spinsThisTurn = 0;
        // Enable only reroll; attack needs new spin
        if (rerollButton != null) rerollButton.SetInteractable(true);
        if (attackButton != null) attackButton.SetInteractable(false);
        hasSpunThisTurn = false;
    }

    // helper builds matches without side-effects
    private List<Match> BuildMatchesFromSymbols(SymbolType[] symbols)
    {
        SlotGrid grid = new SlotGrid(3,3);
        for (int i = 0; i < 9; i++)
        {
            int row = i / 3;
            int col = i % 3;
            grid.SetSlot(row,col, symbols[i]);
        }
        MatchDetector det = new MatchDetector(grid);
        List<Match> matches = det.DetectMatches();
        foreach (var m in matches)
            m.SetArchetype(SymbolGenerator.GetArchetypeForSymbol(m.GetSymbol()));
        return matches;
    }

    private void AssignMatchUnitNames(List<Match> matches, eDeckType deckType)
    {
        Deck deck = DeckManager.instance.GetDeckByType(deckType);
        foreach (var m in matches)
        {
            string picked = null;
            foreach (UnitObject u in deck)
            {
                if (u == null || u.IsDead()) continue;
                // fallback first alive
                if (picked == null) picked = u.unitSO.unitName; 
                if (u.unitSO.eUnitArchetype == m.GetArchetype())
                {
                    picked = u.unitSO.unitName;
                    break;
                }
            }
            m.SetUnitName(picked);
        }
    }

    private bool CheckAllEnemiesDead()
    {
        Deck enemyDeck = DeckManager.instance.GetDeckByType(eDeckType.ENEMY);

        // Determine if the enemy deck ever contained at least one unit.
        bool hadEnemyUnits = false;
        foreach (UnitObject u in enemyDeck)
        {
            if (u != null)
            {
                hadEnemyUnits = true;
                break;
            }
        }

        if (!hadEnemyUnits)
        {
            // Nothing to defeat yet – dont auto-win.
            return false;
        }

        if (!DeckHelperFunc.IsDeckEmptyOrDead(enemyDeck))
        {
            // Still some enemies alive.
            return false;
        }

        return true;
    }

    // Public helpers
    public void ForceSpinPreview()
    {
        spinMode = SpinMode.PreviewOnly;
        Spin();
    }

    public void ForceSpinPlayerCombat()
    {
        spinMode = SpinMode.PlayerCombat;
        Spin();
    }

    public void ForceSpinFullCombat()
    {
        spinMode = SpinMode.PlayerAndEnemy;
        Spin();
    }

    // ---------------- Public API For spinning ----------------

    public void TriggerSpinMode(SpinMode mode)
    {
        spinMode = mode;
        Spin();
    }

    // player roll + combat + enemy counter-roll
    public void TriggerFullCombatSpin()
    {
        TriggerSpinMode(SpinMode.PlayerAndEnemy);
    }

    // Preview spin – rolls visuals only, no combat logic
    public void TriggerPreviewSpin()
    {
        TriggerSpinMode(SpinMode.PreviewOnly);
    }

    // Player-only combat spin – player rolls and resolves combat, Enemy does not battle
    public void TriggerPlayerCombatSpin()
    {
        TriggerSpinMode(SpinMode.PlayerCombat);
    }

    // ---------------- Combat-only helpers ----------------

    // Execute combat without performing another spin.
    // If enemyRetaliates is true, enemy will roll once and attack back
    public void ExecuteCombat(bool enemyRetaliates)
    {
        if (IsRolling())
        {
            Debug.LogWarning("[SkillSlotMachine] Cannot execute combat while reels are still spinning");
            return;
        }

        // Player side combat
        RunPlayerCombat();

        if (enemyRetaliates == false)
        {
            SetButtonsInteractable(true);
            // Player-only combat ends here
            return; 
        }

        // uses existing EndPlayerTurn which handles enemy roll + combat
        EndPlayerTurn();
    }

    public void ExecutePlayerCombatOnly()
    {
        ExecuteCombat(false);
    }

    public void ExecuteFullCombatButton()
    {
        ExecuteCombat(true);
    }

    // UI helpers
    void ShowEnemyRollPopups(List<Match> matches, Deck enemyDeck)
    {
        int matchCount = matches?.Count ?? 0;

        foreach (Match m in matches)
        {
            // Find an alive unit of the same archetype for the icon
            UnitObject matchUnit = null;
            for (int i = 0; i < enemyDeck.GetDeckMaxSize(); i++)
            {
                UnitObject u = enemyDeck.GetUnitObject(i);
                if (u != null && !u.IsDead() && u.unitSO != null && u.unitSO.eUnitArchetype == m.GetArchetype())
                {
                    matchUnit = u;
                    break;
                }
            }

            Sprite iconSprite = matchUnit != null ? RenderUtilities.RenderUnitHeadSprite(matchUnit) : null;

            string popupText;
            if (matchUnit != null && matchUnit.unitSO != null)
            {
                popupText = $"{m.GetMatchType()} {matchUnit.unitSO.unitName}";
            }
            else
            {
                popupText = $"{m.GetMatchType()} {m.GetArchetype()}";
            }

            if (iconSprite != null)
            {
                UIPopUpManager.instance.CreatePopUp(popupText, iconSprite);
            }
            else
            {
                UIPopUpManager.instance.CreatePopUp(popupText);
            }
        }
    }
} 