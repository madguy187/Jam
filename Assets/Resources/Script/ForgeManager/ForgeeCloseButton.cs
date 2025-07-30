using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ForgeCloseButton : MonoBehaviour 
{
    public Button myButton;
    public GameObject panel;

    private void Start() 
    {
        if (myButton != null) {
            myButton.onClick.AddListener(OnButtonClicked);
        }

        // Make sure layout starts hidden
        panel.SetActive(false);
    }

    private void OnButtonClicked() 
    {
        panel.SetActive(false);
    }
}