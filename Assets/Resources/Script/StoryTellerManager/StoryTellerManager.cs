using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StoryTellerManager : MonoBehaviour
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
    private string[] dialogueLines;
    private List<string[]> dialogueSequences = new List<string[]>();

    [Header("Sample Content Pools")]
    public Sprite goldIcon;
    public int minGold = 5;
    public int maxGold = 20;

    private MockItemType finalMockItemType;
    private UnitObject finalUnitObject;
    private RelicScriptableObject finalRelicObject;
    private int finalGoldAmount;

    private void Start()
    {
        claimButton.interactable = false;
        claimButtonText.text = "Claim";
        goldIcon = Resources.Load<Sprite>("Asset/Admurin's Pixel Items/PixelItems/General/Singles/122_Ingot_Gold");
        finalGoldAmount = 0;
        finalMockItemType = MockItemType.None;

        LoadDialogueOptions();
        dialogueLines = dialogueSequences[Random.Range(0, dialogueSequences.Count)];

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
        if (ItemTracker.Instance == null)
        {
            Global.DEBUG_PRINT("[StoryTellerManager::OnClaimPressed] ItemTracker instance is null!");
        } else {
            // For unit or relic
            if (finalMockItemType != MockItemType.None) {
                if (finalMockItemType == MockItemType.Unit) {
                    ItemTracker.Instance.AddItem(TrackerType.BagContainer, finalMockItemType, new MockInventoryItem(finalUnitObject));
                    Global.DEBUG_PRINT($"[StoryTellerManager::OnClaimPressed] Added unit to inventory.");
                } else {
                    ItemTracker.Instance.AddItem(TrackerType.BagContainer, finalMockItemType, new MockInventoryItem(finalRelicObject));
                    Global.DEBUG_PRINT($"[StoryTellerManager::OnClaimPressed] Added relic to inventory.");
                }
            // For gold
            } else {
                GoldManager.instance.AddGold(finalGoldAmount);
            }
        }
        claimButtonText.text = "Claimed!";
        claimButton.interactable = false;
        StartCoroutine(DelayedLoadScene());

    }

    private IEnumerator DelayedLoadScene()
    {
        // Delay 1 second before loading the next scene
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene("Game_Map");
    }

    void GenerateReward()
    {
        float roll = Random.value;

        var resourceManager = ResourceManager.instance;
        if (resourceManager == null) {
            Global.DEBUG_PRINT("[StoryTellerManager::GetRandomItem] ResourceManager instance is null!");
            return;
        }
    
        if (roll < 0.33f)
        {
            var relicSO = resourceManager.Debug_RandRelic();
            RewardItem relicReward = new RewardItem(
                relicSO.GetRelicName(),
                relicSO.GetRelicDescription(),
                RewardItemType.Relic,
                relicSO.GetRelicCost(),
                relicSO.GetRelicSprite()
            );
            finalMockItemType = MockItemType.Relic;
            finalRelicObject = relicSO;
            DisplayReward(relicReward);
        }
        else if (roll < 0.66f)
        {
            string unitKey = resourceManager.Debug_RandUnit();
            GameObject unitPrefab = resourceManager.GetUnit(unitKey);
            if (unitPrefab == null) {
                Global.DEBUG_PRINT($"[StoryTellerManager::GetRandomItem] Unit prefab not found for key: {unitKey}");
                return;
            }
            // Instantiate unit temporarily in the scene
            GameObject unitInstance = GameObject.Instantiate(unitPrefab);
            unitInstance.SetActive(true);
            UnitObject unitObj = unitInstance.GetComponent<UnitObject>();
            var so = unitObj.unitSO;
            finalUnitObject = unitObj;
            Sprite unitIcon = GetUnitSprite(unitInstance);
            RewardItem unitReward = new RewardItem(
                so.GetUnitName(),
                so.GetUnitDescription(),
                RewardItemType.Unit,
                so.GetUnitCost(),
                unitIcon
            );
            finalMockItemType = MockItemType.Unit;
            DisplayReward(unitReward);
        }
        else
        {
            int goldAmount = Random.Range(minGold, maxGold + 1);
            finalMockItemType = MockItemType.None;
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
        finalGoldAmount = amount;
    }

    Sprite GetUnitSprite(GameObject unit)
    {
        RenderTexture tex;
        GameObject camObj;
        var unitRoot = unit.transform.Find("UnitRoot");
        if (unitRoot == null) {
            Global.DEBUG_PRINT("[StoryTellerManager::GetUnitSprite] UnitRoot not found on unitPrefab!");
            GameObject.DestroyImmediate(unit);
            return null; // Or handle error properly
        }
        (tex, camObj) = RenderUtilities.RenderUnitToTexture(unitRoot.gameObject, 1.25f);
        Sprite unitIcon = RenderUtilities.ConvertRenderTextureToSprite(tex);
        Destroy(camObj);
        Destroy(tex);
        // Destroy(unit); 
        return unitIcon;
    }

    void LoadDialogueOptions()
    {
        dialogueSequences.Add(new string[]
        {
            "Hey there, adventurer!",
            "Here's something special for you.",
            "Make good use of it!"
        });
    
        dialogueSequences.Add(new string[]
        {
            "Welcome back, hero.",
            "You've earned this reward.",
            "Use it wisely in your battles."
        });
    
        dialogueSequences.Add(new string[]
        {
            "Another gift from the gods!",
            "Treasure it well.",
            "Your journey isn't over yet."
        });
    
        dialogueSequences.Add(new string[]
        {
            "You again? Lucky day, huh?",
            "Take this.",
            "Don't let it go to waste."
        });
    }

#if UNITY_EDITOR
    [ContextMenu("Manual Refresh Reward Item")]
    public void EditorRefresh()
    {
        GenerateReward();
    }
#endif

}
