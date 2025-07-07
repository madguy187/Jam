using UnityEngine;

public class SymbolGenerator : MonoBehaviour
{
    public static SymbolGenerator instance;
    
    [Header("Symbol Probabilities")]
    // PLS make sure all probabilities add up to 100%/1.0
    [SerializeField] [Range(0f, 1f)] private float attackProbability = 0.4f;   
    [SerializeField] [Range(0f, 1f)] private float defenseProbability = 0.3f;  
    [SerializeField] [Range(0f, 1f)] private float specialProbability = 0.3f; 
    
    void Awake()
    {
        instance = this;   
    }
    
    // Generates a random symbol based on the probability settings
    public SymbolType GenerateRandomSymbol()
    {
        float roll = Random.value;
        float currentThreshold = 0f;
        
        // Check if roll falls within probability range
        currentThreshold += attackProbability;
        if (roll <= currentThreshold) return SymbolType.Attack;
        
        currentThreshold += defenseProbability;
        if (roll <= currentThreshold) return SymbolType.Defense;
        
        currentThreshold += specialProbability;
        if (roll <= currentThreshold) return SymbolType.Special;
        
        return SymbolType.Attack;
    }
    
    // Fills the entire grid with random symbols
    public void FillGridWithRandomSymbols(SlotGrid grid)
    {
        if (grid == null) return;
        
        grid.ClearGrid();
        
        // Fill each slot with a random symbol
        for (int row = 0; row < 3; row++) 
        {
            for (int col = 0; col < 3; col++) 
            {
                grid.SetSlot(row, col, GenerateRandomSymbol());
            }
        }
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