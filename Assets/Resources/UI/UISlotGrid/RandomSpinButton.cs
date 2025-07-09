using UnityEngine;
using UnityEngine.UI;

public class RandomSpinButton : MonoBehaviour
{
    private Button button;
    
    void Start()
    {
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnClick);
        }
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
        SpinController.instance.FillGridWithRandomSymbols();
    }
    
    public void TriggerRandomSpin()
    {
        OnClick();
    }
} 