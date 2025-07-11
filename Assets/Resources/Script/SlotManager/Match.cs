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

    public MatchType GetMatchType() 
    {
        return type;
    }

    public List<Vector2Int> GetPositions() 
    {
        return positions;
    }

    public List<GridPosition> GetReadablePositions() 
    {
        return readablePositions;
    }

    public SymbolType GetSymbol() 
    {
        return symbol;
    }

    public Match(MatchType type, List<Vector2Int> positions, SymbolType symbol)
    {
        this.type = type;
        this.positions = positions;
        this.symbol = symbol;
        this.readablePositions = new List<GridPosition>();
        
        foreach (var pos in positions)
        {
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

        // Check if any position in this match is part of the other match
        foreach (var pos in GetPositions())
        {
            if (match.GetPositions().Contains(pos))
            {
                return true;
            }
        }
        return false;
    }

    private GridPosition GetGridPosition(Vector2Int pos)
    {
        int index = pos.x * 3 + pos.y;
        
        switch (index)
        {
            case 0:
                return GridPosition.TOPLEFT;
            case 1:
                return GridPosition.TOPMIDDLE;
            case 2:
                return GridPosition.TOPRIGHT;
            case 3:
                return GridPosition.MIDDLELEFT;
            case 4:
                return GridPosition.CENTER;
            case 5:
                return GridPosition.MIDDLERIGHT;
            case 6:
                return GridPosition.BOTTOMLEFT;
            case 7:
                return GridPosition.BOTTOMMIDDLE;
            case 8:
                return GridPosition.BOTTOMRIGHT;
            default:
                Global.DEBUG_PRINT($"[Match] Invalid grid position: ({pos.x}, {pos.y})");
                return GridPosition.CENTER;
        }
    }
} 