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
    private const string TAG = "SymbolGenerator";
    public static SymbolGenerator instance;
    
    [Header("Symbol Probabilities")]
    [SerializeField] [Range(0f, 1f)] private float emptyProbability = 0.2f;    
    [SerializeField] [Range(0f, 1f)] private float attackProbability = 0.3f;   
    [SerializeField] [Range(0f, 1f)] private float defenseProbability = 0.25f; 
    [SerializeField] [Range(0f, 1f)] private float specialProbability = 0.25f; 

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.LogWarning("Another instance exists");
            Destroy(gameObject);
            return;
        }
        instance = this;

        ValidateProbabilities();
    }

    private void OnValidate()
    {
        ValidateProbabilities();
    }

    private void ValidateProbabilities()
    {
        float total = emptyProbability + attackProbability + defenseProbability + specialProbability;
        if (Mathf.Abs(total - 1f) > 0.01f)
        {
            Debug.LogWarning("Symbol probabilities sum != 1.0!");
        }
    }

    private SymbolType GenerateSymbolFromProbabilities(float emptyProb, float attackProb, float defenseProb)
    {
        float roll = Random.value;
        float currentThreshold = 0f;

        currentThreshold += emptyProb;
        if (roll <= currentThreshold)
        {
            return SymbolType.EMPTY;
        }

        currentThreshold += attackProb;
        if (roll <= currentThreshold)
        {
            return SymbolType.ATTACK;
        }

        currentThreshold += defenseProb;
        if (roll <= currentThreshold)
        {
            return SymbolType.DEFENSE;
        }

        return SymbolType.SPECIAL;
    }

    public SymbolType GenerateRandomSymbol()
    {
        return GenerateSymbolFromProbabilities(emptyProbability, attackProbability, defenseProbability);
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
        if (unit.IsDead()) return null;

        float tempEmptyProb = emptyProbability;
        float tempAttackProb = attackProbability;
        float tempDefenseProb = defenseProbability;
        float tempSpecialProb = specialProbability;

        // TODO: Adjust probabilities based on unit stats
        
        SymbolType[] symbols = new SymbolType[9];
        for (int i = 0; i < 9; i++)
        {
            symbols[i] = GenerateSymbolFromProbabilities(tempEmptyProb, tempAttackProb, tempDefenseProb);
        }

        return symbols;
    }
    
    public void SetEmptyProbability(float probability)
    {
        emptyProbability = Mathf.Clamp01(probability);
        ValidateProbabilities();
    }
    
    public void SetAttackProbability(float probability) 
    {
        attackProbability = Mathf.Clamp01(probability);
        ValidateProbabilities();
    }
    
    public void SetDefenseProbability(float probability) 
    {
        defenseProbability = Mathf.Clamp01(probability);
        ValidateProbabilities();
    }
    
    public void SetSpecialProbability(float probability) 
    {
        specialProbability = Mathf.Clamp01(probability);
        ValidateProbabilities();
    }
} 