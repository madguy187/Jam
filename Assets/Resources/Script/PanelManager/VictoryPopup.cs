using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class VictoryPopup : MonoBehaviour
{
    public static VictoryPopup instance;
    [Header("UI Refs")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Transform iconContainer; 
    [SerializeField] private Image iconPrefab;       
    [SerializeField] private TMP_Text headerText;
    [SerializeField] private Button returnButton;
    [SerializeField] private string mapSceneName = "Game_Map";
    [SerializeField] private float fadeTime = 0.4f;
    [Header("Icon Settings")]
    [SerializeField] private Vector2 iconSize = new Vector2(80, 80);
    private readonly List<Image> iconPool = new List<Image>();
    private static readonly Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();

        gameObject.SetActive(false);
        canvasGroup.alpha = 0;

        if (returnButton != null)
            returnButton.onClick.AddListener(OnReturnClicked);
    }

    public void Show(IEnumerable<UnitObject> playerUnits, string header = "VICTORY", string overrideSceneName = null)
    {
        ShowInternal(playerUnits, header, overrideSceneName);
    }

    public void ShowDefeat(string sceneName = "Game_MainMenu")
    {
        ShowInternal(null, "D E F E A T", sceneName);
    }

    public void ShowVictory(IEnumerable<UnitObject> playerUnits, string sceneName = "Game_MainMenu")
    {
        ShowInternal(playerUnits, "V I C T O R Y", sceneName);
    }

    // Core display logic
    private void ShowInternal(IEnumerable<UnitObject> playerUnits, string header, string overrideSceneName)
    {
        if (playerUnits == null)
        {
            playerUnits = System.Linq.Enumerable.Empty<UnitObject>();
        }

        if (headerText != null)
            headerText.text = header;

        if (!string.IsNullOrEmpty(overrideSceneName))
            mapSceneName = overrideSceneName;

        int index = 0;
        foreach (UnitObject u in playerUnits)
        {
            if (u == null) continue;

            Sprite icon;
            string cacheKey = u.unitSO != null ? u.unitSO.name : u.name;
            if (!spriteCache.TryGetValue(cacheKey, out icon))
            {
                icon = RenderUtilities.RenderUnitHeadSprite(u);
                if (icon == null) continue;
                spriteCache[cacheKey] = icon;
            }

            // Ensure pool size
            Image img;
            if (index < iconPool.Count)
            {
                img = iconPool[index];
            }
            else
            {
                img = Instantiate(iconPrefab, iconContainer);
                iconPool.Add(img);
            }

            img.gameObject.SetActive(true);
            img.sprite = icon;
            img.rectTransform.sizeDelta = iconSize;
            img.color = Color.white;
            img.enabled = true;
            index++;
        }
        
        // hide unused pooled icons
        for (int i = index; i < iconPool.Count; i++)
        {
            iconPool[i].gameObject.SetActive(false);
        }

        gameObject.SetActive(true);
        transform.SetAsLastSibling();
        StartCoroutine(FadeRoutine(0f, 1f));
    }

    IEnumerator FadeRoutine(float from, float to)
    {
        float t = 0;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(from, to, t / fadeTime);
            yield return null;
        }
        canvasGroup.alpha = to;
    }

    void OnReturnClicked()
    {
        // Simply hide popup; pooled icons are kept for reuse
        gameObject.SetActive(false);
        SceneManager.LoadScene(mapSceneName);
    }
} 