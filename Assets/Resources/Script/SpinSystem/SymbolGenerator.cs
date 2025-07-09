using UnityEngine;

public enum SymbolType
{
    EMPTY = 0,
    ATTACK = 1,
    DEFENSE = 2,
    SPECIAL = 3
}

public class SymbolGenerator : MonoBehaviour
{
    public static SymbolGenerator instance;
    
    [Header("Symbol Probabilities")]
    [SerializeField] private float emptyProbability = 0.2f;    
    [SerializeField] private float attackProbability = 0.3f;   
    [SerializeField] private float defenseProbability = 0.25f; 
    [SerializeField] private float specialProbability = 0.25f; 

    void Awake()
    {
        instance = this;
    }

    public SymbolType GenerateRandomSymbol()
    {
        float roll = Random.value; 
        float currentThreshold = 0f;
        
        currentThreshold += emptyProbability;
        if (roll <= currentThreshold) {
            return SymbolType.EMPTY;
        }
        
        currentThreshold += attackProbability;
        if (roll <= currentThreshold) {
            return SymbolType.ATTACK;
        }
        
        currentThreshold += defenseProbability;
        if (roll <= currentThreshold) {
            return SymbolType.DEFENSE;
        }
        
        return SymbolType.SPECIAL;
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

    // Generate symbols based on unit type and stats
    public SymbolType[] GenerateSymbolsForUnit(UnitObject unit)
    {
        if (unit == null) return null;

        float tempEmptyProb = emptyProbability;
        float tempAttackProb = attackProbability;
        float tempDefenseProb = defenseProbability;
        float tempSpecialProb = specialProbability;

        SymbolType[] symbols = new SymbolType[9];
        for (int i = 0; i < 9; i++)
        {
            float roll = Random.value;
            float currentThreshold = 0f;

            currentThreshold += tempEmptyProb;
            if (roll <= currentThreshold)
            {
                symbols[i] = SymbolType.EMPTY;
                continue;
            }

            currentThreshold += tempAttackProb;
            if (roll <= currentThreshold)
            {
                symbols[i] = SymbolType.ATTACK;
                continue;
            }

            currentThreshold += tempDefenseProb;
            if (roll <= currentThreshold)
            {
                symbols[i] = SymbolType.DEFENSE;
                continue;
            }

            symbols[i] = SymbolType.SPECIAL;
        }

        return symbols;
    }
    
    public void SetEmptyProbability(float probability)
    {
        emptyProbability = Mathf.Clamp01(probability);
    }
    
    public void SetAttackProbability(float probability) 
    {
        attackProbability = Mathf.Clamp01(probability);
    }
    
    public void SetDefenseProbability(float probability) 
    {
        defenseProbability = Mathf.Clamp01(probability);
    }
    
    public void SetSpecialProbability(float probability) 
    {
        specialProbability = Mathf.Clamp01(probability);
    }
} 