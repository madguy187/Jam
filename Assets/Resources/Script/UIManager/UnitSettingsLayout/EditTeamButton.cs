using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EditTeamButton : MonoBehaviour 
{
    public Button myButton;
    public TMP_Text buttonText;
    public UnitSettingLayout unitSettingLayout;
    public MockPlayerInventoryHolder playerInventoryHolder;

    private bool isOpen = false;

    private void Start() {
        if (myButton != null) {
            myButton.onClick.AddListener(OnButtonClicked);
        }

        // Make sure layout starts hidden
        unitSettingLayout.gameObject.SetActive(false);
        buttonText.text = "Edit";
    }

    private void OnButtonClicked() 
    {
        isOpen = !isOpen;

        if (isOpen)
        {
            unitSettingLayout.Init(playerInventoryHolder.playerInventory);
            buttonText.text = "Close";
        }
        else
        {
            unitSettingLayout.CloseLayout();
            buttonText.text = "Edit";
        }
    }
}

