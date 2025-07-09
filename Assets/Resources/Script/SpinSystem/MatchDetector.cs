using System.Collections.Generic;
using UnityEngine;

public enum MatchType
{
    SINGLE,     
    HORIZONTAL, 
    DIAGONAL,   
    ZIGZAG,     
    XSHAPE,     
    FULLGRID    
}

public class Match
{
    public MatchType Type { get; private set; }
    public List<Vector2Int> Positions { get; private set; }
    public SymbolType Symbol { get; private set; }

    // we return position for now first and see do we need to use next time
    // if animations and all need then ok , if not we remove it
    public Match(MatchType type, List<Vector2Int> positions, SymbolType symbol)
    {
        Type = type;
        Positions = positions;
        Symbol = symbol;
    }
}

public class MatchDetector
{
    private SlotGrid grid;
    private HashSet<Vector2Int> matchedPositions = new HashSet<Vector2Int>();
    
    //  Load all the complex shapes first
    private List<Vector2Int>[] horizontalPatterns;
    private List<Vector2Int>[] diagonalPatterns;
    private List<List<Vector2Int>> zigzagPatterns;
    private List<Vector2Int> xShapePattern;
    private List<Vector2Int> fullGridPattern;
    
    public MatchDetector(SlotGrid grid)
    {
        this.grid = grid;
        matchedPositions = new HashSet<Vector2Int>();
        
        horizontalPatterns = new List<Vector2Int>[3];
        for (int row = 0; row < 3; row++)
        {
            horizontalPatterns[row] = new List<Vector2Int>
            {
                new Vector2Int(row, 0),
                new Vector2Int(row, 1),
                new Vector2Int(row, 2)
            };
        }
        
        diagonalPatterns = new List<Vector2Int>[2];

        diagonalPatterns[0] = new List<Vector2Int>
        {
            new Vector2Int(0, 0),
            new Vector2Int(1, 1),
            new Vector2Int(2, 2)
        };

        diagonalPatterns[1] = new List<Vector2Int>
        {
            new Vector2Int(0, 2),
            new Vector2Int(1, 1),
            new Vector2Int(2, 0)
        };
        
        zigzagPatterns = new List<List<Vector2Int>>
        {
            new List<Vector2Int> { // Left zigzag
                new Vector2Int(0, 0), new Vector2Int(0, 1),
                new Vector2Int(1, 1),
                new Vector2Int(2, 1), new Vector2Int(2, 2)
            },
            new List<Vector2Int> { // Right zigzag
                new Vector2Int(0, 2), new Vector2Int(0, 1),
                new Vector2Int(1, 1),
                new Vector2Int(2, 1), new Vector2Int(2, 0)
            }
        };
        
        xShapePattern = new List<Vector2Int>
        {
            new Vector2Int(0, 0), 
            new Vector2Int(0, 2), 
            new Vector2Int(1, 1), 
            new Vector2Int(2, 0), 
            new Vector2Int(2, 2)  
        };
        
        // Init full grid pattern
        fullGridPattern = new List<Vector2Int>();
        for (int row = 0; row < 3; row++)
        {
            for (int col = 0; col < 3; col++)
            {
                fullGridPattern.Add(new Vector2Int(row, col));
            }
        }
    }
    
    public List<Match> DetectAllMatches()
    {
        List<Match> matches = new List<Match>();
        matchedPositions.Clear();
        
        Global.DEBUG_PRINT("=== Starting Match Detection ===");
        
        // Detect patterns first
        var horizontalMatches = DetectHorizontalMatches();
        Global.DEBUG_PRINT($"Found {horizontalMatches.Count} horizontal matches");
        matches.AddRange(horizontalMatches);
        
        var diagonalMatches = DetectDiagonalMatches();
        Global.DEBUG_PRINT($"Found {diagonalMatches.Count} diagonal matches");
        matches.AddRange(diagonalMatches);
        
        var zigzagMatches = DetectZigzagMatches();
        Global.DEBUG_PRINT($"Found {zigzagMatches.Count} zigzag matches");
        matches.AddRange(zigzagMatches);
        
        var xShapeMatches = DetectXShapeMatches();
        Global.DEBUG_PRINT($"Found {xShapeMatches.Count} X-shape matches");
        matches.AddRange(xShapeMatches);
        
        Match fullGridMatch = DetectFullGridMatch();
        if (fullGridMatch != null)
        {
            Global.DEBUG_PRINT("Found full grid match");
            matches.Add(fullGridMatch);
        }

        Global.DEBUG_PRINT($"Matched positions before singles: {matchedPositions.Count}");
        
        // Then detect single matches for any unmatched positions
        var singleMatches = DetectSingleMatches();
        Global.DEBUG_PRINT($"Found {singleMatches.Count} single matches");
        matches.AddRange(singleMatches);
        
        return matches;
    }

    private List<Match> DetectSingleMatches()
    {
        List<Match> matches = new List<Match>();
        HashSet<SymbolType> processedSymbols = new HashSet<SymbolType>();
        
        // Check each position in the grid
        for (int row = 0; row < 3; row++)
        {
            for (int col = 0; col < 3; col++)
            {
                Vector2Int pos = new Vector2Int(row, col);
                SymbolType symbol = grid.GetSlot(row, col);
                
                // Skip if empty or if we already processed this symbol type
                if (symbol == SymbolType.EMPTY || processedSymbols.Contains(symbol)) 
                {
                    continue;
                }
                
                // Skip if this position was part of a pattern match
                if (matchedPositions.Contains(pos)) 
                {
                    continue;
                }
                
                // Add to processed symbols and create a match
                processedSymbols.Add(symbol);
                matches.Add(new Match(
                    MatchType.SINGLE,
                    new List<Vector2Int> { pos },
                    symbol
                ));
                Global.DEBUG_PRINT($"Found single match for symbol {symbol}");
            }
        }
        
        return matches;
    }
    
    private List<Match> DetectHorizontalMatches()
    {
        List<Match> matches = new List<Match>();
        
        foreach (var pattern in horizontalPatterns)
        {
            SymbolType firstSymbol = grid.GetSlot(pattern[0].x, pattern[0].y);
            if (firstSymbol == SymbolType.EMPTY) continue;
            
            bool isMatch = true;
            foreach (var pos in pattern)
            {
                if (grid.GetSlot(pos.x, pos.y) != firstSymbol)
                {
                    isMatch = false;
                    break;
                }
            }
            
            if (isMatch)
            {
                foreach (var pos in pattern)
                {
                    matchedPositions.Add(pos);
                }
                
                matches.Add(new Match(
                    MatchType.HORIZONTAL,
                    new List<Vector2Int>(pattern),
                    firstSymbol
                ));
            }
        }
        
        return matches;
    }
    
    private List<Match> DetectDiagonalMatches()
    {
        List<Match> matches = new List<Match>();
        
        foreach (var pattern in diagonalPatterns)
        {
            SymbolType firstSymbol = grid.GetSlot(pattern[0].x, pattern[0].y);
            if (firstSymbol == SymbolType.EMPTY) continue;
            
            bool isMatch = true;
            foreach (var pos in pattern)
            {
                if (grid.GetSlot(pos.x, pos.y) != firstSymbol)
                {
                    isMatch = false;
                    break;
                }
            }
            
            if (isMatch)
            {
                foreach (var pos in pattern)
                {
                    matchedPositions.Add(pos);
                }
                
                matches.Add(new Match(
                    MatchType.DIAGONAL,
                    pattern,
                    firstSymbol
                ));
            }
        }
        
        return matches;
    }
    
    private List<Match> DetectZigzagMatches()
    {
        List<Match> matches = new List<Match>();
        
        foreach (var pattern in zigzagPatterns)
        {
            SymbolType firstSymbol = grid.GetSlot(pattern[0].x, pattern[0].y);
            if (firstSymbol == SymbolType.EMPTY) continue;
            
            bool isMatch = true;
            foreach (var pos in pattern)
            {
                if (grid.GetSlot(pos.x, pos.y) != firstSymbol)
                {
                    isMatch = false;
                    break;
                }
            }
            
            if (isMatch)
            {
                foreach (var pos in pattern)
                {
                    matchedPositions.Add(pos);
                }
                
                matches.Add(new Match(
                    MatchType.ZIGZAG,
                    pattern,
                    firstSymbol
                ));
            }
        }
        
        return matches;
    }
    
    private List<Match> DetectXShapeMatches()
    {
        List<Match> matches = new List<Match>();
        
        SymbolType centerSymbol = grid.GetSlot(1, 1);
        if (centerSymbol == SymbolType.EMPTY) return matches;
        
        bool isMatch = true;
        foreach (var pos in xShapePattern)
        {
            if (grid.GetSlot(pos.x, pos.y) != centerSymbol)
            {
                isMatch = false;
                break;
            }
        }
        
        if (isMatch)
        {
            foreach (var pos in xShapePattern)
            {
                matchedPositions.Add(pos);
            }
            
            matches.Add(new Match(
                MatchType.XSHAPE,
                xShapePattern,
                centerSymbol
            ));
        }
        
        return matches;
    }
    
    private Match DetectFullGridMatch()
    {
        SymbolType firstSymbol = grid.GetSlot(0, 0);
        if (firstSymbol == SymbolType.EMPTY) return null;
        
        bool isMatch = true;
        foreach (var pos in fullGridPattern)
        {
            if (grid.GetSlot(pos.x, pos.y) != firstSymbol)
            {
                isMatch = false;
                break;
            }
        }
        
        if (isMatch)
        {
            foreach (var pos in fullGridPattern)
            {
                matchedPositions.Add(pos);
            }
            
            return new Match(
                MatchType.FULLGRID,
                fullGridPattern,
                firstSymbol
            );
        }
        
        return null;
    }
} 