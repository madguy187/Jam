using System.Collections.Generic;
using Map;
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

    public NodeType nodeType { get; set; } = NodeType.Enemy;

    const int MIN_INDEX = 0;
    const int MAX_INDEX = 0;

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

    public void PopulateMobBasedOnNodeType() {
        List<UnitObject> listResult = new List<UnitObject>();

        Dictionary<eUnitTier, List<int>> mapGenerateRestrictionFront = new Dictionary<eUnitTier, List<int>>() {
            { eUnitTier.STAR_3, new List<int>() { 0, 0 } },
            { eUnitTier.STAR_2, new List<int>() { 0, 0 } },
            { eUnitTier.STAR_1, new List<int>() { 0, 0 } },
        };

        Dictionary<eUnitTier, List<int>> mapGenerateRestrictionBack = new Dictionary<eUnitTier, List<int>>() {
            { eUnitTier.STAR_3, new List<int>() { 0, 0 } },
            { eUnitTier.STAR_2, new List<int>() { 0, 0 } },
            { eUnitTier.STAR_1, new List<int>() { 0, 0 } },
        };

        switch (nodeType) {
            case NodeType.Enemy:
                mapGenerateRestrictionFront[eUnitTier.STAR_1][MIN_INDEX] = 1;
                mapGenerateRestrictionFront[eUnitTier.STAR_1][MAX_INDEX] = 2;
                mapGenerateRestrictionBack[eUnitTier.STAR_1][MIN_INDEX] = 1;
                mapGenerateRestrictionBack[eUnitTier.STAR_1][MAX_INDEX] = 2;
                mapGenerateRestrictionBack[eUnitTier.STAR_2][MIN_INDEX] = 0;
                mapGenerateRestrictionBack[eUnitTier.STAR_2][MAX_INDEX] = 1;
                break;
            case NodeType.MiniBoss:
                mapGenerateRestrictionFront[eUnitTier.STAR_1][MIN_INDEX] = 2;
                mapGenerateRestrictionFront[eUnitTier.STAR_1][MAX_INDEX] = 3;
                mapGenerateRestrictionFront[eUnitTier.STAR_2][MIN_INDEX] = 0;
                mapGenerateRestrictionFront[eUnitTier.STAR_2][MAX_INDEX] = 1;
                mapGenerateRestrictionBack[eUnitTier.STAR_1][MIN_INDEX] = 0;
                mapGenerateRestrictionBack[eUnitTier.STAR_1][MAX_INDEX] = 1;
                mapGenerateRestrictionBack[eUnitTier.STAR_2][MIN_INDEX] = 1;
                mapGenerateRestrictionBack[eUnitTier.STAR_2][MAX_INDEX] = 2;
                break;
            case NodeType.MajorBoss:
                bool IsFront = Random.Range(0.0f, 1.0f) > 0.5;
                mapGenerateRestrictionFront[eUnitTier.STAR_1][MIN_INDEX] = 0;
                mapGenerateRestrictionFront[eUnitTier.STAR_1][MAX_INDEX] = 1;
                mapGenerateRestrictionFront[eUnitTier.STAR_2][MIN_INDEX] = 1;
                mapGenerateRestrictionFront[eUnitTier.STAR_2][MAX_INDEX] = 2;
                mapGenerateRestrictionBack[eUnitTier.STAR_1][MIN_INDEX] = 0;
                mapGenerateRestrictionBack[eUnitTier.STAR_1][MAX_INDEX] = 2;
                mapGenerateRestrictionBack[eUnitTier.STAR_2][MIN_INDEX] = 0;
                mapGenerateRestrictionBack[eUnitTier.STAR_2][MAX_INDEX] = 2;
                if (IsFront) {
                    mapGenerateRestrictionFront[eUnitTier.STAR_3][MAX_INDEX] = 1;
                } else {
                    mapGenerateRestrictionBack[eUnitTier.STAR_3][MAX_INDEX] = 1;
                }
                break;
        }

        int maxFront = 3;
        foreach (KeyValuePair<eUnitTier, List<int>> pair in mapGenerateRestrictionFront) {
            eUnitTier tier = pair.Key;
            int min = pair.Value[MIN_INDEX];
            int max = pair.Value[MAX_INDEX];
            int randNum = Random.Range(min, max + 1);
            if (randNum <= 0) {
                continue;
            }

            List<string> listRandMob = ResourceManager.instance.GetRandMobByTier(tier, randNum);
            foreach (string mobName in listRandMob) {
                GameObject obj = ResourceManager.instance.GetMob(mobName);
                UnitObject unit = ResourceManager.instance.CreateUnit(obj, true);
                listResult.Add(unit);

                if (listResult.Count >= maxFront) {
                    break;
                }
            }

            if (listResult.Count >= maxFront) {
                break;
            }
        }

        for (int i = listResult.Count; i < maxFront; i++) {
            listResult.Add(null);
        }

        int maxBack = 2;
        foreach (KeyValuePair<eUnitTier, List<int>> pair in mapGenerateRestrictionBack) {
            eUnitTier tier = pair.Key;
            int min = pair.Value[MIN_INDEX];
            int max = pair.Value[MAX_INDEX];
            int randNum = Random.Range(min, max + 1);
            if (randNum <= 0) {
                continue;
            }

            List<string> listRandMob = ResourceManager.instance.GetRandMobByTier(tier, randNum);
            foreach (string mobName in listRandMob) {
                GameObject obj = ResourceManager.instance.GetMob(mobName);
                UnitObject unit = ResourceManager.instance.CreateUnit(obj, true);
                listResult.Add(unit);

                if (listResult.Count >= maxBack + maxFront) {
                    break;
                }
            }

            if (listResult.Count >= maxBack + maxFront) {
                break;
            }
        }

        for (int i = listResult.Count; i < maxBack + maxFront; i++) {
            listResult.Add(null);
        }

        Deck cEnemyDeck = DeckManager.instance.GetDeckByType(eDeckType.ENEMY);
        cEnemyDeck.DestroyAllUnit();

        for (int i = 0; i < listResult.Count; i++) {
            if (i >= maxFront + maxBack) {
                break;
            }
            if (listResult[i] == null) {
                continue;
            }

            cEnemyDeck.AddUnit(listResult[i], i);
        }
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
