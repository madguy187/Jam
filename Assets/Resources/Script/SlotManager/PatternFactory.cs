using UnityEngine;
using System.Collections.Generic;

public class PatternFactory
{
    public static List<Vector2Int[]> GetHorizontalPatterns()
    {
        return new List<Vector2Int[]>
        {
            new Vector2Int[] { new Vector2Int(0, 0), new Vector2Int(0, 1), new Vector2Int(0, 2) },
            new Vector2Int[] { new Vector2Int(1, 0), new Vector2Int(1, 1), new Vector2Int(1, 2) },
            new Vector2Int[] { new Vector2Int(2, 0), new Vector2Int(2, 1), new Vector2Int(2, 2) }
        };
    }

    public static List<Vector2Int[]> GetVerticalPatterns()
    {
        return new List<Vector2Int[]>
        {
            new Vector2Int[] { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0) },
            new Vector2Int[] { new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(2, 1) },
            new Vector2Int[] { new Vector2Int(0, 2), new Vector2Int(1, 2), new Vector2Int(2, 2) }
        };
    }

    public static List<Vector2Int[]> GetDiagonalPatterns()
    {
        return new List<Vector2Int[]>
        {
            new Vector2Int[] { new Vector2Int(0, 0), new Vector2Int(1, 1), new Vector2Int(2, 2) },
            new Vector2Int[] { new Vector2Int(0, 2), new Vector2Int(1, 1), new Vector2Int(2, 0) }
        };
    }

    public static List<Vector2Int[]> GetZigzagPatterns()
    {
        return new List<Vector2Int[]>
        {
            // Horizontal zigzags
            new Vector2Int[] { new Vector2Int(0, 0), new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(1, 2) },
            new Vector2Int[] { new Vector2Int(1, 0), new Vector2Int(1, 1), new Vector2Int(2, 1), new Vector2Int(2, 2) },
            new Vector2Int[] { new Vector2Int(0, 1), new Vector2Int(0, 2), new Vector2Int(1, 0), new Vector2Int(1, 1) },
            new Vector2Int[] { new Vector2Int(1, 1), new Vector2Int(1, 2), new Vector2Int(2, 0), new Vector2Int(2, 1) },
            
            // Vertical zigzags
            new Vector2Int[] { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(1, 1), new Vector2Int(2, 1) },
            new Vector2Int[] { new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(1, 2), new Vector2Int(2, 2) },
            new Vector2Int[] { new Vector2Int(0, 1), new Vector2Int(1, 0), new Vector2Int(1, 1), new Vector2Int(2, 0) },
            new Vector2Int[] { new Vector2Int(0, 2), new Vector2Int(1, 1), new Vector2Int(1, 2), new Vector2Int(2, 1) }
        };
    }

    public static List<Vector2Int[]> GetXShapePatterns()
    {
        return new List<Vector2Int[]>
        {
            new Vector2Int[] 
            { 
                new Vector2Int(0, 0), new Vector2Int(0, 2),
                new Vector2Int(1, 1),
                new Vector2Int(2, 0), new Vector2Int(2, 2)
            }
        };
    }

    public static Vector2Int[] GetFullGridPattern()
    {
        return new Vector2Int[]
        {
            new Vector2Int(0, 0), new Vector2Int(0, 1), new Vector2Int(0, 2),
            new Vector2Int(1, 0), new Vector2Int(1, 1), new Vector2Int(1, 2),
            new Vector2Int(2, 0), new Vector2Int(2, 1), new Vector2Int(2, 2)
        };
    }
} 