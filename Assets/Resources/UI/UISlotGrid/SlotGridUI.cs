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

    [Header("Debug Settings")]
    [Tooltip("Show debug information in console after spins")]
    [SerializeField] private bool showDebugInfo = true;

    [Header("Debug Visualization")]
    [SerializeField] [TextArea(5,10)] 
    private string debugGridState = "Current Grid State:\n-------------\n| - - - |\n| - - - |\n| - - - |\n-------------\n\nCurrent Deck: None";

    [Header("Animation Settings")]
    [SerializeField] [Range(0.5f, 5f)] private float spinDuration = 2.0f;
    // Enemy spins faster
    [SerializeField] [Range(0.5f, 5f)] private float enemySpinDuration = 0.5f;
    [SerializeField] [Range(0.1f, 1f)] private float symbolDropDelay = 0.1f;
    [SerializeField] [Range(0.6f, 0.9f)] private float finalSymbolsStartTime = 0.7f;

    private bool isSpinning = false;
    private float spinTimer = 0f;
    private float nextSymbolDropTime = 0f;
    private SymbolType[,] currentSymbols;
    private SymbolType[,] finalSymbols;

    // Return both RenderTexture and camera GameObject
    (RenderTexture, GameObject) RenderUnitToTexture(UnitObject unit)
    {
        // Create a preview layer
        int previewLayer = 31;

        // Store original layer
        int originalLayer = unit.gameObject.layer;

        // Set unit and all children to preview layer
        SetLayerRecursively(unit.gameObject, previewLayer);

        GameObject camObj = new GameObject("UnitPreviewCamera");
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

        return (rt, camObj);
    }

    // Helper to set layer recursively
    void SetLayerRecursively(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }

    private void UpdateSlotSymbol(int row, int col, SymbolType symbolType)
    {
        int index = row * 3 + col;
        if (index < slots.Length && slots[index] != null)
        {
            Deck currentDeck = DeckManager.instance.GetDeckByType(
                SlotController.instance.IsEnemyTurn() ? eDeckType.ENEMY : eDeckType.PLAYER
            );

            if (currentDeck != null && symbolType != SymbolType.EMPTY)
            {
                UnitObject matchingUnit = null;
                foreach (UnitObject unit in currentDeck)
                {
                    if (unit != null && unit.unitSO != null)
                    {
                        eUnitArchetype unitArchetype = unit.unitSO.eUnitArchetype;
                        if ((symbolType == SymbolType.HOLY && unitArchetype == eUnitArchetype.HOLY) ||
                            (symbolType == SymbolType.UNDEAD && unitArchetype == eUnitArchetype.UNDEAD) ||
                            (symbolType == SymbolType.ELF && unitArchetype == eUnitArchetype.ELF))
                        {
                            matchingUnit = unit;
                            break;
                        }
                    }
                }

                if (matchingUnit != null)
                {
                    // Render unit to texture
                    var (renderTexture, cameraObj) = RenderUnitToTexture(matchingUnit);
                    
                    // Create sprite from render texture
                    Texture2D tex = new Texture2D(renderTexture.width, renderTexture.height);
                    RenderTexture.active = renderTexture;
                    tex.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
                    tex.Apply();
                    
                    // Create and assign sprite
                    slots[index].sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                    slots[index].color = Color.white;

                    // Cleanup
                    RenderTexture.active = null;
                    Destroy(cameraObj);
                    renderTexture.Release();
                }
                else
                {
                    // Empty slot
                    slots[index].sprite = null;
                    slots[index].color = new Color(1, 1, 1, 0.2f);
                }
            }
            else
            {
                // Empty slot
                slots[index].sprite = null;
                slots[index].color = new Color(1, 1, 1, 0.2f);
            }
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

    private void LogSpinResult(SpinResult result)
    {
        if (!showDebugInfo) return;

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        
        // Get current deck info
        bool isEnemyTurn = SlotController.instance.IsEnemyTurn();
        Deck currentDeck = DeckManager.instance.GetDeckByType(
            isEnemyTurn ? eDeckType.ENEMY : eDeckType.PLAYER
        );

        // Add deck info
        if (currentDeck != null)
        {
            HashSet<eUnitArchetype> archetypes = new HashSet<eUnitArchetype>();
            foreach (UnitObject unit in currentDeck)
            {
                if (unit != null && unit.unitSO != null)
                {
                    archetypes.Add(unit.unitSO.eUnitArchetype);
                }
            }
            sb.AppendLine($"Current Deck ({(isEnemyTurn ? "Enemy" : "Player")}): {string.Join(", ", archetypes)}");
        }

        // Add matches info
        if (result != null && result.GetAllMatches().Count > 0)
        {
            sb.AppendLine("\nMatches Found:");
            foreach (Match match in result.GetAllMatches())
            {
                sb.AppendLine($"- {match.GetMatchType()} match for {match.GetSymbol()} (Archetype: {match.GetArchetype()})");
                sb.AppendLine($"  Positions: {string.Join(", ", match.GetReadablePositions())}");
            }
        }

        // Update inspector visualization
        debugGridState = sb.ToString();
        #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        #endif
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
    }

    // Subscribe to match processing events
    private void OnEnable()
    {
        SlotController.OnMatchesProcessed += UpdateDebugVisualization;
    }

    private void OnDisable()
    {
        SlotController.OnMatchesProcessed -= UpdateDebugVisualization;
    }

    private void UpdateDebugVisualization()
    {
        if (showDebugInfo)
        {
            LogSpinResult(SlotController.instance.GetSpinResult());
        }
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
    }

    public Image[] GetSlots() => slots;

    private void Update() { }
} 