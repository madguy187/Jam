using UnityEngine;

[System.Serializable]
public enum SymbolType {
    Attack,   
    Defense,   
    Special,   
    Empty      
}

[System.Serializable]
public class GridSlot {
    public SymbolType symbolType;
    
    public GridSlot(SymbolType type = SymbolType.Empty) {
        symbolType = type;
    }
}

[System.Serializable]
public class SlotGrid {
    private GridSlot[,] grid = new GridSlot[3, 3];
    
    public SlotGrid() {
        InitializeGrid();
    }
    
    void InitializeGrid() {
        for (int row = 0; row < 3; row++) {
            for (int col = 0; col < 3; col++) {
                grid[row, col] = new GridSlot();
            }
        }
    }
    
    public GridSlot GetSlot(int row, int col) {
        if (row < 0 || row >= 3 || col < 0 || col >= 3)
            return null;
        return grid[row, col];
    }
    
    public void SetSlot(int row, int col, SymbolType symbolType) {
        if (row < 0 || row >= 3 || col < 0 || col >= 3)
            return;
        
        grid[row, col].symbolType = symbolType;
    }
    
    public void ClearGrid() {
        for (int row = 0; row < 3; row++) {
            for (int col = 0; col < 3; col++) {
                grid[row, col].symbolType = SymbolType.Empty;
            }
        }
    }
} 