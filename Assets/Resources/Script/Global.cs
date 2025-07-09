using UnityEngine;
using System.Collections.Generic;

public static class Global {

    public static bool IS_DEBUG = false;

    static public void DEBUG_PRINT(string strMessage) {
        if (!IS_DEBUG) {
            return;
        }

        Debug.Log(strMessage);
    }
}

public class SpinResult
{
    public List<MatchData> Matches;
    public SymbolType[,] Grid;
    public int SpinCost;
}

public class MatchData
{
    public MatchType Type;
    public List<Vector2Int> Positions;
    public SymbolType MatchedSymbol;
    public int GoldReward;
}
