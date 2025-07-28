using System.Collections.Generic;
using UnityEngine;

public class RelicCombiner : MonoBehaviour
{
    private Dictionary<(RelicScriptableObject, RelicScriptableObject), RelicScriptableObject> combinationMap;
    private Dictionary<RelicScriptableObject, (RelicScriptableObject part1, RelicScriptableObject part2)> breakMap;

    [SerializeField] private IReadOnlyDictionary<string, RelicScriptableObject> referenceRelics;

    public void Start()
    {
        referenceRelics = ResourceManager.instance?.RelicSOMap;
        if (referenceRelics == null)
        {
            Debug.LogError("Reference relics not found in ResourceManager.");
            return;
        }
        BuildCombinationMap();
    }

    void BuildCombinationMap()
    {
        combinationMap = new Dictionary<(RelicScriptableObject, RelicScriptableObject), RelicScriptableObject>(new RelicPairComparer());
        breakMap = new Dictionary<RelicScriptableObject, (RelicScriptableObject part1, RelicScriptableObject part2)>();

        // foreach (var key in referenceRelics.Keys)
        // {
        //     Debug.Log("Loaded relic: " + key);
        // }

        void AddComboAndBreak(string nameA, string nameB, string resultName)
        {
            if (!referenceRelics.ContainsKey(nameA))
            {
                Debug.LogWarning($"[RelicCombiner::BuildCombinationMap] Missing relic from Resource Manager: {nameA}");
                return;
            }
            if (!referenceRelics.ContainsKey(nameB))
            {
                Debug.LogWarning($"[RelicCombiner::BuildCombinationMap] Missing relic from Resource Manager: {nameB}");
                return;
            }
            if (!referenceRelics.ContainsKey(resultName))
            {
                Debug.LogWarning($"[RelicCombiner::BuildCombinationMap] Missing relic from Resource Manager: {resultName}");
                return;
            }

            var a = referenceRelics[nameA];
            var b = referenceRelics[nameB];
            var result = referenceRelics[resultName];

            combinationMap[(a, b)] = result;
            combinationMap[(b, a)] = result;
            breakMap[result] = (a, b); // Piggyback on the same map for breaking down relics
        }

        // Basic combinations
        AddComboAndBreak("IronShard", "IronShard", "SharpenedBlade");
        AddComboAndBreak("LightFeather", "LightFeather", "SkyDagger");
        AddComboAndBreak("HardenedShell", "HardenedShell", "ReflectiveCore");
        AddComboAndBreak("MagicCore", "MagicCore", "ReinforcedBattery");
        AddComboAndBreak("BlessedCharm", "BlessedCharm", "GreaterBlessing");
        AddComboAndBreak("PhantomInk", "PhantomInk", "TrueInvisibility");
        AddComboAndBreak("DemonClaw", "DemonClaw", "BloodrendClaw");

        // 2-component combos
        AddComboAndBreak("IronShard", "LightFeather", "Executioner’sEdge");
        AddComboAndBreak("IronShard", "HardenedShell", "Warplate");
        AddComboAndBreak("IronShard", "BlessedCharm", "VampiricBrand");
        AddComboAndBreak("IronShard", "MagicCore", "AssassinSigil");
        AddComboAndBreak("IronShard", "DemonClaw", "BrutalSaber");
        AddComboAndBreak("LightFeather", "BlessedCharm", "BlessedFang");
        AddComboAndBreak("LightFeather", "PhantomInk", "ShadowWings");
        AddComboAndBreak("LightFeather", "MagicCore", "PredatorFocus");
        AddComboAndBreak("HardenedShell", "MagicCore", "SteelFrame");
        AddComboAndBreak("HardenedShell", "PhantomInk", "IronVeil");
        AddComboAndBreak("HardenedShell", "DemonClaw", "CrimsonPlating");
        AddComboAndBreak("MagicCore", "BlessedCharm", "ProtectivePulse");
        AddComboAndBreak("MagicCore", "PhantomInk", "NullField");
        AddComboAndBreak("BlessedCharm", "PhantomInk", "SoulCloak");

        // Advanced 2nd-tier
        AddComboAndBreak("Executioner’sEdge", "BrutalSaber", "DaggerofJudgment");
        AddComboAndBreak("GreaterBlessing", "SoulCloak", "DivineHalo");
        AddComboAndBreak("Warplate", "SteelFrame", "TitanArmor");
        AddComboAndBreak("OverclockGauntlet", "SharpenedBlade", "StormPike");
        AddComboAndBreak("ProtectivePulse", "ReinforcedBattery", "SanctifiedCore");
        AddComboAndBreak("BlessedFang", "Executioner’sEdge", "Executioner’sBrand");
        AddComboAndBreak("PredatorFocus", "VampiricBrand", "BloodruneClaws");
        AddComboAndBreak("IronVeil", "ReflectiveCore", "CrimsonFortress");
        AddComboAndBreak("SkyDagger", "SteelFrame", "SkyhammerCannon");
        AddComboAndBreak("NullField", "GreaterBlessing", "BlessedCircuit");
    }

    public bool TryCombine(RelicScriptableObject a, RelicScriptableObject b, out RelicScriptableObject result)
    {
        return combinationMap.TryGetValue((a, b), out result);
    }

    // Optional helper
    public bool CanCombine(RelicScriptableObject a, RelicScriptableObject b)
    {
        return combinationMap.ContainsKey((a, b));
    }

    private class RelicPairComparer : IEqualityComparer<(RelicScriptableObject, RelicScriptableObject)>
    {
        public bool Equals((RelicScriptableObject, RelicScriptableObject) x, (RelicScriptableObject, RelicScriptableObject) y)
        {
            return (x.Item1 == y.Item1 && x.Item2 == y.Item2) || (x.Item1 == y.Item2 && x.Item2 == y.Item1);
        }

        public int GetHashCode((RelicScriptableObject, RelicScriptableObject) pair)
        {
            int h1 = pair.Item1.GetInstanceID();
            int h2 = pair.Item2.GetInstanceID();
            return h1 ^ h2;
        }
    }

    public bool TryBreak(RelicScriptableObject relicToBreak, out RelicScriptableObject part1, out RelicScriptableObject part2)
    {
        if (breakMap != null && breakMap.TryGetValue(relicToBreak, out var parts))
        {
            part1 = parts.part1;
            part2 = parts.part2;
            return true;
        }
        part1 = null;
        part2 = null;
        return false;
    }
}
