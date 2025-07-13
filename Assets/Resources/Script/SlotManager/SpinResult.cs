using System.Collections.Generic;
using UnityEngine;
using System.Linq;

// Other systems can just use SpinResult.instance to access the latest spin results
public class SpinResult
{
    private List<Match> allMatches;
    private Dictionary<MatchType, List<Match>> matchesByType;
    private Dictionary<SymbolType, Match> singleMatches;
    private int totalGoldEarned;

    public SpinResult(List<Match> matches, int goldEarned)
    {
        SetMatches(matches, goldEarned);
    }

    public void SetMatches(List<Match> matches, int goldEarned)
    {
        allMatches = matches ?? new List<Match>();
        totalGoldEarned = goldEarned;
        
        matchesByType = allMatches.GroupBy(m => m.GetMatchType()).ToDictionary(g => g.Key, g => g.ToList());
        singleMatches = allMatches.Where(m => m.GetMatchType() == MatchType.SINGLE).ToDictionary(m => m.GetSymbol(), m => m);
    }

    public void Clear()
    {
        allMatches = new List<Match>();
        matchesByType = new Dictionary<MatchType, List<Match>>();
        singleMatches = new Dictionary<SymbolType, Match>();
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