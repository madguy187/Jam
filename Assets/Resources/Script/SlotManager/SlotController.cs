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
    public static SlotController instance;
    
    [Header("Configuration")]
    [SerializeField] private GoldConfig goldConfig;
    [SerializeField] private SlotConfig slotConfig;

    [Header("References")]
    [SerializeField] private SlotGridUI gridUI; 
    
    private SlotGrid slotGrid;
    private MatchDetector matchDetector;
    private int spinsThisTurn = 0;
    private bool isSpinning;
    private SpinResult spinResult;
    private int currentSpinCost;
    
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
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }

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
        
        StartCoroutine(WaitForSpinComplete(autoSpendGold));
    }
    
    private void ProcessMatchesForUnit(List<Match> matches)
    {
        if (matches.Count <= 0) return;

        UnitObject selectedUnit = null;
        Deck playerDeck = DeckManager.instance.GetDeckByType(eDeckType.PLAYER);
        
        // Get first non-null unit from deck
        for (int i = 0; i < playerDeck.GetDeckMaxSize(); i++)
        {
            var unit = playerDeck.GetUnitObject(i);
            if (unit != null)
            {
                selectedUnit = unit;
                break;
            }
        }
        
        Global.DEBUG_PRINT($"[SlotController] Selected Unit: {(selectedUnit ? selectedUnit.name : "None")}");
        
        if (selectedUnit != null)
        {
            Global.DEBUG_PRINT($"[SlotController] Has UnitSO: {(selectedUnit.unitSO != null ? "Yes" : "No")}");
            if (selectedUnit.unitSO == null)
            {
                Global.DEBUG_PRINT("[SlotController] ERROR: Unit's ScriptableObject (unitSO) is null!");
                return;
            }

            string unitName = selectedUnit.unitSO.unitName;
            Global.DEBUG_PRINT($"[SlotController] Unit Name: {unitName}");
            Global.DEBUG_PRINT($"[SlotController] Unit SO Name: {selectedUnit.unitSO.name}");
            
            foreach (var match in matches)
            {
                match.SetUnitName(unitName);
                Global.DEBUG_PRINT($"[SlotController] Match {match.GetMatchType()} -> Unit: {match.GetUnitName()}");
            }

            // Start the battle loop
            Global.DEBUG_PRINT("[SlotController] Starting Combat");
            CombatManager.instance.StartBattleLoop(matches);
        }
        else
        {
            Global.DEBUG_PRINT("[SlotController] No unit in player deck!");
        }
    }

    private IEnumerator WaitForSpinComplete(bool autoSpendGold)
    {
        // Wait for spin animation to complete
        while (gridUI.GetIsSpinning())
        {
            yield return null;
        }

        // After grid is filled and animation is complete, check for matches
        List<Match> matches = CheckForMatches();
        Global.DEBUG_PRINT($"[SlotController] Found {matches.Count} matches");
        
        // Calculate total gold earned
        int totalGold = 0;
        if (autoSpendGold && matches.Count > 0)
        {
            foreach (Match match in matches)
            {
                if (match.GetMatchType() != MatchType.SINGLE)
                {
                    int goldReward = GetGoldRewardForMatch(match.GetMatchType());
                    totalGold += goldReward;
                    GoldManager.instance.AddGold(goldReward);
                }
            }
        }

        // Process matches and start combat if needed
        ProcessMatchesForUnit(matches);
        
        // Save matches to our SpinResult
        spinResult.SetMatches(matches, totalGold);
        
        isSpinning = false;
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
        
        StartCoroutine(WaitForSpinComplete(true));
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
    
    private int GetGoldRewardForMatch(MatchType type)
    {
        if (goldConfig == null)
        {
            Debug.LogWarning("[SlotController] Gold configuration is missing! Please assign it in the inspector.");
            return 0;
        }

        switch (type)
        {
            case MatchType.HORIZONTAL:
                return goldConfig.horizontalReward;
            case MatchType.VERTICAL:
                return goldConfig.verticalReward;
            case MatchType.DIAGONAL:
                return goldConfig.diagonalReward;
            case MatchType.ZIGZAG:
                return goldConfig.zigzagReward;
            case MatchType.XSHAPE:
                return goldConfig.xShapeReward;
            case MatchType.FULLGRID:
                return goldConfig.fullGridReward;
            default:
                return 0;
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
} 