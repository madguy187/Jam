using UnityEngine;

public class GoldManager : MonoBehaviour
{
    public static GoldManager instance;
    
    [Header("Configuration")]
    [SerializeField] private GoldConfig config;
    
    [Header("Gold Settings")]
    [SerializeField] private int currentGold = 40;

    void Awake()
    {
        instance = this;
        
        if (config == null)
        {
            Debug.LogWarning("[GoldManager] Gold configuration is missing");
        }
    }

    public void CalculateInterest()
    {
        if (config == null) return;

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
} 