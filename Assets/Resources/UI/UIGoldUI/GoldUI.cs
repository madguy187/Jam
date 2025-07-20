using UnityEngine;
using TMPro;
using System.Collections;

public class GoldUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI goldText;
    private const string PREFIX = "Gold: ";
    private const float ANIM_DURATION = 0.2f;
    private const float SCALE_AMOUNT = 1.2f;
    
    private int currentGold = 0;
    private Vector3 originalScale;
    private Coroutine animationCoroutine;

    private void Start()
    {
        InitializeComponents();
        
        currentGold = GoldManager.instance.GetCurrentGold();
        UpdateGoldText();
    }

    private void InitializeComponents()
    {
        if (goldText == null)
        {
            goldText = GetComponent<TextMeshProUGUI>();
            if (goldText == null)
            {
                goldText = GetComponentInChildren<TextMeshProUGUI>();
            }
        }

        originalScale = goldText.transform.localScale;
    }

    private void Update()
    {
        if (!enabled) return;
        
        if (GoldManager.instance != null)
        {
            int newGold = GoldManager.instance.GetCurrentGold();
            if (newGold != currentGold)
            {
                UpdateGoldDisplay(newGold);
            }
        }
    }

    private void UpdateGoldDisplay(int newAmount)
    {
        bool isIncrease = newAmount > currentGold;
        currentGold = newAmount;
        UpdateGoldText();
        
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }
        
        animationCoroutine = StartCoroutine(AnimateGoldChange(isIncrease));
    }

    private void UpdateGoldText()
    {
        if (goldText != null)
        {
            goldText.text = PREFIX + currentGold.ToString();
        }
    }

    private IEnumerator AnimateGoldChange(bool isIncrease)
    {
        float elapsed = 0f;
        Vector3 startScale = goldText.transform.localScale;
        Vector3 targetScale = originalScale * SCALE_AMOUNT;

        while (elapsed < ANIM_DURATION)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / ANIM_DURATION;
            
            // Scale up
            if (t <= 0.5f)
            {
                goldText.transform.localScale = Vector3.Lerp(startScale, targetScale, t * 2f);
            }
            // Scale down
            else
            {
                goldText.transform.localScale = Vector3.Lerp(targetScale, originalScale, (t - 0.5f) * 2f);
            }
            
            yield return null;
        }

        goldText.transform.localScale = originalScale;
        animationCoroutine = null;
    }

    private void OnDestroy()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }
    }
} 