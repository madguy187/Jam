using UnityEngine;

/*
Main controller for the slot machine system.
Grid Management + Random Genration + Animation
Create GameObject and attach the script 
This is the manager
*/

public class SpinController : MonoBehaviour
{
    public static SpinController instance;
    
    [Header("Grid Data")]
    [SerializeField] private SlotGrid slotGrid;
    public System.Action OnGridUpdated;
    
    void Awake()
    {
        instance = this;
        slotGrid = new SlotGrid();
    }
    
    void Start()
    {
        ClearGrid();
        
        if (SymbolGenerator.instance != null)
        {
            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 3; col++)
                {
                    SymbolType randomSymbol = SymbolGenerator.instance.GenerateRandomSymbol();
                    slotGrid.SetSlot(row, col, randomSymbol);
                }
            }
            OnGridUpdated?.Invoke();
        }
    }
    
    public void FillGridWithRandomSymbols()
    {
        if (SymbolGenerator.instance == null) return;
        
        SlotGridUI gridUI = FindObjectOfType<SlotGridUI>();
        
        if (gridUI != null && !gridUI.IsSpinning())
        {
            SymbolType[] finalSymbols = new SymbolType[9];
            for (int i = 0; i < 9; i++)
            {
                finalSymbols[i] = SymbolGenerator.instance.GenerateRandomSymbol();
            }
            
            gridUI.StartSpinAnimation(finalSymbols);
            FillGridWithSymbols(finalSymbols);
        }
    }
    
    private void FillGridWithSymbols(SymbolType[] symbols)
    {
        if (symbols.Length != 9) return;
        
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
    
    public void ClearGrid()
    {
        slotGrid.ClearGrid();
        OnGridUpdated?.Invoke();
    }
    
    public SlotGrid GetSlotGrid()
    {
        return slotGrid;
    }
} 