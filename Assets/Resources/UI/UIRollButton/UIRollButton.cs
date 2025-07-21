using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Button))]
public class UIRollButton : MonoBehaviour
{
    private Button button;
    
    private void InitializeComponents()
    {
        button = GetComponent<Button>();
        if (button == null)
        {
            Debug.LogError("[UIRollButton] Button component not found!");
            enabled = false;
            return;
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
        Global.DEBUG_PRINT("[UIRollButton] Button clicked!");
        SlotController.instance.FillGridWithRandomSymbols();
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