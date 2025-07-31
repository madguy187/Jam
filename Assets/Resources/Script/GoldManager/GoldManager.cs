using UnityEngine;

public class GoldManager : MonoBehaviour
{
    public static GoldManager instance { get; private set; }
    
    [Header("Configuration")]
    [SerializeField] private GoldConfig config;
    
    [Header("Gold Settings")]
    [SerializeField] private int currentGold = 15;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            transform.parent = null; 
            DontDestroyOnLoad(gameObject);
            
            if (config == null)
            {
                Debug.LogWarning("[GoldManager] Gold configuration is missing");
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ResetGold()
    {
        currentGold = 15; // Reset to default value
        Global.DEBUG_PRINT("[GoldManager] Gold reset to default: " + currentGold);
    }

    void Start()
    {
        if (config == null)
        {
            Debug.LogWarning("[GoldManager] Gold configuration is missing");
        }
    }

    public void StartNewTurn()
    {
        CalculateInterest();
        // Add base income
        OnRoundStart(false); 
    }

    public void OnVictory()
    {
        AddGold(config.winRoundBonus);
    }

    public void CalculateInterest()
    {
        int interestAmount = Mathf.Min(currentGold / config.goldPerInterest, config.maxInterest);
        
        Global.DEBUG_PRINT($"[GoldManager] Calculate interest: Current gold={currentGold}, Interest amount={interestAmount}");
        
        if (interestAmount > 0)
        {
            AddGold(interestAmount);
        }
    }

    public void OnRoundStart(bool isVictory)
    {
        Global.DEBUG_PRINT($"[GoldManager] Adding base income: +{config.baseIncomePerRound} gold (Current: {currentGold})");
        AddGold(config.baseIncomePerRound);
        
        if (isVictory)
        {
            Global.DEBUG_PRINT($"[GoldManager] Victory bonus: +{config.winRoundBonus} gold!");
            AddGold(config.winRoundBonus);
        }
    }

    public bool SpendGold(int amount)
    {
        if (amount <= 0)
        {
            Global.DEBUG_PRINT("[GoldManager] Invalid gold amount to spend");
            return false;
        }

        if (currentGold < amount)
        {
            Global.DEBUG_PRINT($"[GoldManager] Not enough gold (Have: {currentGold}, Need: {amount})");
            return false;
        }

        currentGold -= amount;
        Global.DEBUG_PRINT($"[GoldManager] Spent {amount} gold, remaining: {currentGold}");
        return true;
    }

    public void AddGold(int amount)
    {
        if (amount <= 0)
        {
            Global.DEBUG_PRINT("[GoldManager] Invalid gold amount to add");
            return;
        }

        currentGold += amount;
        Global.DEBUG_PRINT($"[GoldManager] Added {amount} gold, new total: {currentGold}");
    }

    public int GetCurrentGold()
    {
        return currentGold;
    }

    public bool HasEnoughGold(int amount)
    {
        return currentGold >= amount;
    }

    public int GetGoldRewardForMatch(MatchType type)
    {
        if (config == null)
        {
            Debug.LogWarning("[GoldManager] Gold configuration is missing!");
            return 0;
        }

        int reward = 0;
        switch (type)
        {
            case MatchType.HORIZONTAL:
                reward = config.horizontalReward;
                break;
            case MatchType.VERTICAL:
                reward = config.verticalReward;
                break;
            case MatchType.DIAGONAL:
                reward = config.diagonalReward;
                break;
            case MatchType.ZIGZAG:
                reward = config.zigzagReward;
                break;
            case MatchType.XSHAPE:
                reward = config.xShapeReward;
                break;
            case MatchType.FULLGRID:
                reward = config.fullGridReward;
                break;
        }
        
        if (reward > 0)
        {
            Global.DEBUG_PRINT($"[GoldManager] Match reward for {type}: +{reward} gold");
        }
        return reward;
    }
} 