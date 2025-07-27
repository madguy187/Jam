using UnityEngine;

public static class Global {
    public const int GRID_SIZE = 3;
    public static bool IS_DEBUG = true;

    // Combat
    public const int PERCENTAGE_CONSTANT = 100;
    public const int RESISTANCE_PERCENTAGE_CONSTANT = 10;
    public const int INVALID_INDEX = -1;

    static public void DEBUG_PRINT(string strMessage) {
        if (!IS_DEBUG) {
            return;
        }

        Debug.Log(strMessage);
    }
}
