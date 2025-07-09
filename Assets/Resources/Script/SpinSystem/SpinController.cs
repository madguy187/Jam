using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinController : MonoBehaviour
{
    public static SpinController instance;
    private SlotGrid slotGrid;
    private MatchDetector matchDetector;
    private int spinsThisTurn = 0;
    private const int FREE_SPINS_PER_TURN = 1;
    private const int BASE_SPIN_COST = 2;
    private List<Match> lastMatches;
    private int lastSpinCost;
    private bool isSpinning;
    
    public bool IsSpinning => isSpinning;
    public int CurrentSpinCost => GetNextSpinCost();
    public bool HasFreeSpinsRemaining => spinsThisTurn < FREE_SPINS_PER_TURN;
    
    /* ====== NOTESSSSS ======
    - First spin each turn is free
    - Additional spins cost gold (increases with each spin)
    - Matches trigger rewards 
    - Different match types have different rewards
    */
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        
        slotGrid = new SlotGrid();
        matchDetector = new MatchDetector(slotGrid);
    }
    
    public void FillGridWithRandomSymbols(bool autoSpendGold = true)
    {
        if (isSpinning) return;
        
        SlotGridUI gridUI = FindObjectOfType<SlotGridUI>();
        if (gridUI == null || gridUI.IsSpinning())
        {
            return;
        }
        
        isSpinning = true;
        
        // Check if we need to spend gold
        if (spinsThisTurn >= FREE_SPINS_PER_TURN)
        {
            lastSpinCost = GetNextSpinCost();
            if (!GoldManager.instance.SpendGold(lastSpinCost))
            {
                isSpinning = false;
                return;
            }
        }
        else
        {
            lastSpinCost = 0;
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
            Global.DEBUG_PRINT("Failed to generate symbols!");
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
        lastMatches = CheckForMatches();
        
        if (autoSpendGold && lastMatches.Count > 0)
        {
            foreach (Match match in lastMatches)
            {
                // Skip adding gold for single matches
                if (match.Type != MatchType.SINGLE)
                {
                    GoldManager.instance.AddGold(GetGoldRewardForMatch(match.Type));
                }
            }
        }
        
        isSpinning = false;
    }
    
    public void SpinForUnit(UnitObject unit)
    {
        if (unit == null)
        {
            Global.DEBUG_PRINT("Cannot spin for null unit!");
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
            Global.DEBUG_PRINT("Failed to generate symbols for unit!");
            isSpinning = false;
            return;
        }
        
        gridUI.StartSpinAnimation(finalSymbols);
        FillGridWithSymbols(finalSymbols);
        
        // Use the same pattern as FillGridWithRandomSymbols
        StartCoroutine(WaitForSpinComplete(gridUI, true));
    }
    
    public SpinResult GetLastSpinResult()
    {
        if (lastMatches == null) 
        {
            return null;
        }
        
        return new SpinResult
        {
            Matches = ConvertMatchesToMatchData(lastMatches),
            Grid = slotGrid.GetGridCopy(),
            SpinCost = lastSpinCost
        };
    }
    
    public void StartNewTurn()
    {
        spinsThisTurn = 0;
        lastMatches = null;
        lastSpinCost = 0;
        Global.DEBUG_PRINT("New turn started - next spin is FREE!");
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
        
        Global.DEBUG_PRINT("=== Match Types Found ===");
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
    
    private List<MatchData> ConvertMatchesToMatchData(List<Match> matches)
    {
        if (matches == null) return null;
        
        var matchDataList = new List<MatchData>();

        foreach (var match in matches)
        {
            matchDataList.Add(new MatchData
            {
                Type = match.Type,
                Positions = match.Positions,
                MatchedSymbol = match.Symbol,
                GoldReward = GetGoldRewardForMatch(match.Type)
            });
        }

        return matchDataList;
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
} 