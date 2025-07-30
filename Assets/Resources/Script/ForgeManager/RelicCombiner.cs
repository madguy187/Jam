using System.Collections.Generic;
using UnityEngine;
using TMPro; // At the top

public enum ForgeRelicRarity
{
    Advanced,
    Legendary
}

public class RelicCombiner : MonoBehaviour
{
    private Dictionary<(RelicScriptableObject, RelicScriptableObject), (RelicScriptableObject result, ForgeRelicRarity rarity)> combinationMap;
    private Dictionary<RelicScriptableObject, (RelicScriptableObject part1, RelicScriptableObject part2, ForgeRelicRarity rarity)> breakMap;

    [SerializeField] private IReadOnlyDictionary<string, RelicScriptableObject> referenceRelics;

    public TMP_Text forgeInfoTextUI;

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

    public void Start()
    {
        referenceRelics = ResourceManager.instance?.RelicSOMap;
        if (referenceRelics == null)
        {
            Debug.LogError("Reference relics not found in ResourceManager.");
            return;
        }
        BuildCombinationMap();
        DisplayForgeInfo();
    }

    void BuildCombinationMap()
    {
        combinationMap = new Dictionary<(RelicScriptableObject, RelicScriptableObject), (RelicScriptableObject result, ForgeRelicRarity rarity)>(new RelicPairComparer());
        breakMap = new Dictionary<RelicScriptableObject, (RelicScriptableObject part1, RelicScriptableObject part2, ForgeRelicRarity rarity)>();

        void AddComboAndBreak(string nameA, string nameB, string resultName, ForgeRelicRarity rarity)
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

            combinationMap[(a, b)] = (result, rarity);
            combinationMap[(b, a)] = (result, rarity);
            breakMap[result] = (a, b, rarity);
        }

        // Advanced Relics (Self-Merges)
        AddComboAndBreak("IronShard", "IronShard", "SharpenedBlade", ForgeRelicRarity.Advanced);
        AddComboAndBreak("LightFeather", "LightFeather", "SkyDagger", ForgeRelicRarity.Advanced);
        AddComboAndBreak("HardenedShell", "HardenedShell", "ReflectiveCore", ForgeRelicRarity.Advanced);
        AddComboAndBreak("MagicCore", "MagicCore", "ReinforcedBattery", ForgeRelicRarity.Advanced);
        AddComboAndBreak("BlessedCharm", "BlessedCharm", "GreaterBlessing", ForgeRelicRarity.Advanced);
        AddComboAndBreak("PhantomInk", "PhantomInk", "TrueInvisibility", ForgeRelicRarity.Advanced);
        AddComboAndBreak("DemonClaw", "DemonClaw", "BloodrendClaw", ForgeRelicRarity.Advanced);

        // Advanced Relics Mixed-Merges
        AddComboAndBreak("IronShard", "LightFeather", "Executioner’sEdge", ForgeRelicRarity.Advanced);
        AddComboAndBreak("IronShard", "HardenedShell", "Warplate", ForgeRelicRarity.Advanced);
        AddComboAndBreak("IronShard", "BlessedCharm", "VampiricBrand", ForgeRelicRarity.Advanced);
        AddComboAndBreak("IronShard", "MagicCore", "AssassinSigil", ForgeRelicRarity.Advanced);
        AddComboAndBreak("IronShard", "DemonClaw", "BrutalSaber", ForgeRelicRarity.Advanced);
        AddComboAndBreak("LightFeather", "BlessedCharm", "BlessedFang", ForgeRelicRarity.Advanced);
        AddComboAndBreak("LightFeather", "PhantomInk", "ShadowWings", ForgeRelicRarity.Advanced);
        AddComboAndBreak("LightFeather", "MagicCore", "PredatorFocus", ForgeRelicRarity.Advanced);
        AddComboAndBreak("HardenedShell", "MagicCore", "SteelFrame", ForgeRelicRarity.Advanced);
        AddComboAndBreak("HardenedShell", "PhantomInk", "IronVeil", ForgeRelicRarity.Advanced);
        AddComboAndBreak("HardenedShell", "DemonClaw", "CrimsonPlating", ForgeRelicRarity.Advanced);
        AddComboAndBreak("MagicCore", "BlessedCharm", "ProtectivePulse", ForgeRelicRarity.Advanced);
        AddComboAndBreak("MagicCore", "PhantomInk", "NullField", ForgeRelicRarity.Advanced);
        AddComboAndBreak("BlessedCharm", "PhantomInk", "SoulCloak", ForgeRelicRarity.Advanced);

        // Legendary Relics
        AddComboAndBreak("Executioner’sEdge", "BrutalSaber", "DaggerofJudgment", ForgeRelicRarity.Legendary);
        AddComboAndBreak("GreaterBlessing", "SoulCloak", "DivineHalo", ForgeRelicRarity.Legendary);
        AddComboAndBreak("Warplate", "SteelFrame", "TitanArmor", ForgeRelicRarity.Legendary);
        AddComboAndBreak("OverclockGauntlet", "SharpenedBlade", "StormPike", ForgeRelicRarity.Legendary);
        AddComboAndBreak("ProtectivePulse", "ReinforcedBattery", "SanctifiedCore", ForgeRelicRarity.Legendary);
        AddComboAndBreak("BlessedFang", "Executioner’sEdge", "Executioner’sBrand", ForgeRelicRarity.Legendary);
        AddComboAndBreak("PredatorFocus", "VampiricBrand", "BloodruneClaws", ForgeRelicRarity.Legendary);
        AddComboAndBreak("IronVeil", "ReflectiveCore", "CrimsonFortress", ForgeRelicRarity.Legendary);
        AddComboAndBreak("SkyDagger", "SteelFrame", "SkyhammerCannon", ForgeRelicRarity.Legendary);
        AddComboAndBreak("NullField", "GreaterBlessing", "BlessedCircuit", ForgeRelicRarity.Legendary);
    }

    public List<string> GetAllCombinationDescriptions()
    {
        List<string> descriptions = new List<string>();

        foreach (var kvp in combinationMap)
        {
            var (a, b) = kvp.Key;
            var (result, rarity) = kvp.Value;

            // To prevent duplicate mirrored entries (A+B and B+A), only list when a name is lexicographically smaller
            if (string.Compare(a.name, b.name) <= 0)
            {
                descriptions.Add($"{a.name} + {b.name} → {result.name} ({rarity})");
            }
        }

        return descriptions;
    }

    public List<string> GetAllBreakDescriptions()
    {
        List<string> descriptions = new List<string>();

        foreach (var kvp in breakMap)
        {
            var result = kvp.Key;
            var (part1, part2, rarity) = kvp.Value;

            descriptions.Add($"{result.name} → {part1.name} + {part2.name} ({rarity})");
        }

        return descriptions;
    }

    public bool TryCombine(RelicScriptableObject a, RelicScriptableObject b, out RelicScriptableObject result)
    {
        if (combinationMap.TryGetValue((a, b), out var data))
        {
            result = data.result;
            return true;
        }
        result = null;
        return false;
    }

    public void DisplayForgeInfo()
    {
        if (forgeInfoTextUI == null)
        {
            Global.DEBUG_PRINT("ForgeInfoTextUI not assigned.");
            return;
        }

        var sb = new System.Text.StringBuilder();

        sb.AppendLine("<b><size=24>Merge Combinations</size></b>");
        foreach (var combo in GetAllCombinationDescriptions())
            sb.AppendLine("• " + combo);

        sb.AppendLine(); // Spacer

        sb.AppendLine("<b><size=24>Break Combinations</size></b>");
        foreach (var br in GetAllBreakDescriptions())
            sb.AppendLine("• " + br);

        forgeInfoTextUI.text = sb.ToString();
    }

    public ForgeRelicRarity? GetCombineRarity(RelicScriptableObject a, RelicScriptableObject b)
    {
        if (combinationMap.TryGetValue((a, b), out var data))
            return data.rarity;
        return null;
    }

    // Optional helper
    public bool CanCombine(RelicScriptableObject a, RelicScriptableObject b)
    {
        return combinationMap.ContainsKey((a, b));
    }

    public int GetCombineCost(RelicScriptableObject a, RelicScriptableObject b)
    {
        var rarity = GetCombineRarity(a, b);
        return rarity switch
        {
            ForgeRelicRarity.Advanced => 5,
            ForgeRelicRarity.Legendary => 10,
            _ => 0
        };
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

    public ForgeRelicRarity? GetBreakRarity(RelicScriptableObject relic)
    {
        if (breakMap.TryGetValue(relic, out var data))
            return data.rarity;
        return null;
    }

    public int GetBreakCost(RelicScriptableObject relic)
    {
        var rarity = GetBreakRarity(relic);
        return rarity switch
        {
            ForgeRelicRarity.Advanced => 5,
            ForgeRelicRarity.Legendary => 10,
            _ => 0
        };
    }
}
