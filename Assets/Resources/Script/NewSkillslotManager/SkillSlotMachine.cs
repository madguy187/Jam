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
    private SlotGrid detectorGrid;
    private readonly List<SkillSlotGrid> columns = new List<SkillSlotGrid>();
    private SpinResult lastSpinResult;

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

    public enum AttackBehaviour 
    { 
        PlayerCombatOnly, PlayerAndEnemyCombat 
    }


    [SerializeField] public SpinMode spinMode = SpinMode.PlayerAndEnemy;
    [Header("Individual Roll Settings")]
    [SerializeField] private bool previewSpin;
    [SerializeField] private bool playerCombatSpin;
    [SerializeField] private bool fullCombatSpin;
    [SerializeField] private AttackBehaviour behaviour = AttackBehaviour.PlayerAndEnemyCombat;

    [Header("Enemy Spin Settings")]
    // If false, enemy dont spin visually, results processed in background
    [SerializeField] private bool enemyVisualSpin = false; 

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
                Debug.Log("[SkillSlotMachine] Not enough gold to spin. Cost: " + cost);
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

        Debug.Log("[SkillSlotMachine] Spin complete – symbols: " + string.Join(",", symbols));

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
    }

    // Match detection & rewards 
    private void ProcessSpinResults(SymbolType[] symbols)
    {
        // Reuse persistent detector grid (no allocation each spin)
        detectorGrid.ClearGrid();
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

        // Update help panel text only during player's turn
        if (!isEnemyTurn && MatchHelpScreen.instance != null)
        {
            MatchHelpScreen.instance.DisplaySpinResults(lastSpinResult);
        }

        // Debug output - , can be removed
        if (matches.Count > 0)
        {
            Debug.Log("=== MATCHES FOUND ===");
            foreach (Match m in matches)
            {
                string posStr = string.Join(", ", m.GetPositions().ConvertAll(PosName));
                Debug.Log($"{m.GetMatchType()} : {m.GetArchetype()} @ {posStr}");
            }
            Debug.Log("=====================");
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
        if (rerollButton != null) rerollButton.SetInteractable(state);
        if (attackButton  != null) attackButton.SetInteractable(state);
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
        Debug.Log("=== PLAYER TURN COMBAT EXECUTION ===");

        ProcessPlayerTurnCombat();

        GoldManager.instance.CalculateInterest();

        if (CheckAllEnemiesDead())
        {
            Debug.Log("[SkillSlotMachine] All enemies dead, player wins!");
            GoldManager.instance.OnVictory();
            SetButtonsInteractable(false);
            return;
        }

        isEnemyTurn = true;

        // Disable buttons during enemy turn
        SetButtonsInteractable(false);

        StartCoroutine(ExecuteEnemyTurn());
    }

    private void ProcessPlayerTurnCombat()
    {
        if (lastSpinResult == null) return;
        
        List<Match> currentMatches = lastSpinResult.GetAllMatches();
        foreach (Match match in currentMatches)
        {
            ExecutePlayerCombat(match);
        }
    }

    private void ExecutePlayerCombat(Match match)
    {
        Deck playerDeck = DeckManager.instance.GetDeckByType(eDeckType.PLAYER);
        Deck enemyDeck  = DeckManager.instance.GetDeckByType(eDeckType.ENEMY);

        for (int i = 0; i < playerDeck.GetDeckMaxSize(); i++)
        {
            UnitObject unit = playerDeck.GetUnitObject(i);
            if (unit != null && unit.unitSO != null)
            {
                int targetIndex = CombatManager.instance.GetLowestHealth(enemyDeck);
                UnitObject target = enemyDeck.GetUnitObject(targetIndex);
                if (target != null)
                {
                    if (unit.unitSO.eUnitArchetype == match.GetArchetype())
                    {
                        match.SetUnitName(unit.unitSO.unitName);
                    }
                    CombatManager.instance.ExecBattle(eDeckType.PLAYER, i);
                }
            }
        }
    }

    private IEnumerator ExecuteEnemyTurn()
    {
        yield return new WaitForSeconds(1f);

        Deck enemyDeck = DeckManager.instance.GetDeckByType(eDeckType.ENEMY);
        SymbolType[] symbols = SymbolGenerator.instance.GenerateSymbolsForDeck(enemyDeck);

        SpinMode previousMode = spinMode;
        // enemy spin should not trigger player combat
        spinMode = SpinMode.PreviewOnly;

        if (enemyVisualSpin)
        {
            // run visual spin with preset symbols
            yield return SpinWithPresetSymbols(symbols);
        }
        else
        {
            //  process results immediately without visual spin
            ProcessSpinResults(symbols);
        }

        // restore original mode for subsequent player actions
        spinMode = previousMode;

        // Show pop-ups summarising enemy roll
        ShowEnemyRollPopups(enemyDeck);

        ProcessEnemyTurnCombat();

        yield return new WaitForSeconds(1f);

        isEnemyTurn = false;
        spinsThisTurn = 0;

        // Player turn back – enable buttons
        SetButtonsInteractable(true);
    }

    private void ProcessEnemyTurnCombat()
    {
        if (lastSpinResult == null) return;
        List<Match> matches = lastSpinResult.GetAllMatches();
        foreach (Match match in matches)
        {
            ExecuteEnemyCombat(match);
        }
    }

    private void ExecuteEnemyCombat(Match match)
    {
        Deck enemyDeck = DeckManager.instance.GetDeckByType(eDeckType.ENEMY);
        for (int i = 0; i < enemyDeck.GetDeckMaxSize(); i++)
        {
            UnitObject unit = enemyDeck.GetUnitObject(i);
            if (unit != null && unit.unitSO != null)
            {
                int targetIndex = CombatManager.instance.GetLowestHealth(DeckManager.instance.GetDeckByType(eDeckType.PLAYER));
                UnitObject target = DeckManager.instance.GetDeckByType(eDeckType.PLAYER).GetUnitObject(targetIndex);
                if (target != null)
                {
                    if (unit.unitSO.eUnitArchetype == match.GetArchetype())
                    {
                        match.SetUnitName(unit.unitSO.unitName);
                    }
                    CombatManager.instance.ExecBattle(eDeckType.ENEMY, i);
                }
            }
        }
    }

    private bool CheckAllEnemiesDead()
    {
        Deck enemyDeck = DeckManager.instance.GetDeckByType(eDeckType.ENEMY);
        for (int i = 0; i < enemyDeck.GetDeckMaxSize(); i++)
        {
            UnitObject unit = enemyDeck.GetUnitObject(i);
            if (unit != null && !unit.IsDead())
            {
                return false;
            }
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

    public void ExecuteFullCombat()
    {
        ExecuteCombat(true);
    }

    // UI helpers
    void ShowEnemyRollPopups(Deck enemyDeck)
    {
        if (UIPopUpManager.instance == null || lastSpinResult == null) return;

        foreach (Match m in lastSpinResult.GetAllMatches())
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

            string popupText = $"{m.GetMatchType()} {m.GetArchetype()}";
            if (iconSprite != null)
                UIPopUpManager.instance.CreatePopUp(popupText, iconSprite);
            else
                UIPopUpManager.instance.CreatePopUp(popupText);
        }
    }
} 