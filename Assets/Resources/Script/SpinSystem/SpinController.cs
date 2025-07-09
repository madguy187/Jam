using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SpinController : MonoBehaviour
{
    public static SpinController instance;
    
    private SlotGrid slotGrid;
    private MatchDetector matchDetector;
    private int spinsThisTurn = 0;
    private const int FREE_SPINS_PER_TURN = 1;
    private const int BASE_SPIN_COST = 2;
    private bool isSpinning;
    private SpinResult spinResult;
    private int currentSpinCost;
    
    public bool IsSpinning => isSpinning;
    public int CurrentSpinCost => GetNextSpinCost();
    public bool HasFreeSpinsRemaining => spinsThisTurn < FREE_SPINS_PER_TURN;
    
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
        
        // Create new instances since these are regular classes, not MonoBehaviours
        slotGrid = new SlotGrid();
        matchDetector = new MatchDetector(slotGrid);
        spinResult = new SpinResult(new List<Match>(), 0);  // Initialize with empty matches
    }
    
    public void FillGridWithRandomSymbols(bool autoSpendGold = true)
    {
        if (isSpinning) 
        {
            return;
        }

        SlotGridUI gridUI = FindObjectOfType<SlotGridUI>();
        if (gridUI == null || gridUI.IsSpinning())
        {
            return;
        }
        
        isSpinning = true;
        
        // Check if we need to spend gold
        if (spinsThisTurn >= FREE_SPINS_PER_TURN)
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
        SymbolType[] finalSymbols = new SymbolType[9];
        for (int i = 0; i < 9; i++)
        {
            finalSymbols[i] = SymbolGenerator.instance.GenerateRandomSymbol();
        }
        
        if (finalSymbols == null || finalSymbols.Length != 9)
        {
            isSpinning = false;
            return;
        }
        
        // Start the spin animation and fill the grid
        gridUI.StartSpinAnimation(finalSymbols);
        FillGridWithSymbols(finalSymbols);
        
        StartCoroutine(WaitForSpinComplete(gridUI, autoSpendGold));
    }
    
    private IEnumerator WaitForSpinComplete(SlotGridUI gridUI, bool autoSpendGold)
    {
        // Wait for spin animation to complete
        while (gridUI.IsSpinning())
        {
            yield return null;
        }

        // After grid is filled and animation is complete, check for matches
        List<Match> matches = CheckForMatches();
        
        // Calculate total gold earned
        int totalGold = 0;
        if (autoSpendGold && matches.Count > 0)
        {
            foreach (Match match in matches)
            {
                if (match.Type != MatchType.SINGLE)
                {
                    int goldReward = GetGoldRewardForMatch(match.Type);
                    totalGold += goldReward;
                    GoldManager.instance.AddGold(goldReward);
                }
            }
        }
        
        // Save matches to our SpinResult
        spinResult.SetMatches(matches, totalGold);

        // for demo purposes
        PrintSpinResults();
        
        isSpinning = false;
    }

    private void PrintSpinResults()
    {
        Global.DEBUG_PRINT("=== SPIN RESULT ===");
        
        // Print exactly what GetAllMatches() returns
        List<Match> allMatches = spinResult.GetAllMatches();
        if (allMatches.Count == 0)
        {
            Global.DEBUG_PRINT("No matches found!");
        }
        else 
        {
            foreach (Match match in allMatches)
            {
                string positions = string.Join(", ", match.Positions.Select(p => $"({p.x},{p.y})"));
                Global.DEBUG_PRINT($"Match: Type={match.Type}, Symbol={match.Symbol}, Positions={positions}");
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
        
        SlotGridUI gridUI = FindObjectOfType<SlotGridUI>();
        if (gridUI == null || gridUI.IsSpinning() || isSpinning)
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
        
        // Use the same pattern as FillGridWithRandomSymbols
        StartCoroutine(WaitForSpinComplete(gridUI, true));
    }
    
    public SpinResult GetSpinResult()
    {
        return spinResult;
    }
    
    public void StartNewTurn()
    {
        spinsThisTurn = 0;
        spinResult.Clear();
    }
    
    public bool CanSpin()
    {
        if (isSpinning) 
        {
            return false;
        }
        
        if (spinsThisTurn < FREE_SPINS_PER_TURN) 
        {
            return true;
        }

        return GoldManager.instance.HasEnoughGold(GetNextSpinCost());
    }
    
    public int GetNextSpinCost()
    {
        if (spinsThisTurn < FREE_SPINS_PER_TURN)
        {
            return 0;
        }

        return BASE_SPIN_COST * (spinsThisTurn - FREE_SPINS_PER_TURN + 1);
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
        List<Match> matches = matchDetector.DetectAllMatches();
        
        // Debugging function , can comment out
        /*Global.DEBUG_PRINT("=== Match Types Found ===");
        if (matches.Count > 0)
        {
            foreach (Match match in matches)
            {
                Global.DEBUG_PRINT($"Match: {match.Type} with symbol: {match.Symbol}");
            }
        }
        else
        {
            Global.DEBUG_PRINT("No matches");
        }
        Global.DEBUG_PRINT("=====================");
        */
        
        return matches;
    }
    
    private void FillGridWithSymbols(SymbolType[] symbols)
    {
        if (symbols == null || symbols.Length != 9) return;
        
        slotGrid.ClearGrid();
        for (int row = 0; row < 3; row++)
        {
            for (int col = 0; col < 3; col++)
            {
                int index = row * 3 + col;
                slotGrid.SetSlot(row, col, symbols[index]);
            }
        }
    }
    
    private string GetMatchDescription(MatchType type)
    {
        switch(type) {
            case MatchType.SINGLE:
                return "Single Symbol";
            case MatchType.HORIZONTAL:
                return "Row Match";
            case MatchType.DIAGONAL:
                return "Diagonal Match";
            case MatchType.XSHAPE:
                return "X-Shape Match";
            case MatchType.FULLGRID:
                return "Full Grid Match";
            case MatchType.ZIGZAG:
                return "Zigzag Match";
            default:
                return "Unknown Match";
        }
    }
    
    private int GetGoldRewardForMatch(MatchType type)
    {
        switch(type)
        {
            case MatchType.SINGLE:
                return 0;  // Single matches don't give gold rewards
            case MatchType.HORIZONTAL:
                return GoldConstants.HORIZONTAL_REWARD;
            case MatchType.DIAGONAL:
                return GoldConstants.DIAGONAL_REWARD;
            case MatchType.ZIGZAG:
                return GoldConstants.ZIGZAG_REWARD;
            case MatchType.XSHAPE:
                return GoldConstants.XSHAPE_REWARD;
            case MatchType.FULLGRID:
                return GoldConstants.FULL_GRID_REWARD;
            default:
                return 0;
        }
    }

    // Helper method to get current spin state
    public bool HasActiveSpinResult() => spinResult != null;
    
    // Clear spin result (e.g., when starting new turn)
    public void ClearSpinResult()
    {
        spinResult.Clear();
    }
} 