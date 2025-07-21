using UnityEngine;
using UnityEngine.UI;

public class EditTeamButton : MonoBehaviour 
{
    public Button myButton;
    public UnitSettingLayout unitSettingLayout;
    public MockPlayerInventoryHolder playerInventoryHolder;

    private void Start() {
        if (myButton != null) {
            myButton.onClick.AddListener(OnButtonClicked);
        }
    }

    public void OnButtonClicked() 
    {
        Debug.Log("Edit Team Button clicked.");
        unitSettingLayout.Init(playerInventoryHolder.playerInventory);
    }
}

