using UnityEngine;
using System.Collections.Generic;

public class ProbabilityCalculator : MonoBehaviour
{
    [Header("Base Probabilities")]
    [Tooltip("Base prob for empty slots (10-60%). Higher values mean more empty slots.")]
    [SerializeField] [Range(0.1f, 0.6f)] private float baseEmptyProbability = 0.4f;

    [Tooltip("Minimum prob for each archetype present in deck (10-50%). Higher values ensure more consistent appearance of each archetype.")]
    [SerializeField] [Range(0.1f, 0.5f)] private float minArchetypeProbability = 0.2f;

    private Dictionary<SymbolType, float> currentProbabilities = new Dictionary<SymbolType, float>();
    public float EmptyProbability => GetProbabilityForSymbol(SymbolType.EMPTY);

    private void Start()
    {
        // Initialize probabilities with default values
        currentProbabilities.Clear();
        currentProbabilities[SymbolType.EMPTY] = baseEmptyProbability;
        currentProbabilities[SymbolType.HOLY] = 0f;
        currentProbabilities[SymbolType.UNDEAD] = 0f;
        currentProbabilities[SymbolType.ELF] = 0f;
    }

    public void CalculateProbabilities(Deck deck)
    {
        if (deck == null)
        {
            Debug.LogWarning("[ProbabilityCalculator] Deck is null!");
            return;
        }

        // Get unique archetypes in deck
        HashSet<eUnitArchetype> archetypes = new HashSet<eUnitArchetype>();
        Dictionary<eUnitArchetype, int> archetypeCounts = new Dictionary<eUnitArchetype, int>();
        int totalUnits = 0;

        foreach (UnitObject unit in deck)
        {
            if (unit != null && unit.unitSO != null && !unit.IsDead())
            {
                totalUnits++;
                archetypes.Add(unit.unitSO.eUnitArchetype);
                
                if (!archetypeCounts.ContainsKey(unit.unitSO.eUnitArchetype))
                    archetypeCounts[unit.unitSO.eUnitArchetype] = 0;
                archetypeCounts[unit.unitSO.eUnitArchetype]++;
            }
        }

        /*Debug.Log($"[ProbabilityCalculator] Found {archetypes.Count} archetypes in deck. Total units: {totalUnits}");
        foreach (var archetype in archetypes)
        {
            Debug.Log($"[ProbabilityCalculator] Archetype: {archetype}, Count: {archetypeCounts[archetype]}");
        }*/

        // Clear current probabilities and initialize with all possible symbols
        currentProbabilities.Clear();
        currentProbabilities[SymbolType.EMPTY] = baseEmptyProbability;
        currentProbabilities[SymbolType.HOLY] = 0f;
        currentProbabilities[SymbolType.UNDEAD] = 0f;
        currentProbabilities[SymbolType.ELF] = 0f;

        if (archetypes.Count == 0)
        {
            currentProbabilities[SymbolType.EMPTY] = 1f;
            return;
        }

        // Calculate remaining probability to distribute
        float remainingProb = 1f - baseEmptyProbability;
        float probPerArchetype = remainingProb / archetypes.Count;
        // Debug.Log($"[ProbabilityCalculator] Initial prob per archetype: {probPerArchetype:P1}");

        // Ensure minimum probability per archetype
        if (probPerArchetype < minArchetypeProbability)
        {
            float neededProb = minArchetypeProbability * archetypes.Count;
            currentProbabilities[SymbolType.EMPTY] = Mathf.Max(0.1f, 1f - neededProb);
            probPerArchetype = minArchetypeProbability;
            // Debug.Log($"[ProbabilityCalculator] Adjusted for minimum. New empty: {currentProbabilities[SymbolType.EMPTY]:P1}, New per archetype: {probPerArchetype:P1}");
        }

        // Assign probabilities based on archetype presence
        foreach (eUnitArchetype archetype in archetypes)
        {
            SymbolType symbolType = GetSymbolTypeForArchetype(archetype);
            currentProbabilities[symbolType] = probPerArchetype;
        }

        // Normalize to 1
        NormalizeProbabilities();

    }

    private void NormalizeProbabilities()
    {
        float sum = 0f;
        foreach (float prob in currentProbabilities.Values)
        {
            sum += prob;
        }

        if (Mathf.Approximately(sum, 0f)) return;

        float multiplier = 1f / sum;
        var keys = new List<SymbolType>(currentProbabilities.Keys);
        foreach (var key in keys)
        {
            currentProbabilities[key] *= multiplier;
        }
    }

    private SymbolType GetSymbolTypeForArchetype(eUnitArchetype archetype)
    {
        switch (archetype)
        {
            case eUnitArchetype.HOLY:
                return SymbolType.HOLY;
            case eUnitArchetype.UNDEAD:
                return SymbolType.UNDEAD;
            case eUnitArchetype.ELF:
                return SymbolType.ELF;
            default:
                return SymbolType.EMPTY;
        }
    }

    public SymbolType GenerateRandomSymbol()
    {
        float random = Random.value;
        float cumulativeProb = 0f;

        /// Debug.Log($"[ProbabilityCalculator] Generating random symbol. Random value: {random:F3}");
        
        foreach (var kvp in currentProbabilities)
        {
            cumulativeProb += kvp.Value;
            // Debug.Log($"[ProbabilityCalculator] Checking {kvp.Key}: prob={kvp.Value:F3}, cumulative={cumulativeProb:F3}");
            if (random <= cumulativeProb)
            {
                //Debug.Log($"[ProbabilityCalculator] Selected: {kvp.Key}");
                return kvp.Key;
            }
        }

        return SymbolType.EMPTY; 
    }

    public float GetProbabilityForSymbol(SymbolType symbolType)
    {
        return currentProbabilities.ContainsKey(symbolType) ? currentProbabilities[symbolType] : 0f;
    }
} 