using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class SlotGridUI : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private SlotConfig slotConfig;

    [Header("Slot Images")]
    [SerializeField] private Image[] slots;
    [SerializeField] private TextMeshProUGUI[] debugTexts; // Add debug text for each slot

    [Header("Symbol Sprites")]
    [SerializeField] private Sprite holySprite;
    [SerializeField] private Sprite undeadSprite;
    [SerializeField] private Sprite elfSprite;
    [SerializeField] private Sprite emptySprite;

    [Header("Debug Settings")]
    [SerializeField] private bool showDebugInfo = true;
    [SerializeField] private Color holyColor = Color.yellow;
    [SerializeField] private Color undeadColor = new Color(0.5f, 0f, 0.5f, 1f); // Purple (R=0.5, G=0, B=0.5)
    [SerializeField] private Color elfColor = Color.green;

    [Header("Animation Settings")]
    [SerializeField] [Range(0.5f, 5f)] private float spinDuration = 2.0f; 
     // Enemy spins faster
    [SerializeField] [Range(0.5f, 5f)] private float enemySpinDuration = 0.5f; 
    [SerializeField] [Range(0.1f, 1f)] private float symbolDropDelay = 0.1f;
    [SerializeField] [Range(0.6f, 0.9f)] private float finalSymbolsStartTime = 0.7f;

    private bool isSpinning = false;
    private SymbolType[,] symbolCache;
    private Dictionary<UnitObject, RenderTexture> previewCache = new Dictionary<UnitObject, RenderTexture>();
    private List<GameObject> previewCameras = new List<GameObject>();

    private void OnDestroy()
    {
        // Cleanup preview textures and cameras
        foreach (var rt in previewCache.Values)
        {
            if (rt != null)
                rt.Release();
        }
        foreach (var cam in previewCameras)
        {
            if (cam != null)
                Destroy(cam);
        }
        previewCache.Clear();
        previewCameras.Clear();
    }

    // Return both RenderTexture and camera GameObject
    private RenderTexture RenderUnitToTexture(UnitObject unit)
    {
        // Create a preview layer
        int previewLayer = 31;

        // Store original layer
        int originalLayer = unit.gameObject.layer;

        // Set unit and all children to preview layer
        SetLayerRecursively(unit.gameObject, previewLayer);

        GameObject camObj = new GameObject("UnitPreviewCamera");
        previewCameras.Add(camObj); // Track for cleanup

        Camera cam = camObj.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.Color;
        cam.backgroundColor = Color.clear;
        cam.orthographic = true;

        RenderTexture rt = new RenderTexture(256, 256, 16);
        cam.targetTexture = rt;

        cam.transform.position = unit.transform.position + new Vector3(0, 0, -10);
        cam.orthographicSize = 1.5f;

        // Only render the preview layer
        cam.cullingMask = 1 << previewLayer;

        cam.Render();

        // Restore original layer
        SetLayerRecursively(unit.gameObject, originalLayer);

        cam.enabled = false;
        camObj.SetActive(false);

        return rt;
    }

    // Helper to set layer recursively
    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }

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

        // Initialize debug texts if needed
        if (debugTexts == null || debugTexts.Length != requiredSlots)
        {
            debugTexts = new TextMeshProUGUI[requiredSlots];
            for (int i = 0; i < requiredSlots; i++)
            {
                // Create debug text objects
                GameObject textObj = new GameObject($"DebugText_{i}");
                textObj.transform.SetParent(slots[i].transform);
                TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
                tmp.fontSize = 12;
                tmp.alignment = TextAlignmentOptions.Center;
                RectTransform rt = tmp.GetComponent<RectTransform>();
                rt.anchoredPosition = Vector2.zero;
                rt.sizeDelta = slots[i].rectTransform.sizeDelta;
                debugTexts[i] = tmp;
            }
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
            // Get current deck
            Deck currentDeck = DeckManager.instance.GetDeckByType(
                SlotController.instance.IsEnemyTurn() ? eDeckType.ENEMY : eDeckType.PLAYER
            );

            if (currentDeck != null && symbolType != SymbolType.EMPTY)
            {
                // Find a unit of matching archetype
                UnitObject matchingUnit = null;
                foreach (UnitObject unit in currentDeck)
                {
                    if (unit != null && unit.unitSO != null)
                    {
                        eUnitArchetype unitArchetype = unit.unitSO.eUnitArchetype;
                        if (SymbolGenerator.GetArchetypeForSymbol(symbolType) == unitArchetype)
                        {
                            matchingUnit = unit;
                            break;
                        }
                    }
                }

                // If we found a matching unit, use its preview
                if (matchingUnit != null)
                {
                    if (!previewCache.ContainsKey(matchingUnit))
                    {
                        previewCache[matchingUnit] = RenderUnitToTexture(matchingUnit);
                    }

                    RenderTexture rt = previewCache[matchingUnit];
                    RenderTexture.active = rt;
                    Texture2D tex = new Texture2D(rt.width, rt.height);
                    tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
                    tex.Apply();
                    RenderTexture.active = null;

                    slots[index].sprite = Sprite.Create(tex, 
                        new Rect(0, 0, tex.width, tex.height), 
                        new Vector2(0.5f, 0.5f));
                    slots[index].color = Color.white;
                }
            }
            else
            {
                // Empty slot
                slots[index].sprite = null;
                slots[index].color = new Color(1, 1, 1, 0.2f); // Semi-transparent white
            }

            // Update debug text
            if (showDebugInfo && debugTexts[index] != null)
            {
                debugTexts[index].text = symbolType.ToString();
                debugTexts[index].gameObject.SetActive(true);
            }
            else if (debugTexts[index] != null)
            {
                debugTexts[index].gameObject.SetActive(false);
            }
        }
    }

    private Sprite GetSpriteForSymbol(SymbolType symbolType)
    {
        switch (symbolType)
        {
            case SymbolType.HOLY:
                return holySprite;
            case SymbolType.UNDEAD:
                return undeadSprite;
            case SymbolType.ELF:
                return elfSprite;
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
            int row = i / slotConfig.gridColumns;
            int col = i % slotConfig.gridColumns;
            UpdateSlotSymbol(row, col, symbols[i]);
        }
    }

    public void StartSpinAnimation(SymbolType[] finalSymbols, float customDuration = -1)
    {
        if (isSpinning) return;
        isSpinning = true;

        // Use custom duration if provided, otherwise use default
        float duration = customDuration > 0 ? customDuration : spinDuration;
        StartCoroutine(SpinAnimationCoroutine(finalSymbols, duration));
    }

    private IEnumerator SpinAnimationCoroutine(SymbolType[] finalSymbols, float duration)
    {
        float elapsedTime = 0f;
        float spinningPhaseTime = duration * finalSymbolsStartTime;
        
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
        float remainingTime = duration * (1 - finalSymbolsStartTime);
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

    public float GetEnemySpinDuration()
    {
        return enemySpinDuration;
    }

    public void SetEnemySpinDuration(float duration)
    {
        enemySpinDuration = Mathf.Clamp(duration, 0.5f, 5f);
    }

    public void SetShowDebugInfo(bool show)
    {
        showDebugInfo = show;
        if (debugTexts != null)
        {
            foreach (var text in debugTexts)
            {
                if (text != null)
                {
                    text.gameObject.SetActive(show);
                }
            }
        }
    }
} 