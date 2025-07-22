using UnityEngine;
using System.Collections.Generic;

public class ProbabilityCalculator : MonoBehaviour
{
    public static ProbabilityCalculator instance { get; private set; }

    [Header("Base Probabilities")]
    [Tooltip("Base prob for empty slots (10-60%). Higher values mean more empty slots.")]
    [SerializeField] [Range(0.1f, 0.6f)] 
    private float baseEmptyProbability = 0.4f;

    [Tooltip("Minimum prob for each archetype present in deck (10-50%). Higher values ensure more consistent appearance of each archetype.")]
    [SerializeField] [Range(0.1f, 0.5f)] 
    private float minArchetypeProbability = 0.2f;

    private Dictionary<SymbolType, float> currentProbabilities = new Dictionary<SymbolType, float>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeProbabilities();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeProbabilities()
    {
        currentProbabilities.Clear();
        SetDefaultProbabilities();
    }

    private void SetDefaultProbabilities()
    {
        currentProbabilities[SymbolType.EMPTY] = baseEmptyProbability;
        currentProbabilities[SymbolType.HOLY] = 0f;
        currentProbabilities[SymbolType.UNDEAD] = 0f;
        currentProbabilities[SymbolType.ELF] = 0f;
    }

    public float GetEmptyProbability()
    {
        return GetProbabilityForSymbol(SymbolType.EMPTY);
    }

    public void CalculateProbabilities(Deck deck)
    {
        if (deck == null)
        {
            Debug.LogWarning("[ProbabilityCalculator] Deck is null!");
            return;
        }

        var (archetypes, archetypeCounts, totalUnits) = AnalyzeDeck(deck);

        /*Debug.Log($"[ProbabilityCalculator] Found {archetypes.Count} archetypes in deck. Total units: {totalUnits}");
        foreach (var archetype in archetypes)
        {
            Debug.Log($"[ProbabilityCalculator] Archetype: {archetype}, Count: {archetypeCounts[archetype]}");
        }*/

        SetDefaultProbabilities();

        if (archetypes.Count == 0)
        {
            currentProbabilities[SymbolType.EMPTY] = 1f;
            return;
        }

        DistributeProbabilities(archetypes);
        NormalizeProbabilities();
    }

    public SymbolType GenerateRandomSymbol()
    {
        float random = Random.value;
        float cumulativeProb = 0f;

        /*Debug.Log($"[ProbabilityCalculator] Generating random symbol. Random value: {random:F3}");*/
        
        foreach (var kvp in currentProbabilities)
        {
            cumulativeProb += kvp.Value;
            /*Debug.Log($"[ProbabilityCalculator] Checking {kvp.Key}: prob={kvp.Value:F3}, cumulative={cumulativeProb:F3}");*/
            if (random <= cumulativeProb)
            {
                /*Debug.Log($"[ProbabilityCalculator] Selected: {kvp.Key}");*/
                return kvp.Key;
            }
        }

        return SymbolType.EMPTY;
    }

    public float GetProbabilityForSymbol(SymbolType symbolType)
    {
        return currentProbabilities.TryGetValue(symbolType, out float probability) ? probability : 0f;
    }

    private (HashSet<eUnitArchetype> archetypes, Dictionary<eUnitArchetype, int> counts, int total) AnalyzeDeck(Deck deck)
    {
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
                {
                    archetypeCounts[unit.unitSO.eUnitArchetype] = 0;
                }
                archetypeCounts[unit.unitSO.eUnitArchetype]++;
            }
        }

        return (archetypes, archetypeCounts, totalUnits);
    }

    private void DistributeProbabilities(HashSet<eUnitArchetype> archetypes)
    {
        float remainingProb = 1f - baseEmptyProbability;
        float probPerArchetype = remainingProb / archetypes.Count;
        /*Debug.Log($"[ProbabilityCalculator] Initial prob per archetype: {probPerArchetype:P1}");*/

        if (probPerArchetype < minArchetypeProbability)
        {
            float neededProb = minArchetypeProbability * archetypes.Count;
            currentProbabilities[SymbolType.EMPTY] = Mathf.Max(0.1f, 1f - neededProb);
            probPerArchetype = minArchetypeProbability;
            /*Debug.Log($"[ProbabilityCalculator] Adjusted for minimum. New empty: {currentProbabilities[SymbolType.EMPTY]:P1}, New per archetype: {probPerArchetype:P1}");*/
        }

        foreach (eUnitArchetype archetype in archetypes)
        {
            SymbolType symbolType = GetSymbolTypeForArchetype(archetype);
            currentProbabilities[symbolType] = probPerArchetype;
        }
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

    private static SymbolType GetSymbolTypeForArchetype(eUnitArchetype archetype)
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
} 