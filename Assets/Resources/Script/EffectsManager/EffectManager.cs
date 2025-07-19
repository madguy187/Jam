using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class EffectManager : MonoBehaviour
{
    [Header("Effect Settings")]
    [SerializeField] private float effectDuration = 0.5f;
    [SerializeField] private float scaleAmount = 1.2f;
    [SerializeField] private Color matchGlowColor = new Color(1f, 0.92f, 0.016f, 0.5f);
    
    [Header("References")]
    [SerializeField] private SlotGridUI slotGridUI;
    [SerializeField] private SlotController slotController;
    
    private bool wasSpinning = false;

    private void Awake()
    {
        if (slotGridUI == null)
        {
            slotGridUI = Object.FindAnyObjectByType<SlotGridUI>();
        }
        if (slotController == null)
        {
            slotController = Object.FindAnyObjectByType<SlotController>();
        }
    }

    private void Update()
    {
        if (wasSpinning && !slotController.GetIsSpinning())
        {
            var spinResult = slotController.GetSpinResult();
            if (spinResult != null)
            {
                PlayMatchEffects(spinResult);
            }
            wasSpinning = false;
        }
        else if (slotController.GetIsSpinning())
        {
            wasSpinning = true;
        }
    }

    public void PlayMatchEffects(SpinResult spinResult)
    {
        var matches = spinResult.GetAllMatches();
        foreach (var match in matches)
        {
            var positions = match.GetPositions();
            
            foreach (var pos in positions)
            {
                int index = pos.x * 3 + pos.y;
                var slot = slotGridUI.transform.GetChild(index).GetComponent<Image>();
                if (slot != null)
                {
                    StartCoroutine(PlaySlotEffect(slot));
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