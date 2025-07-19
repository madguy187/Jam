using System.Collections.Generic;
using UnityEngine;

public enum GridPosition
{
    TOPLEFT,
    TOPMIDDLE,
    TOPRIGHT,
    MIDDLELEFT,
    CENTER,
    MIDDLERIGHT,
    BOTTOMLEFT,
    BOTTOMMIDDLE,
    BOTTOMRIGHT
}

public enum MatchType
{
    NONE,
    SINGLE,     
    HORIZONTAL, 
    VERTICAL,   
    DIAGONAL,   
    ZIGZAG,     
    XSHAPE,     
    FULLGRID    
}

public class Match
{
    private MatchType type;
    private List<Vector2Int> positions;
    private List<GridPosition> readablePositions;
    private SymbolType symbol;
    private string unitName;

    public MatchType GetMatchType() {
        return type;
    }

    public List<Vector2Int> GetPositions() 
    {
        return new List<Vector2Int>(positions);
    }

    public List<GridPosition> GetReadablePositions() 
    {
        return new List<GridPosition>(readablePositions);
    }

    public SymbolType GetSymbol() 
    {
        return symbol;
    }
    
    public string GetUnitName() 
    {
        return unitName;
    }

    public void SetUnitName(string name)
    {
        unitName = name;
    }

    public Match(MatchType type, List<Vector2Int> positions, SymbolType symbol) {
        if (positions == null) {
            Debug.LogWarning("[Match] Positions list cannot be null");
            return;
        }

        this.type = type;
        this.positions = positions;
        this.symbol = symbol;
        this.readablePositions = new List<GridPosition>();

        foreach (var pos in positions) {
            readablePositions.Add(GetGridPosition(pos));
        }
    }

    public bool HasOverlap(Match match)
    {
        // Skip if it's the same match or a single match
        if (match == this || match.GetMatchType() == MatchType.SINGLE)
        {
            return false;
        }

        var otherPositions = new HashSet<Vector2Int>(match.GetPositions());
        foreach (var pos in positions)
        {
            if (otherPositions.Contains(pos))
            {
                return true;
            }
        }
        return false;
    }

    private static readonly GridPosition[] gridPositionMap = 
    {
        GridPosition.TOPLEFT,     GridPosition.TOPMIDDLE,    GridPosition.TOPRIGHT,
        GridPosition.MIDDLELEFT,  GridPosition.CENTER,       GridPosition.MIDDLERIGHT,
        GridPosition.BOTTOMLEFT,  GridPosition.BOTTOMMIDDLE, GridPosition.BOTTOMRIGHT
    };

    private GridPosition GetGridPosition(Vector2Int pos)
    {
        int index = pos.x * 3 + pos.y;
        
        if (index >= 0 && index < gridPositionMap.Length)
        {
            return gridPositionMap[index];
        }
        
        Global.DEBUG_PRINT($"[Match] Invalid grid position: ({pos.x}, {pos.y})");
        return GridPosition.CENTER;
    }
} 