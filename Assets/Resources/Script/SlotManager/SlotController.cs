using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SlotController : MonoBehaviour
{
    public static SlotController instance { get; private set; }
    public static System.Action OnTurnChanged;
    public static System.Action OnMatchesProcessed;  

    // - Configuration
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
    private int spinsThisTurn;
    private bool isEnemyTurn;
    private bool isFirstRoll = true;
    
    // - Initialisation
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            transform.parent = null; 
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

    // - Public Functions
    public bool IsEnemyTurn()
    {
        return isEnemyTurn;
    }

    public void InitializeProbabilitiesIfNeeded()
    {
        if (isFirstRoll)
        {
            SymbolGenerator.instance.UpdateProbabilities();
            isFirstRoll = false;
        }
    }

    public void StartPlayerTurn()
    {
        Debug.Log("[SlotController] Starting player turn");
        isEnemyTurn = false;
        spinsThisTurn = 0;
        spinResult.Clear();
        
        Debug.Log("[SlotController] Adding base income for new turn");
        GoldManager.instance.OnRoundStart(false);

        OnTurnChanged?.Invoke();
    }

    public void EndPlayerTurn()
    {
        Debug.Log("=== PLAYER TURN COMBAT EXECUTION ===");

        ProcessPlayerTurnCombat();
        OnMatchesProcessed?.Invoke();
        GoldManager.instance.CalculateInterest();

        if (CheckAllEnemiesDead())
        {
            Debug.Log("[SlotController] All enemies dead, player wins!");
            GoldManager.instance.OnVictory();
            return;
        }

        isEnemyTurn = true;
        StartCoroutine(ExecuteEnemyTurn());
    }

    public bool GetIsSpinning()
    {
        return isSpinning;
    }

    public int GetSpinsThisTurn()
    {
        return spinsThisTurn;
    }

    public void ResetSpins()
    {
        spinsThisTurn = 0;
    }

    public void IncrementSpins()
    {
        spinsThisTurn++;
    }

    public bool HasFreeSpinAvailable()
    {
        return spinsThisTurn == 0;
    }

    public int GetCurrentSpinCost()
    {
        if (HasFreeSpinAvailable())
        {
            return 0;
        }
        return slotConfig.baseSpinCost + ((spinsThisTurn - 1) * 2);
    }

    public void FillGridWithRandomSymbols()
    {
        if (isSpinning || gridUI.GetIsSpinning())
        {
            return;
        }

        if (!TrySpendGoldForSpin())
        {
            return;
        }

        StartSpinSequence();
    }

    public bool CanSpin()
    {
        if (isSpinning)
        {
            return false;
        }

        if (!HasAlivePlayerUnits())
        {
            Debug.Log("[SlotController] Cannot spin - all player units are dead");
            return false;
        }
        
        return GoldManager.instance.HasEnoughGold(GetCurrentSpinCost());
    }

    public SpinResult GetSpinResult()
    {
        return spinResult;
    }

    public void ClearSpinResult()
    {
        spinResult.Clear();
    }

    // - Private Functions
    private void Update()
    {
        if (isEnemyTurn)
        {
            return;
        }

        bool hasAlivePlayerUnits = HasAlivePlayerUnits();
        bool hasAliveEnemyUnits = HasAliveEnemyUnits();

        UpdateButtonStates(hasAlivePlayerUnits, hasAliveEnemyUnits);
    }

    private void UpdateButtonStates(bool hasAlivePlayerUnits, bool hasAliveEnemyUnits)
    {
        if (rerollButton != null)
        {
            rerollButton.SetInteractable(hasAlivePlayerUnits);
        }

        if (attackButton != null)
        {
            attackButton.SetInteractable(hasAlivePlayerUnits && hasAliveEnemyUnits);
        }
    }

    private bool HasAlivePlayerUnits()
    {
        return HasAliveUnitsInDeck(eDeckType.PLAYER);
    }

    private bool HasAliveEnemyUnits()
    {
        return HasAliveUnitsInDeck(eDeckType.ENEMY);
    }

    private bool HasAliveUnitsInDeck(eDeckType deckType)
    {
        Deck deck = DeckManager.instance.GetDeckByType(deckType);
        for (int i = 0; i < deck.GetDeckMaxSize(); i++)
        {
            UnitObject unit = deck.GetUnitObject(i);
            if (unit != null && !unit.IsDead())
            {
                return true;
            }
        }
        return false;
    }

    private bool TrySpendGoldForSpin()
    {
        bool isFreeSpin = HasFreeSpinAvailable();
        if (!isFreeSpin)
        {
            int spinCost = GetCurrentSpinCost();
            if (!GoldManager.instance.SpendGold(spinCost))
            {
                Debug.Log("Cannot afford spin, cost: " + spinCost);
                return false;
            }
        }
        else
        {
            Debug.Log("Using free spin!");
        }
        return true;
    }

    private void StartSpinSequence()
    {
        isSpinning = true;
        if (rerollButton != null)
        {
            rerollButton.SetInteractable(false);
        }
        
        IncrementSpins();
        
        Deck currentDeck = DeckManager.instance.GetDeckByType(isEnemyTurn ? eDeckType.ENEMY : eDeckType.PLAYER);
        SymbolType[] finalSymbols = SymbolGenerator.instance.GenerateSymbolsForDeck(currentDeck);
        
        if (!ValidateSymbols(finalSymbols))
        {
            return;
        }
        
        gridUI.StartSpinAnimation(finalSymbols);
        FillGridWithSymbols(finalSymbols);
        
        StartCoroutine(WaitForSpinComplete());
    }

    private bool ValidateSymbols(SymbolType[] symbols)
    {
        if (symbols == null || symbols.Length != slotConfig.TotalGridSize)
        {
            Debug.LogError("Failed to generate symbols!");
            isSpinning = false;
            if (rerollButton != null)
            {
                rerollButton.SetInteractable(true);
            }
            return false;
        }
        return true;
    }

    private void ProcessPlayerTurnCombat()
    {
        List<Match> currentMatches = spinResult.GetAllMatches();
        if (currentMatches.Count > 0)
        {
            foreach (Match match in currentMatches)
            {
                eUnitArchetype matchArchetype = match.GetArchetype();
                MatchType matchType = match.GetMatchType();

                Debug.Log($"[SlotController] Processing match: {matchType} for archetype: {matchArchetype}");
                Debug.Log($"Processing match: Type={matchType}, Archetype={matchArchetype}");

                ExecutePlayerCombat(match);
            }
        }
        Debug.Log("=== END OF PLAYER COMBAT ===\n");
    }

    private void ExecutePlayerCombat(Match match)
    {
        Deck playerDeck = DeckManager.instance.GetDeckByType(eDeckType.PLAYER);
        Deck enemyDeck = DeckManager.instance.GetDeckByType(eDeckType.ENEMY);

        for (int i = 0; i < playerDeck.GetDeckMaxSize(); i++)
        {
            UnitObject unit = playerDeck.GetUnitObject(i);
            if (unit != null && unit.unitSO != null && 
                unit.unitSO.eUnitArchetype == match.GetArchetype())
            {
                int targetIndex = CombatManager.instance.GetLowestHealth(enemyDeck);
                UnitObject target = enemyDeck.GetUnitObject(targetIndex);
                
                if (target != null)
                {
                    Debug.Log($"Player unit {unit.unitSO.unitName} attacking target {target.unitSO.unitName}");
                    match.SetUnitName(unit.unitSO.unitName);
                    CombatManager.instance.ExecBattle(eDeckType.PLAYER, i);
                }
            }
        }
    }

    private bool CheckAllEnemiesDead()
    {
        var enemyDeckCheck = DeckManager.instance.GetDeckByType(eDeckType.ENEMY);
        for (int i = 0; i < enemyDeckCheck.GetDeckMaxSize(); i++)
        {
            UnitObject unit = enemyDeckCheck.GetUnitObject(i);
            if (unit != null && !unit.IsDead())
            {
                return false;
            }
        }
        return true;
    }

    private IEnumerator ExecuteEnemyTurn()
    {
        Debug.Log("=== ENEMY TURN COMBAT EXECUTION ===");
        yield return new WaitForSeconds(1f);
        
        Deck enemyDeck = DeckManager.instance.GetDeckByType(eDeckType.ENEMY);
        SymbolType[] finalSymbols = SymbolGenerator.instance.GenerateSymbolsForDeck(enemyDeck);
        
        isSpinning = true;
        gridUI.StartSpinAnimation(finalSymbols);
        FillGridWithSymbols(finalSymbols);
        
        while (gridUI.GetIsSpinning())
        {
            yield return null;
        }

        ProcessEnemyTurnCombat();

        yield return new WaitForSeconds(1f);
        StartPlayerTurn();
    }

    private void ProcessEnemyTurnCombat()
    {
        List<Match> matches = CheckForMatches();
        
        if (matches.Count > 0)
        {
            foreach (Match match in matches)
            {
                match.SetArchetype(SymbolGenerator.GetArchetypeForSymbol(match.GetSymbol()));
                MatchType matchType = match.GetMatchType();

                Debug.Log($"[SlotController] Enemy processing match: {matchType} for archetype: {match.GetArchetype()}");
                Debug.Log($"Enemy processing match: Type={matchType}, Archetype={match.GetArchetype()}");

                ExecuteEnemyCombat(match);
            }
        }
        Debug.Log("=== END OF ENEMY COMBAT ===\n");

        spinResult.SetMatches(matches, 0);
        isSpinning = false;

        OnMatchesProcessed?.Invoke();
    }

    private void ExecuteEnemyCombat(Match match)
    {
        Deck enemyDeck = DeckManager.instance.GetDeckByType(eDeckType.ENEMY);
        for (int i = 0; i < enemyDeck.GetDeckMaxSize(); i++)
        {
            UnitObject unit = enemyDeck.GetUnitObject(i);
            if (unit != null && unit.unitSO != null && 
                unit.unitSO.eUnitArchetype == match.GetArchetype())
            {
                Debug.Log($"[SlotController] Enemy unit {unit.unitSO.unitName} (index {i}) executing {match.GetMatchType()}");
                Debug.Log($"Enemy unit {unit.unitSO.unitName} executing {match.GetMatchType()} attack");
                match.SetUnitName(unit.unitSO.unitName);
                CombatManager.instance.ExecBattle(eDeckType.ENEMY, i);
            }
        }
    }

    private IEnumerator WaitForSpinComplete()
    {
        while (gridUI.GetIsSpinning())
        {
            yield return null;
        }

        ProcessSpinResults();
    }

    private void ProcessSpinResults()
    {
        List<Match> matches = CheckForMatches();
        
        int totalGold = 0;
        foreach (Match match in matches)
        {
            match.SetArchetype(SymbolGenerator.GetArchetypeForSymbol(match.GetSymbol()));

            if (match.GetMatchType() != MatchType.SINGLE)
            {
                int goldReward = GoldManager.instance.GetGoldRewardForMatch(match.GetMatchType());
                totalGold += goldReward;
                GoldManager.instance.AddGold(goldReward);
            }
        }

        spinResult.SetMatches(matches, totalGold);
        isSpinning = false;

        if (rerollButton != null)
        {
            rerollButton.SetInteractable(true);
        }
    }

    private void ClearGrid()
    {
        slotGrid.ClearGrid();
    }
    
    private List<Match> CheckForMatches()
    {
        return matchDetector.DetectMatches();
    }
    
    private void FillGridWithSymbols(SymbolType[] symbols)
    {
        if (symbols == null || symbols.Length != slotConfig.TotalGridSize)
        {
            return;
        }
        
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
} 