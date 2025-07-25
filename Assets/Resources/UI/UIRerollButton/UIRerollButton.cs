using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Button))]
public class UIRerollButton : MonoBehaviour
{
    private Button button;

    [Header("References")]
    [SerializeField] private SkillSlotGrid skillSlotGrid;
    
    private void InitializeComponents()
    {
        button = GetComponent<Button>();
        if (button == null)
        {
            Debug.LogError("[UIRollButton] Button component not found!");
            enabled = false;
            return;
        }

        if (skillSlotGrid == null)
        {
            Debug.LogError("[UIRollButton] SkillSlotGrid reference not found!");
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
        if (skillSlotGrid != null)
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