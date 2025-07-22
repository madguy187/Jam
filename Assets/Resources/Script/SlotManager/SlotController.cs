using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SlotController : MonoBehaviour
{
    public static SlotController instance { get; private set; }
    
    [Header("Configuration")]
    [SerializeField] private SlotConfig slotConfig;

    [Header("References")]
    [SerializeField] private SlotGridUI gridUI;
    [SerializeField] private UIRollButton rollButton;
    [SerializeField] private UIEndTurnButton endTurnButton;
    
    private SlotGrid slotGrid;
    private MatchDetector matchDetector;
    private bool isSpinning;
    private SpinResult spinResult;
    private int spinsThisTurn = 0;
    private bool isEnemyTurn = false;
    
    public bool IsEnemyTurn() => isEnemyTurn;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSlotController();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeSlotController()
    {
        ValidateReferences();
        InitializeComponents();
        StartPlayerTurn();
    }

    private void ValidateReferences()
    {
        if (slotConfig == null)
        {
            Debug.LogError("[SlotController] Slot configuration is missing!");
            return;
        }

        if (gridUI == null)
        {
            Debug.LogError("[SlotController] SlotGridUI reference is missing!");
            return;
        }

        if (rollButton == null)
        {
            Debug.LogError("[SlotController] Roll button reference is missing!");
        }

        if (endTurnButton == null)
        {
            Debug.LogError("[SlotController] End turn button reference is missing!");
        }
    }

    private void InitializeComponents()
    {
        slotGrid = new SlotGrid(slotConfig.gridRows, slotConfig.gridColumns);
        matchDetector = new MatchDetector(slotGrid);
        spinResult = new SpinResult(new List<Match>(), 0);
    }

    public void StartPlayerTurn()
    {
        Debug.Log("[SlotController] Starting player turn");
        isEnemyTurn = false;
        spinsThisTurn = 0;
        spinResult.Clear();
        
        // Add base income at start of turn
        Debug.Log("[SlotController] Adding base income for new turn");
        GoldManager.instance.OnRoundStart(false);
        
        // Enable buttons for player turn
        if (rollButton != null) rollButton.SetInteractable(true);
        if (endTurnButton != null) endTurnButton.SetInteractable(true);
    }

    public void EndPlayerTurn()
    {
        Debug.Log("[SlotController] EndPlayerTurn called");
        Debug.Log("=== PLAYER TURN COMBAT EXECUTION ===");

        // Execute combat with current matches
        List<Match> currentMatches = spinResult.GetAllMatches();
        if (currentMatches.Count > 0)
        {
            // For each match
            foreach (Match match in currentMatches)
            {
                eUnitArchetype matchArchetype = match.GetArchetype();
                MatchType matchType = match.GetMatchType();

                Debug.Log($"[SlotController] Processing match: {matchType} for archetype: {matchArchetype}");
                Debug.Log($"Processing match: Type={matchType}, Archetype={matchArchetype}");

                // Get current deck
                Deck playerDeck = DeckManager.instance.GetDeckByType(eDeckType.PLAYER);
                Deck enemyDeck = DeckManager.instance.GetDeckByType(eDeckType.ENEMY);

                // Find all units of matching archetype
                for (int i = 0; i < playerDeck.GetDeckMaxSize(); i++)
                {
                    UnitObject unit = playerDeck.GetUnitObject(i);
                    if (unit != null && unit.unitSO != null)
                    {
                        // Find target
                        int targetIndex = CombatManager.instance.GetLowestHealth(enemyDeck);
                        UnitObject target = enemyDeck.GetUnitObject(targetIndex);
                        
                        if (target != null)
                        {
                            Debug.Log($"Player unit {unit.unitSO.unitName} attacking target {target.unitSO.unitName}");
                            // Set match info before executing battle
                            match.SetUnitName(unit.unitSO.unitName);
                            // Execute battle for this unit
                            CombatManager.instance.ExecBattle(eDeckType.PLAYER, i);
                        }
                    }
                }
            }
        }
        Debug.Log("=== END OF PLAYER COMBAT ===\n");

        // Calculate interest at end of player's turn
        GoldManager.instance.CalculateInterest();
        StartEnemyTurn();
    }

    public void StartEnemyTurn()
    {
        isEnemyTurn = true;
        spinsThisTurn = 0;
        spinResult.Clear();

        // Disable buttons during enemy turn
        if (rollButton != null) rollButton.SetInteractable(false);
        if (endTurnButton != null) endTurnButton.SetInteractable(false);

        // Execute enemy's single spin
        StartCoroutine(ExecuteEnemyTurn());
    }

    private IEnumerator ExecuteEnemyTurn()
    {
        Debug.Log("=== ENEMY TURN COMBAT EXECUTION ===");
        // Small delay before enemy acts
        yield return new WaitForSeconds(1f); 
        
        // Generate symbols based on enemy deck archetypes
        Deck enemyDeck = DeckManager.instance.GetDeckByType(eDeckType.ENEMY);
        SymbolType[] finalSymbols = SymbolGenerator.instance.GenerateSymbolsForDeck(enemyDeck);
        
        isSpinning = true;
        gridUI.StartSpinAnimation(finalSymbols);
        FillGridWithSymbols(finalSymbols);
        
        // Wait for spin animation
        while (gridUI.GetIsSpinning())
        {
            yield return null;
        }

        // Check for matches
        List<Match> matches = CheckForMatches();
        
        // Execute combat with matches
        if (matches.Count > 0)
        {
            // For each match
            foreach (Match match in matches)
            {
                eUnitArchetype matchArchetype = match.GetArchetype();
                MatchType matchType = match.GetMatchType();

                Debug.Log($"[SlotController] Enemy processing match: {matchType} for archetype: {matchArchetype}");
                Debug.Log($"Enemy processing match: Type={matchType}, Archetype={matchArchetype}");

                // Find all enemy units of matching archetype
                for (int i = 0; i < enemyDeck.GetDeckMaxSize(); i++)
                {
                    UnitObject unit = enemyDeck.GetUnitObject(i);
                    if (unit != null && unit.unitSO != null && 
                        unit.unitSO.eUnitArchetype == matchArchetype)
                    {
                        Debug.Log($"[SlotController] Enemy unit {unit.unitSO.unitName} (index {i}) executing {matchType}");
                        Debug.Log($"Enemy unit {unit.unitSO.unitName} executing {matchType} attack");
                        // Set match info before executing battle
                        match.SetUnitName(unit.unitSO.unitName);
                        // Execute battle for this unit
                        CombatManager.instance.ExecBattle(eDeckType.ENEMY, i);
                    }
                }
            }
        }
        Debug.Log("=== END OF ENEMY COMBAT ===\n");

        spinResult.SetMatches(matches, 0); // Enemy doesn't earn gold
        isSpinning = false;

        yield return new WaitForSeconds(1f); 
        StartPlayerTurn();
    }
    
    public bool GetIsSpinning() {
        return isSpinning;
    }

    public int GetSpinsThisTurn() {
        return spinsThisTurn;
    }

    public void ResetSpins() {
        spinsThisTurn = 0;
    }

    public void IncrementSpins() {
        spinsThisTurn++;
    }

    public bool HasFreeSpinAvailable() {
        return spinsThisTurn == 0;
    }

    public int GetCurrentSpinCost()
    {
        if (HasFreeSpinAvailable()) {
            return 0;
        }
        return slotConfig.baseSpinCost + ((spinsThisTurn - 1) * 2);
    }
    
    public void FillGridWithRandomSymbols()
    {
        if (isSpinning) 
        {
            return;
        }

        if (gridUI.GetIsSpinning())
        {
            return;
        }
        
        // Check if this is a free spin or if we need to spend gold
        bool isFreeSpin = HasFreeSpinAvailable();
        if (!isFreeSpin)
        {
            // Only try to spend gold if it's not a free spin
            int spinCost = GetCurrentSpinCost();
            if (!GoldManager.instance.SpendGold(spinCost))
            {
                Debug.Log("Cannot afford spin, cost: " + spinCost);
                return;
            }
        }
        else
        {
            Debug.Log("Using free spin!");
        }

        isSpinning = true;
        // Disable roll button during spin
        if (rollButton != null)
        {
            rollButton.SetInteractable(false);
        }
        
        IncrementSpins();
        
        // Get current deck's archetypes
        Deck currentDeck = DeckManager.instance.GetDeckByType(isEnemyTurn ? eDeckType.ENEMY : eDeckType.PLAYER);
        SymbolType[] finalSymbols = SymbolGenerator.instance.GenerateSymbolsForDeck(currentDeck);
        
        if (finalSymbols == null || finalSymbols.Length != slotConfig.TotalGridSize)
        {
            Debug.LogError("Failed to generate symbols!");
            isSpinning = false;
            // Re-enable roll button if spin fails
            if (rollButton != null)
            {
                rollButton.SetInteractable(true);
            }
            return;
        }
        
        // Start the spin animation and fill the grid
        gridUI.StartSpinAnimation(finalSymbols);
        FillGridWithSymbols(finalSymbols);
        
        StartCoroutine(WaitForSpinComplete());
    }

    private IEnumerator WaitForSpinComplete()
    {
        // Wait for spin animation to complete
        while (gridUI.GetIsSpinning())
        {
            yield return null;
        }

        // After grid is filled and animation is complete, check for matches
        List<Match> matches = CheckForMatches();
        
        // Calculate total gold earned
        int totalGold = 0;
        foreach (Match match in matches)
        {
            // Set archetype for each match based on symbol
            match.SetArchetype(SymbolGenerator.GetArchetypeForSymbol(match.GetSymbol()));

            if (match.GetMatchType() != MatchType.SINGLE)
            {
                int goldReward = GoldManager.instance.GetGoldRewardForMatch(match.GetMatchType());
                totalGold += goldReward;
                GoldManager.instance.AddGold(goldReward);
            }
        }

        // Save matches to our SpinResult
        spinResult.SetMatches(matches, totalGold);
        isSpinning = false;

        // Re-enable roll button after spin completes
        if (rollButton != null)
        {
            rollButton.SetInteractable(true);
        }
    }

    public bool CanSpin()
    {
        if (isSpinning) 
        {
            return false;
        }
        
        return GoldManager.instance.HasEnoughGold(GetCurrentSpinCost());
    }
    
    private void ClearGrid()
    {
        slotGrid.ClearGrid();
    }
    
    private List<Match> CheckForMatches()
    {
        List<Match> matches = matchDetector.DetectMatches();
        return matches;
    }
    
    private void FillGridWithSymbols(SymbolType[] symbols)
    {
        if (symbols == null || symbols.Length != slotConfig.TotalGridSize) return;
        
        slotGrid.ClearGrid();
        for (int row = 0; row < slotConfig.gridRows; row++)
        {
            for (int col = 0; col < slotConfig.gridColumns; col++)
            {
                int index = row * slotConfig.gridColumns + col;
                slotGrid.SetSlot(row, col, symbols[index]);
            }
        }
    }

    public SpinResult GetSpinResult()
    {
        return spinResult;
    }

    public void ClearSpinResult()
    {
        spinResult.Clear();
    }
} 