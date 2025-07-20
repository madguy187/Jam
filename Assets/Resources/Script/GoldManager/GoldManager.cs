using UnityEngine;

public class GoldManager : MonoBehaviour
{
    public static GoldManager instance { get; private set; }
    
    [Header("Configuration")]
    [SerializeField] private GoldConfig config;
    
    [Header("Gold Settings")]
    [SerializeField] private int currentGold = 40;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }
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
        OnRoundStart(false); // Add base income
    }

    public void OnVictory()
    {
        AddGold(config.winRoundBonus); // +3 gold for winning
    }

    public void CalculateInterest()
    {
        if (config == null) return;

        // For every 10 unspent gold, gain +1 gold (max +5)
        int interestAmount = Mathf.Min(currentGold / config.goldPerInterest, config.maxInterest);
        
        Global.DEBUG_PRINT($"[GoldManager] Calculate interest: {interestAmount} gold");
        
        if (interestAmount > 0)
        {
            AddGold(interestAmount);
        }
    }

    public void OnRoundStart(bool isVictory)
    {
        if (config == null) return;

        // Base income of 5 gold
        Global.DEBUG_PRINT($"Adding base income: +{config.baseIncomePerRound} gold");
        AddGold(config.baseIncomePerRound);
        
        if (isVictory)
        {
            Global.DEBUG_PRINT($"Victory bonus: +{config.winRoundBonus} gold!");
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
            Global.DEBUG_PRINT("[GoldManager] Not enough gold");
            return false;
        }

        currentGold -= amount;
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
            Debug.LogWarning("[GoldManager] Gold configuration is missing! Please assign it in the inspector.");
            return 0;
        }

        switch (type)
        {
            case MatchType.HORIZONTAL:
                return config.horizontalReward;
            case MatchType.VERTICAL:
                return config.verticalReward;
            case MatchType.DIAGONAL:
                return config.diagonalReward;
            case MatchType.ZIGZAG:
                return config.zigzagReward;
            case MatchType.XSHAPE:
                return config.xShapeReward;
            case MatchType.FULLGRID:
                return config.fullGridReward;
            default:
                return 0;
        }
    }
} 