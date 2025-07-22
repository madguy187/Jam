using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class SlotGridUI : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private SlotConfig slotConfig;
    [SerializeField] private Image[] slots;
    [SerializeField] private bool showDebugInfo = true;

    [Header("Debug Visualization")]
    [SerializeField] [TextArea(5,10)] 
    private string debugGridState = "";

    [Header("Animation Settings")]
    [SerializeField] [Range(0.5f, 5f)] private float spinDuration = 2.0f;
    [SerializeField] [Range(0.5f, 5f)] private float enemySpinDuration = 0.5f;
    [SerializeField] [Range(0.1f, 1f)] private float symbolDropDelay = 0.1f;
    [SerializeField] [Range(0.6f, 0.9f)] private float finalSymbolsStartTime = 0.7f;

    private bool isSpinning;

    // just fore debugging purposes, can ignore
    private void OnEnable()
    {
        SlotController.OnMatchesProcessed += UpdateDebugVisualization;
    }
    
    private void OnDisable()
    {
        SlotController.OnMatchesProcessed -= UpdateDebugVisualization;
    }

    // - Public Functions
    public void UpdateGrid(SymbolType[] symbols)
    {
        if (symbols.Length != slotConfig.TotalGridSize)
        {
            return;
        }
        
        for (int i = 0; i < symbols.Length; i++)
        {
            int row = i / slotConfig.gridColumns;
            int col = i % slotConfig.gridColumns;
            UpdateSlotSymbol(row, col, symbols[i]);
        }
    }

    public void StartSpinAnimation(SymbolType[] finalSymbols, float customDuration = -1)
    {
        if (isSpinning)
        {
            return;
        }

        isSpinning = true;
        float duration = customDuration > 0 ? customDuration : spinDuration;
        StartCoroutine(SpinAnimationCoroutine(finalSymbols, duration));
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

    public float GetEnemySpinDuration()
    {
        return enemySpinDuration;
    }

    public void SetSpinDuration(float duration)
    {
        spinDuration = Mathf.Clamp(duration, 0.5f, 5f);
    }

    public void SetEnemySpinDuration(float duration)
    {
        enemySpinDuration = Mathf.Clamp(duration, 0.5f, 5f);
    }

    public void SetSymbolDropDelay(float delay)
    {
        symbolDropDelay = Mathf.Clamp(delay, 0.1f, 1f);
    }

    public void SetFinalSymbolsStartTime(float time)
    {
        finalSymbolsStartTime = Mathf.Clamp(time, 0.6f, 0.9f);
    }

    public void SetShowDebugInfo(bool show)
    {
        showDebugInfo = show;
    }

    public Image[] GetSlots()
    {
        return slots;
    }

    // - Private Functions
    private void UpdateSlotSymbol(int row, int col, SymbolType symbolType)
    {
        int index = row * 3 + col;
        if (index >= slots.Length || slots[index] == null)
        {
            return;
        }

        Deck currentDeck = DeckManager.instance.GetDeckByType(
            SlotController.instance.IsEnemyTurn() ? eDeckType.ENEMY : eDeckType.PLAYER
        );

        if (currentDeck == null || symbolType == SymbolType.EMPTY)
        {
            SetEmptySlot(index);
            return;
        }

        UnitObject matchingUnit = FindMatchingUnit(currentDeck, symbolType);
        if (matchingUnit != null)
        {
            RenderUnitToSlot(matchingUnit, index);
        }
        else
        {
            SetEmptySlot(index);
        }
    }

    private UnitObject FindMatchingUnit(Deck deck, SymbolType symbolType)
    {
        foreach (UnitObject unit in deck)
        {
            if (unit != null && unit.unitSO != null)
            {
                eUnitArchetype unitArchetype = unit.unitSO.eUnitArchetype;
                if ((symbolType == SymbolType.HOLY && unitArchetype == eUnitArchetype.HOLY) ||
                    (symbolType == SymbolType.UNDEAD && unitArchetype == eUnitArchetype.UNDEAD) ||
                    (symbolType == SymbolType.ELF && unitArchetype == eUnitArchetype.ELF))
                {
                    return unit;
                }
            }
        }
        return null;
    }

    private void RenderUnitToSlot(UnitObject unit, int slotIndex)
    {
        var (renderTexture, cameraObj) = RenderUnitToTexture(unit);
        
        Texture2D tex = new Texture2D(renderTexture.width, renderTexture.height);
        RenderTexture.active = renderTexture;
        tex.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        tex.Apply();
        
        slots[slotIndex].sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        slots[slotIndex].color = Color.white;

        RenderTexture.active = null;
        Destroy(cameraObj);
        renderTexture.Release();
    }

    private void SetEmptySlot(int index)
    {
        slots[index].sprite = null;
        slots[index].color = new Color(1, 1, 1, 0.2f);
    }

    private (RenderTexture, GameObject) RenderUnitToTexture(UnitObject unit)
    {
        int previewLayer = 31;
        int originalLayer = unit.gameObject.layer;
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
        cam.cullingMask = 1 << previewLayer;
        cam.Render();

        SetLayerRecursively(unit.gameObject, originalLayer);
        cam.enabled = false;
        camObj.SetActive(false);

        return (rt, camObj);
    }

    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }

    private IEnumerator SpinAnimationCoroutine(SymbolType[] finalSymbols, float duration)
    {
        float elapsedTime = 0f;
        float spinningPhaseTime = duration * finalSymbolsStartTime;
        
        while (elapsedTime < spinningPhaseTime)
        {
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
        
        UpdateGrid(finalSymbols);
        
        float remainingTime = duration * (1 - finalSymbolsStartTime);
        yield return new WaitForSeconds(remainingTime);
        
        isSpinning = false;
    }

    private void UpdateDebugVisualization()
    {
        if (showDebugInfo)
        {
            LogSpinResult(SlotController.instance.GetSpinResult());
        }
    }

    // Just a debug function , can ignore it    
    private void LogSpinResult(SpinResult result)
    {
        if (!showDebugInfo)
        {
            return;
        }

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        
        bool isEnemyTurn = SlotController.instance.IsEnemyTurn();
        Deck currentDeck = DeckManager.instance.GetDeckByType(
            isEnemyTurn ? eDeckType.ENEMY : eDeckType.PLAYER
        );

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

        if (result != null && result.GetAllMatches().Count > 0)
        {
            sb.AppendLine("\nMatches Found:");
            foreach (Match match in result.GetAllMatches())
            {
                sb.AppendLine($"- {match.GetMatchType()} match for {match.GetSymbol()} (Archetype: {match.GetArchetype()})");
                sb.AppendLine($"  Positions: {string.Join(", ", match.GetReadablePositions())}");
            }
        }

        debugGridState = sb.ToString();
        #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        #endif
    }
} 