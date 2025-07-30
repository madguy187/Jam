using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public enum SymbolType
{
    EMPTY = 0,
    HOLY = 1,
    UNDEAD = 2,
    ELF = 3,
    MOB = 4,
}

public class SymbolGenerator : MonoBehaviour
{
    private const int SLOT_COUNT = 9;
    public static SymbolGenerator instance { get; private set; }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            transform.parent = null;
            DontDestroyOnLoad(gameObject);
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
            SkillSlotMachine.IsEnemyTurnStatic ? eDeckType.ENEMY : eDeckType.PLAYER
        );
        ProbabilityCalculator.instance.CalculateProbabilities(currentDeck);
    }

    public SymbolType GenerateRandomSymbol()
    {
        UpdateProbabilities(); 
        return ProbabilityCalculator.instance.GenerateRandomSymbol();
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
            case SymbolType.MOB:
                return eUnitArchetype.MOB;
            default:
                return eUnitArchetype.NONE;
        }
    }
    
    public void FillGridWithRandomSymbols(SlotGrid grid)
    {
        if (grid == null) return;
        
        Global.DEBUG_PRINT("[SymbolGenerator] Starting to fill grid with random symbols");
        
        UpdateProbabilities();
        
        grid.ClearGrid();
        
        for (int row = 0; row < 3; row++) 
        {
            for (int col = 0; col < 3; col++) 
            {
                SymbolType symbol = GenerateRandomSymbol();
                Global.DEBUG_PRINT($"[SymbolGenerator] Setting position ({row}, {col}) to {symbol}");
                grid.SetSlot(row, col, symbol);
            }
        }

        Global.DEBUG_PRINT("[SymbolGenerator] Finished filling grid");
    }

    public SymbolType[] GenerateSymbolsForDeck(Deck deck)
    {
        if (deck == null ||
            deck.GetUnitByPredicate(u => u != null && !u.IsDead()).Count == 0)
        {
            Debug.LogWarning("[SymbolGenerator] Enemy deck empty returning EMPTY symbols");
            return System.Linq.Enumerable.Repeat(SymbolType.EMPTY, SLOT_COUNT).ToArray();
        }

        // Collect ONLY alive units so dead archetypes are not considered
        HashSet<UnitObject> uniqueUnits = new HashSet<UnitObject>();
        foreach (UnitObject unit in deck)
        {
            if (unit == null) continue;
            if (unit.IsDead()) continue;

            uniqueUnits.Add(unit);
        }

        SymbolType[] symbols = new SymbolType[SLOT_COUNT];
        
        int currentSlot = 0;
        foreach (UnitObject unit in uniqueUnits)
        {
            if (currentSlot >= SLOT_COUNT) break; 

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
                case eUnitArchetype.MOB:
                    symbols[currentSlot] = SymbolType.MOB;
                    break;                    
                default:
                    symbols[currentSlot] = SymbolType.EMPTY;
                    break;
            }
            currentSlot++;
        }

        for (int i = currentSlot; i < SLOT_COUNT; i++)
        {
            if (Random.value < ProbabilityCalculator.instance.GetProbabilityForSymbol(SymbolType.EMPTY))
            {
                symbols[i] = SymbolType.EMPTY;
                continue;
            }

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
                    case eUnitArchetype.MOB:
                        symbols[i] = SymbolType.MOB;
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

        for (int i = SLOT_COUNT - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            SymbolType temp = symbols[i];
            symbols[i] = symbols[randomIndex];
            symbols[randomIndex] = temp;
        }

        return symbols;
    }
} 