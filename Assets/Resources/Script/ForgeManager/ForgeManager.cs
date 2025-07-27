using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ForgeManager : MonoBehaviour
{
    [Header("Bag Panel")]
    [SerializeField] private Transform bagContainer;         // GridLayoutGroup parent for bag
    [SerializeField] private GameObject bagSlotPrefab;       // UI prefab that displays a relic

    [Header("Forge Panel")]
    [SerializeField] private Image relicSlot1;
    [SerializeField] private Image relicSlot2;
    [SerializeField] private Image resultSlot;

    [Header("Buttons")]
    [SerializeField] private Button mergeButton;
    [SerializeField] private Button breakButton;
    [SerializeField] private Button forgeButton;

    [Header("Combiner")]
    public RelicCombiner relicCombiner;

    [Header("Test Relics")]
    public List<RelicScriptableObject> testRelics;

    private RelicScriptableObject selectedRelic1;
    private RelicScriptableObject selectedRelic2;
    private RelicScriptableObject forgedResult;

    private List<GameObject> bagSlots = new();

    private void Start()
    {
        mergeButton.onClick.AddListener(() => Debug.Log("Merge Mode Selected")); // Placeholder for toggle logic
        breakButton.onClick.AddListener(() => Debug.Log("Break Mode Selected"));
        forgeButton.onClick.AddListener(DoForge);
        PopulateBagItems(testRelics);
    }

    // =============================
    // Bag Panel Setup
    // =============================

    public void ClearBagItemsOnly()
    {
        foreach (Transform child in bagContainer)
        {
            Destroy(child.gameObject);
        }
    }

    public void GenerateFixedBagSlots(int count)
    {
        ClearBagItemsOnly();
        for (int i = 0; i < count; i++)
        {
            var slotObj = Instantiate(bagSlotPrefab, bagContainer);
            slotObj.name = $"BagSlot_{i}";
            slotObj.GetComponent<Button>().interactable = false;
            slotObj.GetComponentInChildren<TMP_Text>().text = "Empty";
            bagSlots.Add(slotObj);
        }
    }

    public void PopulateBagItems(List<RelicScriptableObject> relics)
    {
        ClearBagItemsOnly();
        foreach (var relic in relics)
        {
            var slotObj = Instantiate(bagSlotPrefab, bagContainer);
            var btn = slotObj.GetComponent<Button>();
            var image = slotObj.transform.Find("RelicImage").GetComponent<Image>();
            var label = slotObj.transform.Find("RelicNameText").GetComponent<TMP_Text>();

            image.sprite = relic.GetRelicSprite();
            label.text = relic.GetRelicName();

            btn.onClick.AddListener(() => OnRelicSlotClicked(relic));
            bagSlots.Add(slotObj);
        }
    }

    private void OnRelicSlotClicked(RelicScriptableObject relic)
    {
        if (selectedRelic1 == relic)
        {
            selectedRelic1 = null;
            relicSlot1.sprite = null;
            relicSlot1.color = new Color(1, 1, 1, 0);
        }
        else if (selectedRelic2 == relic)
        {
            selectedRelic2 = null;
            relicSlot2.sprite = null;
            relicSlot2.color = new Color(1, 1, 1, 0);
        }
        else if (selectedRelic1 == null)
        {
            selectedRelic1 = relic;
            relicSlot1.sprite = relic.GetRelicSprite();
            relicSlot1.color = Color.white;
        }
        else if (selectedRelic2 == null)
        {
            selectedRelic2 = relic;
            relicSlot2.sprite = relic.GetRelicSprite();
            relicSlot2.color = Color.white;
        }
        else
        {
            Debug.Log("Both slots are already filled.");
        }

        TryUpdateForgeResult();
    }


    // =============================
    // Forge Logic
    // =============================

    private void TryUpdateForgeResult()
    {
        if (selectedRelic1 != null && selectedRelic2 != null)
        {
            RelicScriptableObject result;
            if (relicCombiner.TryCombine(selectedRelic1, selectedRelic2, out result))
            {
                resultSlot.sprite = result.GetRelicSprite();
                resultSlot.color = Color.white;
                Global.DEBUG_PRINT($"[ForgeManager::TryUpdateForgeResult] Forge Result: {result.GetRelicName()}");
            }
            else
            {
                resultSlot.sprite = null;
                resultSlot.color = new Color(1, 1, 1, 0); // hide
            }
        }
    }

    private void DoForge()
    {
        if (selectedRelic1 != null && selectedRelic2 != null)
        {
            RelicScriptableObject forgedResult;
            if (relicCombiner.TryCombine(selectedRelic1, selectedRelic2, out forgedResult))
            {
                resultSlot.sprite = forgedResult.GetRelicSprite();
                resultSlot.color = Color.white;
                Debug.Log($"Forged: {forgedResult.GetRelicName()}");

                // Add result to bag
                testRelics.Add(forgedResult);
                PopulateBagItems(testRelics);

                // Clear forge slots
                ClearForgeSlots();
            }
            else
            {
                Debug.Log("Invalid combination.");
                resultSlot.color = new Color(1, 1, 1, 0);
                forgedResult = null;
            }
        }
        else
        {
            Debug.Log("Select two relics first.");
        }
    }

    public void ClearForgeSlots()
    {
        selectedRelic1 = null;
        selectedRelic2 = null;
        relicSlot1.sprite = null;
        relicSlot2.sprite = null;
        relicSlot1.color = new Color(1, 1, 1, 0);
        relicSlot2.color = new Color(1, 1, 1, 0);
        resultSlot.sprite = null;
        resultSlot.color = new Color(1, 1, 1, 0);
    }
}
