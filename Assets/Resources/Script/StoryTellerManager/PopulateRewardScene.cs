using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PopulateRewardScene
{
    [MenuItem("Tools/Populate Reward Scene")]
    public static void PopulateScene()
    {
        var canvas = GameObject.FindObjectOfType<Canvas>();
        if (!canvas)
        {
            Debug.LogError("No Canvas found in scene. Please add a Canvas first.");
            return;
        }

        GameObject rewardPanel = CreateRewardPanel(canvas.transform);
        GameObject dialogueBox = CreateDialogueBox(canvas.transform);
        GameObject managerGO = CreateSceneManager(canvas.transform, rewardPanel, dialogueBox);

        ApplySampleData(managerGO.GetComponent<RewardSceneManager>());

        Debug.Log("Reward UI populated in current scene!");
    }

    static GameObject CreateRewardPanel(Transform parent)
    {
        GameObject panel = CreateUI("RewardPanel", parent);
        var rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.2f, 0.3f);
        rect.anchorMax = new Vector2(0.8f, 0.8f);
        rect.offsetMin = rect.offsetMax = Vector2.zero;

        var layout = panel.AddComponent<VerticalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.spacing = 10;

        panel.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        var icon = CreateUI("Icon", panel.transform).AddComponent<Image>();
        icon.GetComponent<RectTransform>().sizeDelta = new Vector2(200, 200);

        var nameText = CreateTMP("NameText", panel.transform);
        nameText.fontSize = 32;
        nameText.alignment = TextAlignmentOptions.Center;

        var descText = CreateTMP("DescText", panel.transform);
        descText.fontSize = 24;
        descText.alignment = TextAlignmentOptions.Center;

        var claimButton = CreateUI("ClaimButton", panel.transform);
        var buttonImage = claimButton.AddComponent<Image>();
        buttonImage.color = new Color(1f, 1f, 1f, 0.9f);
        var button = claimButton.AddComponent<Button>();

        var claimText = CreateTMP("ClaimText", claimButton.transform);
        claimText.text = "Claim";
        claimText.alignment = TextAlignmentOptions.Center;
        claimText.fontSize = 28;
        claimText.rectTransform.sizeDelta = new Vector2(160, 40);

        return panel;
    }

    static GameObject CreateDialogueBox(Transform parent)
    {
        GameObject box = CreateUI("DialogueBox", parent);
        var rect = box.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.25f, 0f);
        rect.anchorMax = new Vector2(0.75f, 0.2f);
        rect.offsetMin = rect.offsetMax = Vector2.zero;

        var layout = box.AddComponent<HorizontalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.spacing = 10;
        layout.padding = new RectOffset(10, 10, 10, 10);

        var image = CreateUI("PortraitImage", box.transform).AddComponent<Image>();
        image.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 100);

        var dialogueText = CreateTMP("DialogueText", box.transform);
        dialogueText.fontSize = 24;
        dialogueText.alignment = TextAlignmentOptions.Left;
        dialogueText.enableWordWrapping = true;
        dialogueText.rectTransform.sizeDelta = new Vector2(600, 100);

        return box;
    }

    static GameObject CreateUI(string name, Transform parent)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        return go;
    }

    static TextMeshProUGUI CreateTMP(string name, Transform parent)
    {
        var go = CreateUI(name, parent);
        return go.AddComponent<TextMeshProUGUI>();
    }

    static GameObject CreateSceneManager(Transform parent, GameObject rewardPanel, GameObject dialogueBox)
    {
        var managerGO = new GameObject("RewardSceneManager");
        managerGO.transform.SetParent(parent, false);
        var manager = managerGO.AddComponent<RewardSceneManager>();

        manager.rewardIcon = rewardPanel.transform.Find("Icon").GetComponent<Image>();
        manager.rewardName = rewardPanel.transform.Find("NameText").GetComponent<TextMeshProUGUI>();
        manager.rewardDesc = rewardPanel.transform.Find("DescText").GetComponent<TextMeshProUGUI>();
        manager.claimButton = rewardPanel.transform.Find("ClaimButton").GetComponent<Button>();
        manager.claimButtonText = rewardPanel.transform.Find("ClaimButton/ClaimText").GetComponent<TextMeshProUGUI>();
        manager.portraitImage = dialogueBox.transform.Find("PortraitImage").GetComponent<Image>();
        manager.dialogueText = dialogueBox.transform.Find("DialogueText").GetComponent<TextMeshProUGUI>();

        return managerGO;
    }

    static void ApplySampleData(RewardSceneManager manager)
    {
        manager.rewardName.text = "Relic of Fortune";
        manager.rewardDesc.text = "This ancient relic blesses you with gold every turn.";
        manager.claimButtonText.text = "Claim Relic";
        manager.dialogueLines = new string[]
        {
            "You've proven yourself worthy.",
            "Accept this gift as a token of your strength."
        };
    
        var icon = Resources.Load<Sprite>("Sprites/game-icons.net/orc-head");
        if (icon) manager.rewardIcon.sprite = icon;
        else manager.rewardIcon.color = new Color(0.8f, 0.8f, 0.8f);
    
        manager.claimButton.onClick.RemoveAllListeners();
        manager.claimButton.onClick.AddListener(manager.OnClaimPressed);
    }
}
