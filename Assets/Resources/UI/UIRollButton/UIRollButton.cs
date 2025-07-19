using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Button))]
public class UIRollButton : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI buttonText;
    
    private void Awake()
    {
        InitializeComponents();
    }
    
    private void InitializeComponents()
    {
        if (button == null)
        {
            button = GetComponent<Button>();
            if (button == null)
            {
                Debug.LogError("[UIRollButton] Button component not found!");
                enabled = false;
                return;
            }
        }
        
        if (buttonText == null)
        {
            buttonText = GetComponentInChildren<TextMeshProUGUI>();
        }
    }
    
    void Start()
    {
        if (!enabled) return;
        
        if (SlotController.instance == null)
        {
            Debug.LogError("[UIRollButton] SlotController instance not found!");
            enabled = false;
            return;
        }
        
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
        if (SlotController.instance != null)
        {
            SlotController.instance.FillGridWithRandomSymbols();
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