using UnityEngine;

public static class GoldConstants
{
    public const int HORIZONTAL_REWARD = 2;
    public const int DIAGONAL_REWARD = 3;
    public const int ZIGZAG_REWARD = 4;
    public const int XSHAPE_REWARD = 5;
    public const int FULL_GRID_REWARD = 10;

    public const int BASE_INCOME_PER_ROUND = 5;
    public const int WIN_ROUND_BONUS = 3;
    public const int GOLD_PER_INTEREST = 10;
    public const int MAX_INTEREST = 5;
}

public class GoldManager : MonoBehaviour
{
    public static GoldManager instance;
    
    [Header("Gold Settings")]
    [SerializeField] private int currentGold = 40;
    private int unspentGold;

    void Awake()
    {
        instance = this;
        unspentGold = currentGold;
    }

    public void UpdateUnspentGold()
    {
        unspentGold = currentGold;
        Global.DEBUG_PRINT($" [GoldManager] Updated unspent gold to {unspentGold}");
    }

    public void CalculateInterest()
    {
        int interestAmount = Mathf.Min(unspentGold / GoldConstants.GOLD_PER_INTEREST, GoldConstants.MAX_INTEREST);
        
        Global.DEBUG_PRINT($" [GoldManager] Calculate interest: {interestAmount} gold");
        
        if (interestAmount > 0)
        {
            AddGold(interestAmount);
        }
    }

    public void OnRoundStart(bool isVictory)
    {
        Global.DEBUG_PRINT($" Adding base income: +{GoldConstants.BASE_INCOME_PER_ROUND} gold");
        AddGold(GoldConstants.BASE_INCOME_PER_ROUND);
        
        if (isVictory)
        {
            Global.DEBUG_PRINT($" Victory bonus: +{GoldConstants.WIN_ROUND_BONUS} gold!");
            AddGold(GoldConstants.WIN_ROUND_BONUS);
        }
    }

    public bool SpendGold(int amount)
    {
        if (amount <= 0)
        {
            Global.DEBUG_PRINT("[GoldManager] Invalid spend amount");
            return false;
        }
        
        if (currentGold < amount)
        {
            Global.DEBUG_PRINT("[GoldManager] Not enough gold");
            return false;
        }
        
        currentGold -= amount;
        Global.DEBUG_PRINT($"[GoldManager] Spent {amount} gold. New total: {currentGold}");
        return true;
    }

    public void AddGold(int amount)
    {
        // Validate amount before adding
        if (amount <= 0)
        {
            Global.DEBUG_PRINT("[GoldManager] Invalid add amount");
            return;
        }
        
        currentGold += amount;
        Global.DEBUG_PRINT($"[GoldManager] Added {amount} gold. New total: {currentGold}");
    }

    public int GetCurrentGold()
    {
        return currentGold;
    }

    public int GetUnspentGold()
    {
        return unspentGold;
    }

    public bool HasEnoughGold(int amount)
    {
        return currentGold >= amount;
    }

    private int GetMatchReward(MatchType matchType)
    {
        switch (matchType)
        {
            case MatchType.HORIZONTAL:
                return GoldConstants.HORIZONTAL_REWARD;
            case MatchType.DIAGONAL:
                return GoldConstants.DIAGONAL_REWARD;
            case MatchType.ZIGZAG:
                return GoldConstants.ZIGZAG_REWARD;
            case MatchType.XSHAPE:
                return GoldConstants.XSHAPE_REWARD;
            case MatchType.FULLGRID:
                return GoldConstants.FULL_GRID_REWARD;
            default:
                return 0;
        }
    }
} 