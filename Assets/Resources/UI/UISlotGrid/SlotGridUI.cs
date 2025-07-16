using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class SlotGridUI : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private SlotConfig slotConfig;

    [Header("Slot Images")]
    [SerializeField] private Image[] slots;

    [Header("Symbol Sprites")]
    [SerializeField] private Sprite attackSprite;
    [SerializeField] private Sprite defenseSprite;
    [SerializeField] private Sprite specialSprite;
    [SerializeField] private Sprite emptySprite;

    [Header("Animation Settings")]
    [SerializeField] [Range(0.5f, 5f)] private float spinDuration = 2f;
    [SerializeField] [Range(0.1f, 1f)] private float symbolDropDelay = 0.1f;
    [SerializeField] [Range(0.6f, 0.9f)] private float finalSymbolsStartTime = 0.7f;

    private bool isSpinning = false;
    private SymbolType[,] symbolCache;

    private void Awake()
    {
        if (slotConfig == null)
        {
            Debug.LogWarning("[SlotGridUI] SlotConfig is missing");
            return;
        }

        // Initialize symbolCache with configured grid size
        symbolCache = new SymbolType[slotConfig.gridRows, slotConfig.gridColumns];
        
        // Validate slot count matches grid size
        int requiredSlots = slotConfig.TotalGridSize;
        if (slots == null || slots.Length != requiredSlots)
        {
            Debug.LogWarning($"[SlotGridUI] Number of slot images ({(slots == null ? 0 : slots.Length)}) does not match grid size ({requiredSlots})!");
            return;
        }

        // Initialize symbolCache
        for (int row = 0; row < slotConfig.gridRows; row++)
        {
            for (int col = 0; col < slotConfig.gridColumns; col++)
            {
                symbolCache[row, col] = SymbolType.EMPTY;
            }
        }
    }

    private void UpdateSlotSymbol(int row, int col, SymbolType symbolType)
    {
        symbolCache[row, col] = symbolType;
        int index = row * slotConfig.gridColumns + col;
    
        if (index < slots.Length && slots[index] != null)
        {
            slots[index].sprite = GetSpriteForSymbol(symbolType);
        }
    }

    private Sprite GetSpriteForSymbol(SymbolType symbolType)
    {
        // Map each symbol type to its corresponding sprite
        switch (symbolType)
        {
            case SymbolType.ATTACK:
                return attackSprite;
            case SymbolType.DEFENSE:
                return defenseSprite;
            case SymbolType.SPECIAL:
                return specialSprite;
            case SymbolType.EMPTY:
            default:
                return emptySprite;
        }
    }
    
    public void UpdateGrid(SymbolType[] symbols)
    {
        if (symbols.Length != slotConfig.TotalGridSize) return;
        
        for (int i = 0; i < symbols.Length; i++)
        {
            if (i < slots.Length && slots[i] != null)
            {
                slots[i].sprite = GetSpriteForSymbol(symbols[i]);
                slots[i].color = Color.white;
            }
        }
    }

    public void StartSpinAnimation(SymbolType[] finalSymbols)
    {
        if (isSpinning) return;
        isSpinning = true;
        StartCoroutine(SpinAnimationCoroutine(finalSymbols));
    }

    private IEnumerator SpinAnimationCoroutine(SymbolType[] finalSymbols)
    {
        float elapsedTime = 0f;
        float spinningPhaseTime = spinDuration * finalSymbolsStartTime;
        
        // Fast spinning phase
        while (elapsedTime < spinningPhaseTime)
        {
            // Generate random temporary symbols for spin effect
            for (int row = 0; row < slotConfig.gridRows; row++)
            {
                for (int col = 0; col < slotConfig.gridColumns; col++)
                {
                    UpdateSlotSymbol(row, col, SymbolGenerator.instance.GenerateRandomSymbol());
                }
            }
            
            yield return new WaitForSeconds(0.05f);
            elapsedTime += 0.05f;
        }
        
        // Show final symbols
        UpdateGrid(finalSymbols);
        
        // Wait for the remaining duration
        float remainingTime = spinDuration * (1 - finalSymbolsStartTime);
        yield return new WaitForSeconds(remainingTime);
        
        isSpinning = false;
        Global.DEBUG_PRINT("Spin complete");
    }
    
    public bool GetIsSpinning()
    {
        return isSpinning;
    }

    public float GetSpinDuration()
    { 
        return spinDuration; 
    }

    public float GetSymbolDropDelay()
    { 
        return symbolDropDelay; 
    }

    public float GetFinalSymbolsStartTime()
    { 
        return finalSymbolsStartTime; 
    }

    public void SetSpinDuration(float duration) 
    { 
        spinDuration = Mathf.Clamp(duration, 0.5f, 5f); 
    }

    public void SetSymbolDropDelay(float delay) 
    { 
        symbolDropDelay = Mathf.Clamp(delay, 0.1f, 1f); 
    }

    public void SetFinalSymbolsStartTime(float time) 
    { 
        finalSymbolsStartTime = Mathf.Clamp(time, 0.6f, 0.9f); 
    }
} 