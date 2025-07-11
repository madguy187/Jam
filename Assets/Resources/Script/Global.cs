using UnityEngine;
using System.Collections.Generic;

public static class Global {
    public const int GRID_SIZE = 3; 
    public static bool IS_DEBUG = true;
    public static bool RUN_SIMULATION = false; 
    public const bool EXCLUDE_SINGLES_IN_LARGER_PATTERNS = true;

    static public void DEBUG_PRINT(string strMessage) {
        if (!IS_DEBUG) {
            return;
        }

        Debug.Log(strMessage);
    }
}
