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
        Dictionary<SymbolType, List<Match>> matchesBySymbolAndType = new Dictionary<SymbolType, List<Match>>();

        // First detect all complex patterns
        var complexMatches = new List<Match>();
        complexMatches.AddRange(DetectFullGridMatches());
        complexMatches.AddRange(DetectXShapeMatches());
        complexMatches.AddRange(DetectDiagonalMatches());
        complexMatches.AddRange(DetectZigzagMatches());
        complexMatches.AddRange(DetectHorizontalMatches());
        complexMatches.AddRange(DetectVerticalMatches());

        // Group matches by symbol and type
        foreach (var match in complexMatches)
        {
            if (!matchesBySymbolAndType.ContainsKey(match.GetSymbol()))
            {
                matchesBySymbolAndType[match.GetSymbol()] = new List<Match>();
            }
            matchesBySymbolAndType[match.GetSymbol()].Add(match);

            // Track all positions that are part of complex patterns
            foreach (var pos in match.GetPositions())
            {
                positionsInComplexPatterns.Add(pos);
            }
        }

        // If flag is ON, add all complex matches
        if (Global.EXCLUDE_SINGLES_IN_LARGER_PATTERNS)
        {
            allMatches.AddRange(complexMatches);
        }
        else
        {
            foreach (var symbolMatches in matchesBySymbolAndType)
            {
                var uniqueTypeMatches = symbolMatches.Value
                    .GroupBy(m => m.GetMatchType())
                    .Select(g => g.First());
                allMatches.AddRange(uniqueTypeMatches);
            }
        }

        // Now handle singles detection
        var potentialSingles = DetectSingleMatches();
        var singlesBySymbol = new Dictionary<SymbolType, List<Match>>();    

        // Group singles by symbol type
        foreach (var single in potentialSingles)
        {
            if (!singlesBySymbol.ContainsKey(single.GetSymbol()))
            {
                singlesBySymbol[single.GetSymbol()] = new List<Match>();
            }
            singlesBySymbol[single.GetSymbol()].Add(single);
        }

        // For each symbol type, add ONE single if any position of that symbol is not part of a complex pattern
        foreach (var symbolSingles in singlesBySymbol)
        {
            foreach (var single in symbolSingles.Value)
            {
                // If this position is not part of any complex pattern
                if (!positionsInComplexPatterns.Contains(single.GetPositions()[0]))
                {
                    // Only add one single per symbol type
                    allMatches.Add(single);
                    break; 
                }
            }
        }

        return allMatches;
    }

    private List<Match> DetectFullGridMatches()
    {
        List<Match> matches = new List<Match>();
        SymbolType firstSymbol = grid.GetSlot(0, 0);
        
        if (firstSymbol == SymbolType.EMPTY)
            return matches;

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
            matches.Add(new Match(MatchType.FULLGRID, new List<Vector2Int>(fullGridPattern), firstSymbol));
        }

        return matches;
    }

    private List<Match> DetectXShapeMatches()
    {
        List<Match> matches = new List<Match>();
        SymbolType centerSymbol = grid.GetSlot(1, 1);
        
        if (centerSymbol == SymbolType.EMPTY)
            return matches;

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
            matches.Add(new Match(MatchType.XSHAPE, new List<Vector2Int>(xShapePattern), centerSymbol));
        }

        return matches;
    }

    private List<Match> DetectZigzagMatches()
    {
        List<Match> matches = new List<Match>();

        foreach (var pattern in zigzagPatterns)
        {
            SymbolType firstSymbol = grid.GetSlot(pattern[0].x, pattern[0].y);
            if (firstSymbol == SymbolType.EMPTY)
                continue;

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
                matches.Add(new Match(MatchType.ZIGZAG, new List<Vector2Int>(pattern), firstSymbol));
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
            if (firstSymbol == SymbolType.EMPTY)
                continue;

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
                matches.Add(new Match(MatchType.DIAGONAL, new List<Vector2Int>(pattern), firstSymbol));
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
            if (firstSymbol == SymbolType.EMPTY)
                continue;

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
                matches.Add(new Match(MatchType.HORIZONTAL, new List<Vector2Int>(pattern), firstSymbol));
            }
        }

        return matches;
    }

    private List<Match> DetectVerticalMatches()
    {
        List<Match> matches = new List<Match>();

        foreach (var pattern in verticalPatterns)
        {
            SymbolType firstSymbol = grid.GetSlot(pattern[0].x, pattern[0].y);
            if (firstSymbol == SymbolType.EMPTY)
                continue;

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
                matches.Add(new Match(MatchType.VERTICAL, new List<Vector2Int>(pattern), firstSymbol));
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