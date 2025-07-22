using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public enum SymbolType
{
    EMPTY = 0,
    HOLY = 1,
    UNDEAD = 2,
    ELF = 3
}

public class SymbolGenerator : MonoBehaviour
{
    public static eUnitArchetype GetArchetypeForSymbol(SymbolType symbol)
    {
        switch (symbol)
        {
            case SymbolType.HOLY:
                return eUnitArchetype.HOLY;
            case SymbolType.UNDEAD:
                return eUnitArchetype.UNDEAD;
            case SymbolType.ELF:
                return eUnitArchetype.ELF;
            default:
                return eUnitArchetype.NONE;
        }
    }

    public static SymbolGenerator instance { get; private set; }
    
    [Header("Symbol Probabilities")]
    [SerializeField] [Range(0f, 1f)] private float emptyProbability = 0.2f;    
    [SerializeField] [Range(0f, 1f)] private float holyProbability = 0.3f;   
    [SerializeField] [Range(0f, 1f)] private float undeadProbability = 0.25f; 
    [SerializeField] [Range(0f, 1f)] private float elfProbability = 0.25f; 

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            ValidateProbabilities();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnValidate()
    {
        ValidateProbabilities();
    }

    private void ValidateProbabilities()
    {
        float total = emptyProbability + holyProbability + undeadProbability + elfProbability;
        if (Mathf.Abs(total - 1f) > 0.01f)
        {
            Debug.LogWarning("Symbol probabilities sum != 1.0!");
        }
    }

    private SymbolType GenerateSymbolFromProbabilities(float emptyProb, float holyProb, float undeadProb)
    {
        float roll = Random.value;
        float currentThreshold = 0f;

        currentThreshold += emptyProb;
        if (roll <= currentThreshold)
        {
            return SymbolType.EMPTY;
        }

        currentThreshold += holyProb;
        if (roll <= currentThreshold)
        {
            return SymbolType.HOLY;
        }

        currentThreshold += undeadProb;
        if (roll <= currentThreshold)
        {
            return SymbolType.UNDEAD;
        }

        return SymbolType.ELF;
    }

    public SymbolType GenerateRandomSymbol()
    {
        return GenerateSymbolFromProbabilities(emptyProbability, holyProbability, undeadProbability);
    }
    
    public void FillGridWithRandomSymbols(SlotGrid grid)
    {
        if (grid == null) return;
        
        // Clear existing symbols to prevent any leftover state
        grid.ClearGrid();
        
        // Fill each position with a new random symbol
        for (int row = 0; row < 3; row++) 
        {
            for (int col = 0; col < 3; col++) 
            {
                grid.SetSlot(row, col, GenerateRandomSymbol());
            }
        }
    }

    // Generate symbols based on unit archetypes in deck
    public SymbolType[] GenerateSymbolsForDeck(Deck deck)
    {
        if (deck == null) return null;

        // Get all unique units from deck
        HashSet<UnitObject> uniqueUnits = new HashSet<UnitObject>();
        foreach (UnitObject unit in deck)
        {
            if (unit != null && unit.unitSO != null)
            {
                uniqueUnits.Add(unit);
            }
        }

        // Create array for 9 slots
        SymbolType[] symbols = new SymbolType[9];
        
        // First, ensure each unique unit appears once
        int currentSlot = 0;
        foreach (UnitObject unit in uniqueUnits)
        {
            if (currentSlot >= 9) break; 

            // Convert unit's archetype to symbol
            switch (unit.unitSO.eUnitArchetype)
            {
                case eUnitArchetype.HOLY:
                    symbols[currentSlot] = SymbolType.HOLY;
                    break;
                case eUnitArchetype.UNDEAD:
                    symbols[currentSlot] = SymbolType.UNDEAD;
                    break;
                case eUnitArchetype.ELF:
                    symbols[currentSlot] = SymbolType.ELF;
                    break;
                default:
                    symbols[currentSlot] = SymbolType.EMPTY;
                    break;
            }
            currentSlot++;
        }

        // Fill remaining slots randomly
        for (int i = currentSlot; i < 9; i++)
        {
            // 20% chance for empty slot
            if (Random.value < emptyProbability)
            {
                symbols[i] = SymbolType.EMPTY;
                continue;
            }

            // Pick random unit from deck for archetype
            if (uniqueUnits.Count > 0)
            {
                UnitObject randomUnit = uniqueUnits.ElementAt(Random.Range(0, uniqueUnits.Count));
                switch (randomUnit.unitSO.eUnitArchetype)
                {
                    case eUnitArchetype.HOLY:
                        symbols[i] = SymbolType.HOLY;
                        break;
                    case eUnitArchetype.UNDEAD:
                        symbols[i] = SymbolType.UNDEAD;
                        break;
                    case eUnitArchetype.ELF:
                        symbols[i] = SymbolType.ELF;
                        break;
                    default:
                        symbols[i] = SymbolType.EMPTY;
                        break;
                }
            }
            else
            {
                symbols[i] = SymbolType.EMPTY;
            }
        }

        // Shuffle the array to randomize positions
        for (int i = symbols.Length - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            SymbolType temp = symbols[i];
            symbols[i] = symbols[randomIndex];
            symbols[randomIndex] = temp;
        }

        return symbols;
    }
} 