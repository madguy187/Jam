using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MatchDetector
{
    private SlotGrid grid;
    
    // Load all the complex shapes first
    private List<Vector2Int>[] horizontalPatterns;
    private List<Vector2Int>[] verticalPatterns;
    private List<Vector2Int>[] diagonalPatterns;
    private List<List<Vector2Int>> zigzagPatterns;
    private List<Vector2Int> xShapePattern;
    private List<Vector2Int> fullGridPattern;

    public MatchDetector(SlotGrid grid)
    {
        this.grid = grid;
        InitializePatterns();
    }

    private void InitializePatterns()
    {
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

        verticalPatterns = new List<Vector2Int>[3];
        for (int col = 0; col < 3; col++)
        {
            verticalPatterns[col] = new List<Vector2Int>
            {
                new Vector2Int(0, col),
                new Vector2Int(1, col),
                new Vector2Int(2, col)
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
        
        // Initialize zigzag patterns
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
        
        fullGridPattern = new List<Vector2Int>();
        for (int row = 0; row < 3; row++)
        {
            for (int col = 0; col < 3; col++)
            {
                fullGridPattern.Add(new Vector2Int(row, col));
            }
        }
    }

    public List<Match> DetectMatches()
    {
        List<Match> allMatches = new List<Match>();
        HashSet<Vector2Int> positionsInComplexPatterns = new HashSet<Vector2Int>();

        var complexMatches = new List<Match>();
        complexMatches.AddRange(DetectHorizontalMatches());  
        complexMatches.AddRange(DetectVerticalMatches());   
        complexMatches.AddRange(DetectDiagonalMatches());    
        complexMatches.AddRange(DetectZigzagMatches());      
        complexMatches.AddRange(DetectXShapeMatches());      
        complexMatches.AddRange(DetectFullGridMatches());    

        // Track all positions that are part of complex patterns
        foreach (var match in complexMatches)
        {
            foreach (var pos in match.GetPositions())
            {
                positionsInComplexPatterns.Add(pos);
            }
        }

        // Handle singles detection
        var potentialSingles = DetectSingleMatches();
        var singlesBySymbol = new Dictionary<SymbolType, List<Match>>();    

        // Group singles by symbol type
        foreach (var single in potentialSingles)
        {
            var symbol = single.GetSymbol();
            if (!singlesBySymbol.ContainsKey(symbol))
            {
                singlesBySymbol[symbol] = new List<Match>();
            }
            singlesBySymbol[symbol].Add(single);
        }

        // Add singles that aren't part of complex patterns
        foreach (var symbolSingles in singlesBySymbol)
        {
            foreach (var single in symbolSingles.Value)
            {
                if (!positionsInComplexPatterns.Contains(single.GetPositions()[0]))
                {
                    allMatches.Add(single);
                    break; // Only add one single per symbol type
                }
            }
        }

        // Then add complex matches
        if (Global.EXCLUDE_SINGLES_IN_LARGER_PATTERNS)
        {
            allMatches.AddRange(complexMatches);
        }

        return allMatches;
    }

    private bool IsMatchingPattern(List<Vector2Int> pattern, out SymbolType symbol)
    {
        symbol = grid.GetSlot(pattern[0].x, pattern[0].y);
        if (symbol == SymbolType.EMPTY)
            return false;

        foreach (var pos in pattern)
        {
            if (grid.GetSlot(pos.x, pos.y) != symbol)
                return false;
        }

        return true;
    }

    private List<Match> DetectFullGridMatches()
    {
        List<Match> matches = new List<Match>();
        
        if (IsMatchingPattern(fullGridPattern, out SymbolType symbol))
        {
            matches.Add(new Match(MatchType.FULLGRID, new List<Vector2Int>(fullGridPattern), symbol));
        }

        return matches;
    }

    private List<Match> DetectXShapeMatches()
    {
        List<Match> matches = new List<Match>();
        
        if (IsMatchingPattern(xShapePattern, out SymbolType symbol))
        {
            matches.Add(new Match(MatchType.XSHAPE, new List<Vector2Int>(xShapePattern), symbol));
        }

        return matches;
    }

    private List<Match> DetectZigzagMatches()
    {
        List<Match> matches = new List<Match>();

        foreach (var pattern in zigzagPatterns)
        {
            if (IsMatchingPattern(pattern, out SymbolType symbol))
            {
                matches.Add(new Match(MatchType.ZIGZAG, new List<Vector2Int>(pattern), symbol));
            }
        }

        return matches;
    }

    private List<Match> DetectDiagonalMatches()
    {
        List<Match> matches = new List<Match>();

        foreach (var pattern in diagonalPatterns)
        {
            if (IsMatchingPattern(pattern, out SymbolType symbol))
            {
                matches.Add(new Match(MatchType.DIAGONAL, new List<Vector2Int>(pattern), symbol));
            }
        }

        return matches;
    }

    private List<Match> DetectHorizontalMatches()
    {
        List<Match> matches = new List<Match>();

        foreach (var pattern in horizontalPatterns)
        {
            if (IsMatchingPattern(pattern, out SymbolType symbol))
            {
                matches.Add(new Match(MatchType.HORIZONTAL, new List<Vector2Int>(pattern), symbol));
            }
        }

        return matches;
    }

    private List<Match> DetectVerticalMatches()
    {
        List<Match> matches = new List<Match>();

        foreach (var pattern in verticalPatterns)
        {
            if (IsMatchingPattern(pattern, out SymbolType symbol))
            {
                matches.Add(new Match(MatchType.VERTICAL, new List<Vector2Int>(pattern), symbol));
            }
        }

        return matches;
    }

    private List<Match> DetectSingleMatches()
    {
        List<Match> matches = new List<Match>();

        for (int row = 0; row < Global.GRID_SIZE; row++)
        {
            for (int col = 0; col < Global.GRID_SIZE; col++)
            {
                SymbolType symbol = grid.GetSlot(row, col);
                if (symbol != SymbolType.EMPTY)
                {
                    var positions = new List<Vector2Int> { new Vector2Int(row, col) };
                    matches.Add(new Match(MatchType.SINGLE, positions, symbol));
                }
            }
        }

        return matches;
    }
} 