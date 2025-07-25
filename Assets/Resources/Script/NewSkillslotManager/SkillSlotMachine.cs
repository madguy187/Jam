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
    [Tooltip("Optional stagger between starting each column (seconds)")]
    [SerializeField] private float columnStartDelay = 0f;

    [Header("Spin Cost Settings")]
    [SerializeField] private int baseSpinCost = 2;

    private int spinsThisTurn = 0;

    private readonly List<SkillSlotGrid> columns = new List<SkillSlotGrid>();
    private SpinResult lastSpinResult;

    [Header("UI References")]
    [SerializeField] private UIRerollButton rerollButton;
    [SerializeField] private UIAttack       attackButton;

    // Turn or Combat thinGS
    private bool isEnemyTurn = false;
    public static bool IsEnemyTurnStatic => instance != null && instance.isEnemyTurn;


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
        if (leftColumn != null) columns.Add(leftColumn);
        if (centreColumn != null) columns.Add(centreColumn);
        if (rightColumn != null) columns.Add(rightColumn);

        // Auto-find buttons if not wired
        if (rerollButton == null) rerollButton = FindObjectOfType<UIRerollButton>();
        if (attackButton == null) attackButton = FindObjectOfType<UIAttack>();
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

        // Convert to SymbolType array (length 9) for MatchDetector / SlotController
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
        if (spinsThisTurn == 0) return 0;
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
        // Build a SlotGrid for detection
        SlotGrid grid = new SlotGrid(3,3);
        for (int i = 0; i < 9; i++)
        {
            int row = i / 3;
            int col = i % 3;
            grid.SetSlot(row, col, symbols[i]);
        }

        MatchDetector detector = new MatchDetector(grid);
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
            Debug.Log("=== MATCHES FOUND ===");
            foreach (Match m in matches)
            {
                string posStr = string.Join(", ", m.GetPositions().ConvertAll(PosName));
                Debug.Log($"{m.GetMatchType()} : {m.GetArchetype()} @ {posStr}");
            }
            Debug.Log("=====================");
        }

        // Re-enable buttons when player spin completed
        if (!isEnemyTurn)
        {
            SetButtonsInteractable(true);
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

        yield return SpinWithPresetSymbols(symbols);

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
} 