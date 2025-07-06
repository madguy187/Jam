using UnityEngine;

public static class Global {

    public static bool IS_DEBUG = false;

    static public void DEBUG_PRINT(string strMessage) {
        if (!IS_DEBUG) {
            return;
        }

        Debug.Log(strMessage);
    }

}
