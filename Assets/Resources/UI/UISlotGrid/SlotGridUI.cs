using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SlotGridUI : MonoBehaviour
{
    [Header("Slot Images")]
    [SerializeField] private Image[] slotImages = new Image[9];
    
    [Header("Symbol Sprites")]
    [SerializeField] private Sprite attackSprite;
    [SerializeField] private Sprite defenseSprite;
    [SerializeField] private Sprite specialSprite;
    [SerializeField] private Sprite emptySprite;
    
    [Header("Animation Settings")]
    [SerializeField] [Range(1f, 10f)] private float spinDuration = 3f;
    [SerializeField] [Range(0.2f, 2f)] private float slowdownDuration = 0.5f;
    [SerializeField] [Range(1f, 10f)] private float spinSpeed = 5f;
    [SerializeField] [Range(0.2f, 2f)] private float minSpinSpeed = 0.5f;
    
    public float TotalDuration => spinDuration + slowdownDuration;
    
    private bool isSpinning = false;
    private SymbolType[] finalResults = new SymbolType[9];
    private SymbolType[,] symbolCache = new SymbolType[3, 3];
    
    void Start()
    {
        InitializeSymbolCache();
        SubscribeToEvents();
    }
    
    // Create empty symbol grid for animation
    private void InitializeSymbolCache()
    {
        for (int row = 0; row < 3; row++)
        {
            for (int col = 0; col < 3; col++)
            {
                symbolCache[row, col] = SymbolType.Empty;
            }
        }
    }
    
    private void SubscribeToEvents()
    {
        if (SpinController.instance != null)
        {
            SpinController.instance.OnGridUpdated += UpdateGridDisplay;
            UpdateGridDisplay();
        }
    }
    
    // Update the visual state of all grid slots
    public void UpdateGridDisplay()
    {
        if (SpinController.instance == null) return;
        
        SlotGrid grid = SpinController.instance.GetSlotGrid();
        
        if (grid == null) return;
        
        UpdateAllSlots(grid);
    }
    
    // Update each slot sprite based on its symbol type
    private void UpdateAllSlots(SlotGrid grid)
    {
        for (int row = 0; row < 3; row++)
        {
            for (int col = 0; col < 3; col++)
            {
                int index = row * 3 + col;
                GridSlot slot = grid.GetSlot(row, col);
                
                if (slot != null && index < slotImages.Length && slotImages[index] != null)
                {
                    SetSlotImage(slotImages[index], slot.symbolType);
                }
            }
        }
    }
    
    void SetSlotImage(Image slotImage, SymbolType symbolType)
    {
        slotImage.color = Color.white;
        switch (symbolType)
        {
            case SymbolType.Attack:
                slotImage.sprite = attackSprite;
                break;
            case SymbolType.Defense:
                slotImage.sprite = defenseSprite;
                break;
            case SymbolType.Special:
                slotImage.sprite = specialSprite;
                break;
            case SymbolType.Empty:
                slotImage.sprite = emptySprite;
                break;
        }
    }
    
    // Start the spinning animation with the given final symbol arrangement
    public void StartSpinAnimation(SymbolType[] finalResult)
    {
        if (isSpinning) return;
        
        if (finalResult.Length != 9) return;
        
        isSpinning = true;
        finalResults = finalResult;
        
        StartCoroutine(SpinAnimationCoroutine());
    }
    
    // Handle the spinning animation sequence
    private IEnumerator SpinAnimationCoroutine()
    {
        float elapsedTime = 0f;
        float currentSpeed = spinSpeed;
        float nextUpdateTime = 0f;
        
        while (elapsedTime < spinDuration)
        {
            if (Time.time >= nextUpdateTime)
            {
                UpdateSpinningSymbols();
                nextUpdateTime = Time.time + (1f / currentSpeed);
            }
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        elapsedTime = 0f;
        while (elapsedTime < slowdownDuration)
        {
            if (Time.time >= nextUpdateTime)
            {
                float t = elapsedTime / slowdownDuration;
                currentSpeed = Mathf.Lerp(spinSpeed, minSpinSpeed, t);
                
                UpdateSpinningSymbols();
                nextUpdateTime = Time.time + (1f / currentSpeed);
            }
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        DisplayFinalResults();
        isSpinning = false;
    }
    
    private void UpdateSpinningSymbols()
    {
        for (int row = 2; row > 0; row--)
        {
            for (int col = 0; col < 3; col++)
            {
                symbolCache[row, col] = symbolCache[row - 1, col];
            }
        }
        
        for (int col = 0; col < 3; col++)
        {
            int randomIndex = Random.Range(0, 3);
            symbolCache[0, col] = (SymbolType)randomIndex;
        }
        
        for (int row = 0; row < 3; row++)
        {
            for (int col = 0; col < 3; col++)
            {
                int index = row * 3 + col;
                if (slotImages[index] != null)
                {
                    SetSlotImage(slotImages[index], symbolCache[row, col]);
                }
            }
        }
    }
    
    private void DisplayFinalResults()
    {
        for (int i = 0; i < 9; i++)
        {
            if (slotImages[i] != null)
            {
                SetSlotImage(slotImages[i], finalResults[i]);
            }
        }
    }
    
    public bool IsSpinning()
    {
        return isSpinning;
    }
} 