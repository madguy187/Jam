using UnityEngine;

public class EnemyManager : MonoBehaviour {
    public static EnemyManager instance;

    [Header("Health")]
    [SerializeField] int nDifficultyHealthPercent = 10;

    [Header("Atk")]
    [SerializeField] float fDifficultAtk = 1.0f;
    [SerializeField] int nDifficultIncreaseEveryXNode = 2;


    [Header("CritRate")]
    [SerializeField] int nDifficultyCritRate = 2;
    [SerializeField] int nDifficultyCritRateIncreaseEveryXNode = 3;
    [SerializeField] float fDifficultyCritRateMax = 15.0f;

    int node_count = 0;
    int combat_count = 0;

    void Awake() {
        if (instance != null) {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public float GetDifficultyHealthPercent(bool isBoss = false) {
        if (isBoss) {
            return 0.0f;
        }

        return (Global.PERCENTAGE_CONSTANT + node_count * nDifficultyHealthPercent) / Global.PERCENTAGE_CONSTANT;
    }

    public float GetDifficultyAtk() {
        return fDifficultAtk * Mathf.Floor(combat_count / nDifficultIncreaseEveryXNode);
    }

    public int GetDifficultyCritRate() {
        float critRate = nDifficultyCritRate * Mathf.Floor(combat_count / nDifficultyCritRateIncreaseEveryXNode) / Global.PERCENTAGE_CONSTANT;
        if (critRate > fDifficultyCritRateMax) {
            critRate = fDifficultyCritRateMax;
        }
        return (int)critRate;
    }

    public void ClearNode(bool isCombat) {
        if (isCombat) {
            combat_count++;
        }
        node_count++;
    }
}
