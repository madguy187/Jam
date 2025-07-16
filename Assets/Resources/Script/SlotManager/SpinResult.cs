using System.Collections.Generic;
using UnityEngine;
using System.Linq;

// Other systems can just use SpinResult.instance to access the latest spin results
public class SpinResult
{
    private List<Match> allMatches = new List<Match>();
    private Dictionary<MatchType, List<Match>> matchesByType = new Dictionary<MatchType, List<Match>>();
    private Dictionary<SymbolType, Match> singleMatches = new Dictionary<SymbolType, Match>();
    private int totalGoldEarned;

    public SpinResult(List<Match> matches, int goldEarned)
    {
        SetMatches(matches, goldEarned);
    }

    public void SetMatches(List<Match> matches, int goldEarned)
    {
        allMatches.Clear();
        if (matches != null)
        {
            allMatches.AddRange(matches);
        }
        totalGoldEarned = goldEarned;
        
        matchesByType.Clear();
        singleMatches.Clear();
        
        foreach (var group in allMatches.GroupBy(m => m.GetMatchType()))
        {
            matchesByType[group.Key] = group.ToList();
        }
        
        foreach (var match in allMatches.Where(m => m.GetMatchType() == MatchType.SINGLE))
        {
            singleMatches[match.GetSymbol()] = match;
        }
    }

    public void Clear()
    {
        allMatches.Clear();
        matchesByType.Clear();
        singleMatches.Clear();
        totalGoldEarned = 0;
    }

    public List<Match> GetAllMatches()
    {
        return allMatches;
    }

    public Dictionary<MatchType, List<Match>> GetMatchesByType()
    {
        return matchesByType;
    }

    public Dictionary<SymbolType, Match> GetSingleMatches()
    {
        return singleMatches;
    }

    public int GetTotalGoldEarned()
    {
        return totalGoldEarned;
    }

    public bool HasMatchType(MatchType type) 
    {
        return matchesByType.ContainsKey(type);
    }

    public bool HasSingleMatch(SymbolType symbol) 
    {
        return singleMatches.ContainsKey(symbol);
    }

    public List<Match> GetMatchesOfType(MatchType type) 
    {
        if (matchesByType.TryGetValue(type, out var matches))
        {
            return matches;
        }
        return new List<Match>();
    }

    public Match GetSingleMatch(SymbolType symbol) 
    {
        if (singleMatches.TryGetValue(symbol, out var match))
        {
            return match;
        }
        return null;
    }
} 