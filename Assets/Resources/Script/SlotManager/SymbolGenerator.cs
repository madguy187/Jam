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
    public static SymbolGenerator instance;

    [SerializeField] private ProbabilityCalculator probabilityCalculator;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            // Ensure we have a ProbabilityCalculator
            if (probabilityCalculator == null)
            {
                // First try to find an existing one
                probabilityCalculator = GetComponent<ProbabilityCalculator>();
                
                // If none exists, create one
                if (probabilityCalculator == null)
                {
                    probabilityCalculator = gameObject.AddComponent<ProbabilityCalculator>();
                }
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        UpdateProbabilities();
    }

    public void UpdateProbabilities()
    {
        Deck currentDeck = DeckManager.instance.GetDeckByType(
            SlotController.instance.IsEnemyTurn() ? eDeckType.ENEMY : eDeckType.PLAYER
        );
        probabilityCalculator.CalculateProbabilities(currentDeck);
    }

    public SymbolType GenerateRandomSymbol()
    {
        UpdateProbabilities(); 
        return probabilityCalculator.GenerateRandomSymbol();
    }

    public static eUnitArchetype GetArchetypeForSymbol(SymbolType symbolType)
    {
        switch (symbolType)
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
    
    public void FillGridWithRandomSymbols(SlotGrid grid)
    {
        if (grid == null) return;
        
        Debug.Log("[SymbolGenerator] Starting to fill grid with random symbols");
        
        // Update probabilities based on current deck
        UpdateProbabilities();
        
        // Clear existing symbols to prevent any leftover state
        grid.ClearGrid();
        
        // Fill each position with a new random symbol
        for (int row = 0; row < 3; row++) 
        {
            for (int col = 0; col < 3; col++) 
            {
                SymbolType symbol = GenerateRandomSymbol();
                Debug.Log($"[SymbolGenerator] Setting position ({row}, {col}) to {symbol}");
                grid.SetSlot(row, col, symbol);
            }
        }

        Debug.Log("[SymbolGenerator] Finished filling grid");
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
            if (Random.value < probabilityCalculator.GetProbabilityForSymbol(SymbolType.EMPTY))
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