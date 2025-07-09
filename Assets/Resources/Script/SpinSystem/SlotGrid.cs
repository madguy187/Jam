using UnityEngine;

public class SlotGrid
{
    private SymbolType[,] grid = new SymbolType[3, 3];
    
    public void SetSlot(int row, int col, SymbolType symbol)
    {
        grid[row, col] = symbol;
    }
    
    public SymbolType GetSlot(int row, int col)
    {
        return grid[row, col];
    }
    
    public void ClearGrid()
    {
        for (int row = 0; row < 3; row++)
        {
            for (int col = 0; col < 3; col++)
            {
                grid[row, col] = SymbolType.EMPTY;
            }
        }
    }

    public SymbolType[,] GetGridCopy()
    {
        SymbolType[,] copy = new SymbolType[3, 3];
        for (int row = 0; row < 3; row++)
        {
            for (int col = 0; col < 3; col++)
            {
                copy[row, col] = grid[row, col];
            }
        }
        return copy;
    }
}