using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SlotGridUI : MonoBehaviour
{
    [Header("Slot Images")]
    [SerializeField] private Image slot0;
    [SerializeField] private Image slot1;
    [SerializeField] private Image slot2;
    [SerializeField] private Image slot3;
    [SerializeField] private Image slot4;
    [SerializeField] private Image slot5;
    [SerializeField] private Image slot6;
    [SerializeField] private Image slot7;
    [SerializeField] private Image slot8;

    [Header("Symbol Sprites")]
    [SerializeField] private Sprite attackSprite;
    [SerializeField] private Sprite defenseSprite;
    [SerializeField] private Sprite specialSprite;
    [SerializeField] private Sprite emptySprite;

    [Header("Animation Settings")]
    [SerializeField] [Range(0.5f, 5f)] private float spinDuration = 2f;
    [SerializeField] [Range(0.1f, 1f)] private float symbolDropDelay = 0.1f;
    [SerializeField] [Range(0.6f, 0.9f)] private float finalSymbolsStartTime = 0.7f;

    private Image[] slots = new Image[9];
    private bool isSpinning = false;
    private SymbolType[,] symbolCache = new SymbolType[3, 3];

    private void Awake()
    {
        slots[0] = slot0;
        slots[1] = slot1;
        slots[2] = slot2;
        slots[3] = slot3;
        slots[4] = slot4;
        slots[5] = slot5;
        slots[6] = slot6;
        slots[7] = slot7;
        slots[8] = slot8;

        for (int row = 0; row < 3; row++)
        {
            for (int col = 0; col < 3; col++)
            {
                symbolCache[row, col] = SymbolType.EMPTY;
            }
        }
    }

    private void UpdateSlotSymbol(int row, int col, SymbolType symbolType)
    {
        symbolCache[row, col] = symbolType;
        int index = row * 3 + col;
    
        if (slots[index] != null)
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
        if (symbols.Length != 9) return;
        
        for (int i = 0; i < 9; i++)
        {
            if (slots[i] != null)
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
        
        //Global.DEBUG_PRINT($"Starting spin animation. Duration: {spinDuration}, Spinning phase: {spinningPhaseTime}");
        
        // Fast spinning phase
        while (elapsedTime < spinningPhaseTime)
        {
            // Generate random temporary symbols for spin effect
            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 3; col++)
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
    
    public bool IsSpinning()
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