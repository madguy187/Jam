using System.Collections.Generic;
using UnityEngine;

public class RelicCombiner : MonoBehaviour
{
    private Dictionary<(RelicScriptableObject, RelicScriptableObject), RelicScriptableObject> combinationMap;

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

        foreach (var key in referenceRelics.Keys)
        {
            Debug.Log("Loaded relic: " + key);
        }

        void AddCombo(string nameA, string nameB, string resultName)
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
        }

        // Basic combinations
        AddCombo("IronShard", "IronShard", "SharpenedBlade");
        AddCombo("LightFeather", "LightFeather", "SkyDagger");
        AddCombo("HardenedShell", "HardenedShell", "ReflectiveCore");
        AddCombo("MagicCore", "MagicCore", "ReinforcedBattery");
        AddCombo("BlessedCharm", "BlessedCharm", "GreaterBlessing");
        AddCombo("PhantomInk", "PhantomInk", "TrueInvisibility");
        AddCombo("DemonClaw", "DemonClaw", "BloodrendClaw");

        // 2-component combos
        AddCombo("IronShard", "LightFeather", "Executioner’sEdge");
        AddCombo("IronShard", "HardenedShell", "Warplate");
        AddCombo("IronShard", "BlessedCharm", "VampiricBrand");
        AddCombo("IronShard", "MagicCore", "AssassinSigil");
        AddCombo("IronShard", "DemonClaw", "BrutalSaber");
        AddCombo("LightFeather", "BlessedCharm", "BlessedFang");
        AddCombo("LightFeather", "PhantomInk", "ShadowWings");
        AddCombo("LightFeather", "MagicCore", "PredatorFocus");
        AddCombo("HardenedShell", "MagicCore", "SteelFrame");
        AddCombo("HardenedShell", "PhantomInk", "IronVeil");
        AddCombo("HardenedShell", "DemonClaw", "CrimsonPlating");
        AddCombo("MagicCore", "BlessedCharm", "ProtectivePulse");
        AddCombo("MagicCore", "PhantomInk", "NullField");
        AddCombo("BlessedCharm", "PhantomInk", "SoulCloak");

        // Advanced 2nd-tier
        AddCombo("Executioner’sEdge", "BrutalSaber", "DaggerofJudgment");
        AddCombo("GreaterBlessing", "SoulCloak", "DivineHalo");
        AddCombo("Warplate", "SteelFrame", "TitanArmor");
        AddCombo("OverclockGauntlet", "SharpenedBlade", "StormPike");
        AddCombo("ProtectivePulse", "ReinforcedBattery", "SanctifiedCore");
        AddCombo("BlessedFang", "Executioner’sEdge", "Executioner’sBrand");
        AddCombo("PredatorFocus", "VampiricBrand", "BloodruneClaws");
        AddCombo("IronVeil", "ReflectiveCore", "CrimsonFortress");
        AddCombo("SkyDagger", "SteelFrame", "SkyhammerCannon");
        AddCombo("NullField", "GreaterBlessing", "BlessedCircuit");
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
}
