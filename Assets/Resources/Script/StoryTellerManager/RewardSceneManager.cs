using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RewardSceneManager : MonoBehaviour
{
    [Header("Reward UI")]
    public Image rewardIcon;
    public TextMeshProUGUI rewardName;
    public TextMeshProUGUI rewardDesc;
    public Button claimButton;
    public TextMeshProUGUI claimButtonText;
    public float fadeDuration = 0.5f;

    [Header("Dialogue System")]
    public DialogueManager dialogueManager;
    [TextArea(2, 4)]
    public string[] dialogueLines;

    [Header("Sample Content Pools")]
    public List<RewardItem> unitPool = new();
    public List<RewardItem> relicPool = new();
    public Sprite goldIcon;
    public int minGold = 20;
    public int maxGold = 50;

    private void Start()
    {
        claimButton.interactable = false;
        claimButtonText.text = "Claim";

        dialogueLines = new string[]
        {
            "Hey there, adventurer!",
            "Here's something special for you.",
            "Make good use of it!"
        };

        LoadDummyData();
        GenerateReward();
        StartCoroutine(FadeInCanvasGroup(rewardIcon.canvasRenderer, fadeDuration));
        dialogueManager.StartDialogue(dialogueLines, () =>
        {
            claimButton.interactable = true;
        });
    }

    IEnumerator FadeInCanvasGroup(CanvasRenderer renderer, float duration)
    {
        renderer.SetAlpha(0f);
        renderer.gameObject.SetActive(true);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
            renderer.SetAlpha(alpha);
            elapsed += Time.deltaTime;
            yield return null;
        }

        renderer.SetAlpha(1f);
    }

    public void OnClaimPressed()
    {
        Debug.Log("Reward claimed!");
        claimButtonText.text = "Claimed!";
        claimButton.interactable = false;
    }

    void GenerateReward()
    {
        float roll = Random.value;
    
        if (roll < 0.33f && unitPool.Count > 0)
        {
            RewardItem unit = unitPool[Random.Range(0, unitPool.Count)];
            DisplayReward(unit);
        }
        else if (roll < 0.66f && relicPool.Count > 0)
        {
            RewardItem relic = relicPool[Random.Range(0, relicPool.Count)];
            DisplayReward(relic);
        }
        else
        {
            int goldAmount = Random.Range(minGold, maxGold + 1);
            DisplayGoldReward(goldAmount);
        }
    }

    void DisplayReward(RewardItem item)
    {
        rewardIcon.sprite = item.icon;
        rewardName.text = item.name;
        rewardDesc.text = item.description;
        claimButtonText.text = $"Claim {RewardItemTypeConverter.ToString(item.type)}";
        claimButton.onClick.RemoveAllListeners();
        claimButton.onClick.AddListener(() => OnClaimPressed());
    }
    
    void DisplayGoldReward(int amount)
    {
        rewardIcon.sprite = goldIcon;
        rewardName.text = $"{amount} Gold";
        rewardDesc.text = "Free gold! Spend it wisely.";
        claimButtonText.text = $"Claim {amount}g";
        claimButton.onClick.RemoveAllListeners();
        claimButton.onClick.AddListener(() => OnClaimPressed());
    }

    void LoadDummyData()
    {
        // Load icons from Resources folder
        Sprite knightIcon = Resources.Load<Sprite>("Sprites/game-icons.net/mounted-knight");
        Sprite monkIcon = Resources.Load<Sprite>("Sprites/game-icons.net/monk-face");
        Sprite goldIconSprite = Resources.Load<Sprite>("Sprites/game-icons.net/gold-bar");

        // Add to unit pool
        unitPool.Add(new RewardItem("Knight", "A sturdy frontline unit", RewardItemType.Unit, 0, knightIcon));
        unitPool.Add(new RewardItem("Paladin", "Heals allies over time", RewardItemType.Unit, 0, monkIcon));

        // Add to relic pool
        relicPool.Add(new RewardItem("Amulet of Speed", "Increases move speed by 20%", RewardItemType.Relic, 0, monkIcon));
        relicPool.Add(new RewardItem("Brute's Might", "Gain +2 strength", RewardItemType.Relic, 0, knightIcon));

        // Set gold icon
        goldIcon = goldIconSprite;
    }

#if UNITY_EDITOR
    [ContextMenu("Manual Refresh Reward Item")]
    public void EditorRefresh()
    {
        GenerateReward();
    }
#endif

}
