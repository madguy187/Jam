using UnityEngine;

public static class Global {
    public const int GRID_SIZE = 3;
    public static bool IS_DEBUG = true;

    // Combat
    public const int PERCENTAGE_CONSTANT = 100;
    public const int RESISTANCE_PERCENTAGE_CONSTANT = 10;
    public const int INVALID_INDEX = -1;
    public const int TEMP_EFFECT_ONLY_THIS_TURN = 1;
    public const int TEMP_EFFECT_ONLY_THIS_ROUND = 0;

    // Simulation test for Darren&Tianyu to verify all kinds of complicated scenarios
    public static bool RUN_SIMULATION = false; 
    // Discussed with Tianyu that there are 2 kinds of behavior, 
    // So added a feature flag to control this behaviour
    // If false= We only return UNIQUE matches
    public const bool EXCLUDE_SINGLES_IN_LARGER_PATTERNS = true;

    static public void DEBUG_PRINT(string strMessage) {
        if (!IS_DEBUG) {
            return;
        }

        Debug.Log(strMessage);
    }
}
