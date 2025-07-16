using UnityEngine;

public class SlotGrid
{
    private SymbolType[,] grid;
    private readonly int rows;
    private readonly int columns;
    
    public SlotGrid(int rows, int columns)
    {
        this.rows = rows;
        this.columns = columns;
        grid = new SymbolType[rows, columns];
        ClearGrid();
    }
    
    public void SetSlot(int row, int col, SymbolType symbol)
    {
        if (!IsValidPosition(row, col))
        {
            Debug.LogWarning($"[SlotGrid] Invalid grid position: ({row}, {col})");
            return;
        }
        grid[row, col] = symbol;
    }
    
    public SymbolType GetSlot(int row, int col)
    {
        if (!IsValidPosition(row, col))
        {
            Debug.LogWarning($"[SlotGrid] Invalid grid position: ({row}, {col})");
            return SymbolType.EMPTY;
        }
        return grid[row, col];
    }
    
    public void ClearGrid()
    {
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                grid[row, col] = SymbolType.EMPTY;
            }
        }
    }

    public SymbolType[,] GetGridCopy()
    {
        SymbolType[,] copy = new SymbolType[rows, columns];
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                copy[row, col] = grid[row, col];
            }
        }
        return copy;
    }

    public int GetRows() => rows;
    public int GetColumns() => columns;
}