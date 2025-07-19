using UnityEngine;
using System.Collections.Generic;

public enum RelicTier
{
    Basic,
    AdvancedSelf,  
    AdvancedMixed,  
    Legendary
}

[CreateAssetMenu(fileName = "RelicConfig", menuName = "Game/Relic Configuration")]
public class RelicConfig : ScriptableObject
{
    [System.Serializable]
    public struct RelicData
    {
        public string name;
        public string effect;
        public Sprite icon;
        public RelicTier tier;
        [Tooltip("For Advanced/Legendary relics, what relics are needed to create this")]
        public string[] requiredRelics;
    }

    [Header("Basic Relics")]
    public RelicData[] basicRelics;

    [Header("Advanced Relics (Self-Merges)")]
    public RelicData[] advancedSelfRelics;

    [Header("Advanced Relics (Mixed-Merges)")]
    public RelicData[] advancedMixedRelics;

    [Header("Legendary Relics")]
    public RelicData[] legendaryRelics;

    private Dictionary<string, RelicData> relicCache;

    private void OnEnable()
    {
        InitializeCache();
    }

    private void InitializeCache()
    {
        relicCache = new Dictionary<string, RelicData>();
        
        CacheRelicArray(basicRelics);
        CacheRelicArray(advancedSelfRelics);
        CacheRelicArray(advancedMixedRelics);
        CacheRelicArray(legendaryRelics);
    }

    private void CacheRelicArray(RelicData[] relics)
    {
        if (relics == null) return;
        
        foreach (var relic in relics)
        {
            if (!string.IsNullOrEmpty(relic.name))
            {
                relicCache[relic.name] = relic;
            }
        }
    }

    public RelicData? FindRelicByName(string relicName)
    {
        // Ensure cache is initialized
        if (relicCache == null)
        {
            InitializeCache();
        }

        // O(1) lookup
        if (relicCache.TryGetValue(relicName, out RelicData relic))
        {
            return relic;
        }

        return null;
    }
} 