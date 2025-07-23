using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class EffectManager : MonoBehaviour
{
    public static EffectManager instance { get; private set; }

    [Header("Effect Settings")]
    [SerializeField] private float effectDuration = 0.5f;
    [SerializeField] private float scaleAmount = 1.2f;
    [SerializeField] private Color matchGlowColor = new Color(1f, 0.92f, 0.016f, 0.5f);
    
    [Header("References")]
    [SerializeField] private SlotGridUI slotGridUI;  // Keep this as it's a UI reference
    
    private bool wasSpinning = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            transform.parent = null; 
            DontDestroyOnLoad(gameObject);
            InitializeManager();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeManager()
    {
        if (slotGridUI == null)
        {
            slotGridUI = FindObjectOfType<SlotGridUI>();
            if (slotGridUI == null)
            {
                Debug.LogError("[EffectManager] SlotGridUI reference is missing!");
            }
        }
    }

    private void Update()
    {
        if (wasSpinning && !SlotController.instance.GetIsSpinning())
        {
            var spinResult = SlotController.instance.GetSpinResult();
            if (spinResult != null)
            {
                PlayMatchEffects(spinResult);
            }
            wasSpinning = false;
        }
        else if (SlotController.instance.GetIsSpinning())
        {
            wasSpinning = true;
        }
    }

    public void PlayMatchEffects(SpinResult spinResult)
    {
        if (spinResult == null) return;

        var matches = spinResult.GetAllMatches();
        foreach (var match in matches)
        {
            var positions = match.GetPositions();
            
            foreach (var pos in positions)
            {
                int index = pos.x * 3 + pos.y;
                if (slotGridUI != null && index < slotGridUI.transform.childCount)
                {
                    var slot = slotGridUI.transform.GetChild(index).GetComponent<Image>();
                    if (slot != null)
                    {
                        StartCoroutine(PlaySlotEffect(slot));
                    }
                }
            }
        }
    }

    private IEnumerator PlaySlotEffect(Image slot)
    {
        if (slot == null) yield break;

        Color originalColor = slot.color;
        Vector3 originalScale = slot.transform.localScale;
        
        slot.color = matchGlowColor;
        
        float elapsed = 0f;
        float halfDuration = effectDuration * 0.5f;
        
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;
            slot.transform.localScale = Vector3.Lerp(originalScale, originalScale * scaleAmount, t);
            yield return null;
        }
        
        elapsed = 0f;
        
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;
            slot.transform.localScale = Vector3.Lerp(originalScale * scaleAmount, originalScale, t);
            slot.color = Color.Lerp(matchGlowColor, originalColor, t);
            yield return null;
        }
        
        slot.transform.localScale = originalScale;
        slot.color = originalColor;
    }
} 