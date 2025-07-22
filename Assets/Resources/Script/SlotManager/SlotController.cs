using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SlotController : MonoBehaviour
{
    public static SlotController instance { get; private set; }
    
    public static System.Action OnTurnChanged;
    public static System.Action OnMatchesProcessed;  

    [Header("Configuration")]
    [SerializeField] private SlotConfig slotConfig;

    [Header("References")]
    [SerializeField] private SlotGridUI gridUI;
    [SerializeField] private UIRerollButton rerollButton;  
    [SerializeField] private UIAttack attackButton;  
    
    private SlotGrid slotGrid;
    private MatchDetector matchDetector;
    private bool isSpinning;
    private SpinResult spinResult;
    private int spinsThisTurn = 0;
    private bool isEnemyTurn = false;
    private bool isFirstRoll = true;
    
    public bool IsEnemyTurn() => isEnemyTurn;

    public void InitializeProbabilitiesIfNeeded()
    {
        if (isFirstRoll)
        {
            SymbolGenerator.Instance.UpdateProbabilities();
            isFirstRoll = false;
        }
    }

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

        if (rerollButton == null)
        {
            Debug.LogError("[SlotController] Reroll button reference is missing!");
        }

        if (attackButton == null)
        {
            Debug.LogError("[SlotController] Attack button reference is missing!");
        }
    }

    private void InitializeComponents()
    {
        slotGrid = new SlotGrid(slotConfig.gridRows, slotConfig.gridColumns);
        matchDetector = new MatchDetector(slotGrid);
        spinResult = new SpinResult(new List<Match>(), 0);
    }

    private void Update()
    {
        // Don't check during enemy turn
        if (isEnemyTurn) return;

        // Check player units state
        Deck playerDeck = DeckManager.instance.GetDeckByType(eDeckType.PLAYER);
        bool hasAlivePlayerUnits = false;

        for (int i = 0; i < playerDeck.GetDeckMaxSize(); i++)
        {
            UnitObject unit = playerDeck.GetUnitObject(i);
            if (unit != null && !unit.IsDead())
            {
                hasAlivePlayerUnits = true;
                break;
            }
        }

        // Check enemy units state
        Deck enemyDeck = DeckManager.instance.GetDeckByType(eDeckType.ENEMY);
        bool hasAliveEnemyUnits = false;

        for (int i = 0; i < enemyDeck.GetDeckMaxSize(); i++)
        {
            UnitObject unit = enemyDeck.GetUnitObject(i);
            if (unit != null && !unit.IsDead())
            {
                hasAliveEnemyUnits = true;
                break;
            }
        }

        // Update button states
        if (rerollButton != null)
        {
            rerollButton.SetInteractable(hasAlivePlayerUnits);
        }

        if (attackButton != null)
        {
            // Only enable attack if both players and enemies are alive
            attackButton.SetInteractable(hasAlivePlayerUnits && hasAliveEnemyUnits);
        }
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

        // Notify turn changed
        OnTurnChanged?.Invoke();
    }

    public void EndPlayerTurn()
    {
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

        // Notify that matches have been processed
        OnMatchesProcessed?.Invoke();

        // Calculate interest at end of player's turn
        GoldManager.instance.CalculateInterest();

        // Check if all enemies are dead before transitioning to enemy turn
        var enemyDeckCheck = DeckManager.instance.GetDeckByType(eDeckType.ENEMY);
        bool allEnemiesDead = true;

        for (int i = 0; i < enemyDeckCheck.GetDeckMaxSize(); i++)
        {
            UnitObject unit = enemyDeckCheck.GetUnitObject(i);
            if (unit != null && !unit.IsDead()) 
            {
                allEnemiesDead = false;
                break;
            }
        }

        if (allEnemiesDead)
        {
            Debug.Log("[SlotController] All enemy units are dead after combat, starting new player turn");
            StartPlayerTurn();
            return;
        }

        StartEnemyTurn();
    }

    public void StartEnemyTurn()
    {
        // Check if all enemy units are dead
        Deck enemyDeck = DeckManager.instance.GetDeckByType(eDeckType.ENEMY);
        bool allEnemiesDead = true;

        for (int i = 0; i < enemyDeck.GetDeckMaxSize(); i++)
        {
            UnitObject unit = enemyDeck.GetUnitObject(i);
            if (unit != null && !unit.IsDead())  
            {
                allEnemiesDead = false;
                break;
            }
        }

        // If all enemies are dead, don't start enemy turn
        if (allEnemiesDead)
        {
            Debug.Log("[SlotController] All enemy units are dead, skipping enemy turn");
            StartPlayerTurn();
            return;
        }

        isEnemyTurn = true;
        spinsThisTurn = 0;
        spinResult.Clear();

        // Disable buttons during enemy turn
        if (rerollButton != null) rerollButton.SetInteractable(false);  
        if (attackButton != null) attackButton.SetInteractable(false);  // Changed from endTurnButton

        // Notify turn changed
        OnTurnChanged?.Invoke();

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
        SymbolType[] finalSymbols = SymbolGenerator.Instance.GenerateSymbolsForDeck(enemyDeck);
        
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
                // Set archetype for each match based on symbol
                eUnitArchetype matchArchetype = SymbolGenerator.GetArchetypeForSymbol(match.GetSymbol());
                match.SetArchetype(matchArchetype);

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

        // Notify that matches have been processed
        OnMatchesProcessed?.Invoke();

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
        // Disable reroll button during spin
        if (rerollButton != null)  
        {
            rerollButton.SetInteractable(false);  
        }
        
        IncrementSpins();
        
        // Get current deck's archetypes
        Deck currentDeck = DeckManager.instance.GetDeckByType(isEnemyTurn ? eDeckType.ENEMY : eDeckType.PLAYER);
        SymbolType[] finalSymbols = SymbolGenerator.Instance.GenerateSymbolsForDeck(currentDeck);
        
        if (finalSymbols == null || finalSymbols.Length != slotConfig.TotalGridSize)
        {
            Debug.LogError("Failed to generate symbols!");
            isSpinning = false;
            // Re-enable reroll button if spin fails
            if (rerollButton != null)  
            {
                rerollButton.SetInteractable(true);  
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

        // Re-enable reroll button after spin completes
        if (rerollButton != null)  
        {
            rerollButton.SetInteractable(true);  
        }
    }

    public bool CanSpin()
    {
        if (isSpinning) 
        {
            return false;
        }

        // Check if all player units are dead
        Deck playerDeck = DeckManager.instance.GetDeckByType(eDeckType.PLAYER);
        bool hasAliveUnits = false;

        for (int i = 0; i < playerDeck.GetDeckMaxSize(); i++)
        {
            UnitObject unit = playerDeck.GetUnitObject(i);
            if (unit != null && !unit.IsDead())
            {
                hasAliveUnits = true;
                break;
            }
        }

        if (!hasAliveUnits)
        {
            Debug.Log("[SlotController] Cannot spin - all player units are dead");
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