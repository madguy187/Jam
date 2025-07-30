using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ProbabilityCalculator : MonoBehaviour
{
    public static ProbabilityCalculator instance { get; private set; }

    [Header("Base Probabilities")]
    [Tooltip("Base prob for empty slots (10-60%). Higher values mean more empty slots.")]
    [SerializeField] [Range(0f, 0.6f)] 
    private float baseEmptyProbability = 0.3f; 

    public void SetEmptyProbability(float value)
    {
        baseEmptyProbability = Mathf.Clamp(value, 0f, 0.6f);
    }

    [Tooltip("Minimum prob for each archetype present in deck (10-50%). Higher values ensure more consistent appearance of each archetype.")]
    [SerializeField] [Range(0.1f, 0.5f)] 
    private float minArchetypeProbability = 0.2f;

    private Dictionary<SymbolType, float> currentProbabilities = new Dictionary<SymbolType, float>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            transform.parent = null; 
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
        SetDefaultProbabilities(baseEmptyProbability);
    }

    private void SetDefaultProbabilities(float emptyProb)
    {
        currentProbabilities[SymbolType.EMPTY] = emptyProb;
        currentProbabilities[SymbolType.HOLY] = 0f;
        currentProbabilities[SymbolType.UNDEAD] = 0f;
        currentProbabilities[SymbolType.ELF] = 0f;
        currentProbabilities[SymbolType.MOB] = 0f;
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

        // Always allow empty symbols, regardless of archetype diversity
        float emptyProbToUse = baseEmptyProbability;

        SetDefaultProbabilities(emptyProbToUse);

        if (archetypes.Count == 0)
        {
            // No units → 100% empty
            currentProbabilities[SymbolType.EMPTY] = 1f;
            return;
        }

        DistributeProbabilities(archetypes, emptyProbToUse);
        NormalizeProbabilities();
    }

    public SymbolType GenerateRandomSymbol()
    {
        float random = Random.value;
        float cumulativeProb = 0f;

        /*Global.DEBUG_PRINT($"[ProbabilityCalculator] Generating random symbol. Random value: {random:F3}");*/
        
        foreach (var kvp in currentProbabilities)
        {
            if (kvp.Value <= 0f)
                // skip symbols with zero probability
                continue; 

            cumulativeProb += kvp.Value;
            if (random <= cumulativeProb)
            {
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

    private void DistributeProbabilities(HashSet<eUnitArchetype> archetypes, float emptyProb)
    {
        // Calculate remaining probability after accounting for empties
        float remainingProb = Mathf.Max(0f, 1f - emptyProb);
        float probPerArchetype = (archetypes.Count > 0) ? remainingProb / archetypes.Count : 0f;

        // Ensure each archetype gets at least the minimum probability
        if (probPerArchetype < minArchetypeProbability)
        {
            float neededProb = minArchetypeProbability * archetypes.Count;

            // Push down empty probability but never below 10%
            currentProbabilities[SymbolType.EMPTY] = Mathf.Max(0.1f, 1f - neededProb);
            remainingProb = 1f - currentProbabilities[SymbolType.EMPTY];
            probPerArchetype = remainingProb / archetypes.Count;
        }

        // Assign probabilities to each archetype symbol
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

        // Ensure empty probability is exactly zero if baseEmptyProbability is zero
        if (Mathf.Approximately(baseEmptyProbability, 0f))
        {
            currentProbabilities[SymbolType.EMPTY] = 0f;
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
            case eUnitArchetype.MOB:
                return SymbolType.MOB;
            default:
                return SymbolType.EMPTY;
        }
    }
} 