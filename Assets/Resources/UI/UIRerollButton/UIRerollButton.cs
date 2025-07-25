using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Button))]
public class UIRerollButton : MonoBehaviour
{
    private Button button;

    [Header("References")]
    [SerializeField] private SkillSlotMachine slotMachine;
    [SerializeField] private SkillSlotGrid skillSlotGrid; // fallback if machine not assigned
    
    private void InitializeComponents()
    {
        button = GetComponent<Button>();
        if (button == null)
        {
            Debug.LogError("[UIRollButton] Button component not found!");
            enabled = false;
            return;
        }

        if (slotMachine == null)
        {
            slotMachine = FindObjectOfType<SkillSlotMachine>();
        }
        if (skillSlotGrid == null && slotMachine == null)
        {
            skillSlotGrid = FindObjectOfType<SkillSlotGrid>();
        }
        if (slotMachine == null && skillSlotGrid == null)
        {
            Debug.LogError("[UIRollButton] No slot machine or grid found!");
            enabled = false;
        }
    }
    
    void Start()
    {
        InitializeComponents();
        if (!enabled) return;
        button.onClick.AddListener(OnClick);
    }
    
    void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(OnClick);
        }
    }
    
    private void OnClick()
    {
        if (slotMachine != null)
        {
            slotMachine.Spin();
        }
        else if (skillSlotGrid != null)
        {
            skillSlotGrid.Spin();
        }
    }
    
    public void TriggerRoll()
    {
        OnClick();
    }
    
    public void SetInteractable(bool interactable)
    {
        if (button != null)
        {
            button.interactable = interactable;
        }
    }
} 