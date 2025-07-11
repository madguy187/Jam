using UnityEngine;
using TMPro;
using System.Collections;

public class GoldUI : MonoBehaviour
{
    private TextMeshProUGUI goldText;
    private const string PREFIX = "Gold: ";
    private const float ANIM_DURATION = 0.2f;
    private const float SCALE_AMOUNT = 1.2f;
    
    private int currentGold = 0;
    private Vector3 originalScale;
    private Coroutine animationCoroutine;

    private void Awake()
    {
        if (goldText == null)
        {
            goldText = GetComponent<TextMeshProUGUI>();
        }
        
        if (goldText == null)
        {
            Debug.LogError("GoldUI: TextMeshProUGUI component not found!");
            return;
        }

        originalScale = goldText.transform.localScale;
    }

    private void Start()
    {
        // Initialize with current gold value
        currentGold = GoldManager.instance.GetCurrentGold();
        goldText.text = PREFIX + currentGold.ToString();
    }

    private void Update()
    {
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
        goldText.text = PREFIX + currentGold.ToString();
        
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }
        
        animationCoroutine = StartCoroutine(AnimateGoldChange(isIncrease));
    }

    private IEnumerator AnimateGoldChange(bool isIncrease)
    {
        float elapsed = 0f;
        Color originalColor = goldText.color;
        Color targetColor = isIncrease ? Color.green : Color.red;
        
        while (elapsed < ANIM_DURATION)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / ANIM_DURATION;
            
            float scaleT = Mathf.Sin(t * Mathf.PI);
            goldText.transform.localScale = Vector3.Lerp(originalScale, originalScale * SCALE_AMOUNT, scaleT);
            goldText.color = Color.Lerp(originalColor, targetColor, scaleT);
            
            yield return null;
        }
        
        goldText.transform.localScale = originalScale;
        goldText.color = originalColor;
        animationCoroutine = null;
    }
} 