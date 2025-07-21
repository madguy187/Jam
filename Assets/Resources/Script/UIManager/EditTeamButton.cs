using UnityEngine;

public class EditTeamButton : MonoBehaviour 
{
    public UnitSettingLayout unitSettingLayout;
    public MockPlayerInventoryHolder playerInventoryHolder;

    public void OnClick() 
    {
        Debug.Log("Edit Team Button clicked.");
        unitSettingLayout.Init(playerInventoryHolder.playerInventory);
    }
}

