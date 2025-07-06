using UnityEngine;

public class CombatManager : MonoBehaviour
{
    public static CombatManager instance;

    void Awake() {
        if (instance != null)
        {
            Destroy(instance);
        }
        instance = this;
    }
}
