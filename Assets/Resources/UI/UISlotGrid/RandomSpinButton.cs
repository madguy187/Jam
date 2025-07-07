using UnityEngine;
using UnityEngine.UI;

// Handles the spin button click and triggerrs the spin
public class RandomSpinButton : MonoBehaviour
{
    [SerializeField] private Button button;
    
    void Awake()
    {
        button = GetComponent<Button>();
    }
    
    void Start()
    {
        button.onClick.AddListener(OnButtonClick);
    }
    
    public void OnButtonClick()
    {
        if (SpinController.instance == null || SymbolGenerator.instance == null) return;
        
        SlotGridUI gridUI = FindObjectOfType<SlotGridUI>();
        if (gridUI == null) return;
        
        if (gridUI.IsSpinning()) return;
        
        SpinController.instance.FillGridWithRandomSymbols();
    }
    
    public void TriggerRandomSpin()
    {
        OnButtonClick();
    }
} 