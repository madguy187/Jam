using UnityEngine;
using System.Collections.Generic;

public static class Global {

    public static bool IS_DEBUG = true;

    static public void DEBUG_PRINT(string strMessage) {
        if (!IS_DEBUG) {
            return;
        }

        Debug.Log(strMessage);
    }
}
