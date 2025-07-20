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
    
    private SlotGrid slotGrid;
    private MatchDetector matchDetector;
    private bool isSpinning;
    private SpinResult spinResult;
    private int spinsThisTurn = 0;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
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
                Global.DEBUG_PRINT("Cannot afford spin, cost: " + spinCost);
                return;
            }
        }
        else
        {
            Global.DEBUG_PRINT("Using free spin!");
        }

        isSpinning = true;
        // Disable roll button during spin
        if (rollButton != null)
        {
            rollButton.SetInteractable(false);
        }
        
        IncrementSpins();
        
        // Generate random symbols
        SymbolType[] finalSymbols = new SymbolType[slotConfig.TotalGridSize];
        for (int i = 0; i < slotConfig.TotalGridSize; i++)
        {
            finalSymbols[i] = SymbolGenerator.instance.GenerateRandomSymbol();
        }
        
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
            if (match.GetMatchType() != MatchType.SINGLE)
            {
                int goldReward = GoldManager.instance.GetGoldRewardForMatch(match.GetMatchType());
                totalGold += goldReward;
                GoldManager.instance.AddGold(goldReward);
            }
        }

        // If we have matches, execute combat immediately
        if (matches.Count > 0)
        {
            // Get the player's unit that will attack
            Deck playerDeck = DeckManager.instance.GetDeckByType(eDeckType.PLAYER);
            for (int i = 0; i < playerDeck.GetDeckMaxSize(); i++)
            {
                var unit = playerDeck.GetUnitObject(i);
                if (unit != null && unit.unitSO != null)
                {
                    // Set unit name for each match
                    string unitName = unit.unitSO.unitName;
                    foreach (var match in matches)
                    {
                        match.SetUnitName(unitName);
                    }

                    // Execute the attack immediately
                    CombatManager.instance.ExecBattle(eDeckType.PLAYER, i);
                    break;
                }
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