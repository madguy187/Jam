using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/* own notes
Higher Min/Preferred Height = moves content up
Lower Min/Preferred Height = moves content down
Higher Min/Preferred Width = makes wider
Lower Min/Preferred Width = makes narrower
*/

public class SlotController : MonoBehaviour
{
    public static SlotController instance { get; private set; }
    
    [Header("Configuration")]
    [SerializeField] private SlotConfig slotConfig;
    [SerializeField] private bool isCombatEnabled = true;

    [Header("References")]
    [SerializeField] private SlotGridUI gridUI; 
    
    private SlotGrid slotGrid;
    private MatchDetector matchDetector;
    private int spinsThisTurn = 0;
    private bool isSpinning;
    private SpinResult spinResult;
    private int currentSpinCost;
    
    [Header("Turn Settings")]
    [SerializeField] private float enemyTurnDelay = 1f;
    [SerializeField] private float enemySpinDuration = 0.5f;
    private bool isEnemyTurn = false;
    
    public bool GetIsSpinning() {
        return isSpinning;
    }
    
    public int GetCurrentSpinCost() {
        return GetNextSpinCost();
    }
    
    public bool GetHasFreeSpinsRemaining() {
        return spinsThisTurn < slotConfig.freeSpinsPerTurn;
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        if (slotConfig == null)
        {
            Debug.LogWarning("[SlotController] Slot configuration is missing");
            return;
        }

        if (gridUI == null)
        {
            Debug.LogWarning("[SlotController] SlotGridUI reference is missing");
            return;
        }
        
        slotGrid = new SlotGrid(slotConfig.gridRows, slotConfig.gridColumns);
        matchDetector = new MatchDetector(slotGrid);
        spinResult = new SpinResult(new List<Match>(), 0);
    }
    
    public void FillGridWithRandomSymbols(bool autoSpendGold = true)
    {
        if (isSpinning) 
        {
            return;
        }

        if (gridUI.GetIsSpinning())
        {
            return;
        }
        
        isSpinning = true;
        
        // Check if we need to spend gold
        if (spinsThisTurn >= slotConfig.freeSpinsPerTurn)
        {
            currentSpinCost = GetNextSpinCost();
            if (!GoldManager.instance.SpendGold(currentSpinCost))
            {
                isSpinning = false;
                return;
            }
        }
        else
        {
            currentSpinCost = 0;
        }
        
        spinsThisTurn++;
        
        // Generate random symbols
        SymbolType[] finalSymbols = new SymbolType[slotConfig.TotalGridSize];
        for (int i = 0; i < slotConfig.TotalGridSize; i++)
        {
            finalSymbols[i] = SymbolGenerator.instance.GenerateRandomSymbol();
        }
        
        if (finalSymbols == null || finalSymbols.Length != slotConfig.TotalGridSize)
        {
            isSpinning = false;
            return;
        }
        
        // Start the spin animation and fill the grid
        gridUI.StartSpinAnimation(finalSymbols);
        FillGridWithSymbols(finalSymbols);
        
        StartCoroutine(WaitForSpinComplete(autoSpendGold, false));
    }
    
    private void ProcessMatchesForUnit(List<Match> matches, bool isEnemyAttack)
    {
        if (matches.Count <= 0) return;

        if (!isCombatEnabled) {
            Global.DEBUG_PRINT("[SlotController] Combat is disabled");
            return;
        }

        // Get unit from correct deck
        eDeckType attackerDeck = isEnemyAttack ? eDeckType.ENEMY : eDeckType.PLAYER;
        Deck deck = DeckManager.instance.GetDeckByType(attackerDeck);
        
        for (int i = 0; i < deck.GetDeckMaxSize(); i++)
        {
            var unit = deck.GetUnitObject(i);
            if (unit != null && unit.unitSO != null)
            {
                // 1. Fill up List<Match> with unit name
                string unitName = unit.unitSO.unitName;
                foreach (var match in matches)
                {
                    match.SetUnitName(unitName);
                }

                // 2. Then call CombatManager ExecBattle
                CombatManager.instance.ExecBattle(attackerDeck, i);
                break;
            }
        }
    }

    private IEnumerator WaitForSpinComplete(bool autoSpendGold, bool isEnemyAttack)
    {
        // Wait for spin animation to complete
        while (gridUI.GetIsSpinning())
        {
            yield return null;
        }

        // After grid is filled and animation is complete, check for matches
        List<Match> matches = CheckForMatches();
        
        // Calculate total gold earned (only for player)
        int totalGold = 0;
        if (!isEnemyAttack && autoSpendGold && matches.Count > 0)
        {
            foreach (Match match in matches)
            {
                if (match.GetMatchType() != MatchType.SINGLE)
                {
                    int goldReward = GoldManager.instance.GetGoldRewardForMatch(match.GetMatchType());
                    totalGold += goldReward;
                    GoldManager.instance.AddGold(goldReward);
                }
            }
        }

        // Process matches and start combat if needed
        if (matches.Count > 0)
        {
            ProcessMatchesForUnit(matches, isEnemyAttack);
            yield return new WaitForSeconds(0.5f); // Small delay for combat effects
        }
        
        // Save matches to our SpinResult
        spinResult.SetMatches(matches, totalGold);
        isSpinning = false;
        
        // Switch turns
        if (!isEnemyAttack)
        {
            StartEnemyTurn();
        }
        else
        {
            StartPlayerTurn();
        }
    }

    private void PrintSpinResults()
    {
        Global.DEBUG_PRINT("=== SPIN RESULT ===");
        
        List<Match> allMatches = spinResult.GetAllMatches();
        if (allMatches.Count == 0)
        {
            Global.DEBUG_PRINT("No matches found!");
        }
        else 
        {
            // Group matches by type
            var matchesByType = allMatches.GroupBy(m => m.GetMatchType());
            
            foreach (var group in matchesByType)
            {
                Global.DEBUG_PRINT($"\n{group.Key} Matches:");
                foreach (var match in group)
                {
                    Global.DEBUG_PRINT($"  - {match.GetSymbol()} at positions: {string.Join(", ", match.GetReadablePositions())}");
                }
            }
        }
        
        Global.DEBUG_PRINT("==================");
    }
    
    public void SpinForUnit(UnitObject unit)
    {
        if (unit == null)
        {
            return;
        }
        
        if (gridUI.GetIsSpinning() || isSpinning)
        {
            return;
        } 
        
        isSpinning = true;

        SymbolType[] finalSymbols = SymbolGenerator.instance.GenerateSymbolsForUnit(unit);
        if (finalSymbols == null || finalSymbols.Length != 9)
        {
            isSpinning = false;
            return;
        }
        
        gridUI.StartSpinAnimation(finalSymbols);
        FillGridWithSymbols(finalSymbols);
        
        StartCoroutine(WaitForSpinComplete(true, false));
    }
    
    public SpinResult GetSpinResult()
    {
        return spinResult;
    }

    public bool CanSpin()
    {
        if (isSpinning) 
        {
            return false;
        }
        
        if (spinsThisTurn < slotConfig.freeSpinsPerTurn) 
        {
            return true;
        }

        return GoldManager.instance.HasEnoughGold(GetNextSpinCost());
    }
    
    public int GetNextSpinCost()
    {
        if (spinsThisTurn < slotConfig.freeSpinsPerTurn)
        {
            return 0;
        }

        return slotConfig.baseSpinCost * (spinsThisTurn - slotConfig.freeSpinsPerTurn + 1);
    }
    
    public int GetSpinsThisTurn()
    {
        return spinsThisTurn;
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

    public bool GetHasActiveSpinResult() 
    {
        return spinResult != null;
    }
    
    public void ClearSpinResult()
    {
        spinResult.Clear();
    }

    // ===== ADD SOME TURN MANAGEMENT FUNCTIONS HERE FOR BATTLE ======
    public void StartNewTurn()
    {
        spinsThisTurn = 0;
        spinResult.Clear();
    }
    
    public void StartPlayerTurn()
    {
        isEnemyTurn = false;
        spinsThisTurn = 0;
        spinResult.Clear();
    }
    
    public void StartEnemyTurn()
    {
        isEnemyTurn = true;
        spinsThisTurn = 0;
        spinResult.Clear();
        StartCoroutine(ExecuteEnemyTurn());
    }
    
    private IEnumerator ExecuteEnemyTurn()
    {
        yield return new WaitForSeconds(enemyTurnDelay);
        
        if (!isCombatEnabled) yield break;
        
        isSpinning = true;
        
        // Generate random symbols for enemy
        SymbolType[] finalSymbols = new SymbolType[slotConfig.TotalGridSize];
        for (int i = 0; i < slotConfig.TotalGridSize; i++)
        {
            finalSymbols[i] = SymbolGenerator.instance.GenerateRandomSymbol();
        }
        
        // Start spin with enemy duration
        gridUI.StartSpinAnimation(finalSymbols, gridUI.GetEnemySpinDuration());
        FillGridWithSymbols(finalSymbols);
        
        // autoSpendGold=false, isEnemyTurn=true
        StartCoroutine(WaitForSpinComplete(false, true)); 
    }
} 