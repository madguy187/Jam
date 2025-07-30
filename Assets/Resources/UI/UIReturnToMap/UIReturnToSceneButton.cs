using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Map;

public class UIReturnSceneButton : MonoBehaviour {
    // Assign this in the Inspector or via code
    public Button myButton;
    [SerializeField] private string targetScene = "Game_Map";

    private void Start() {
        if (myButton != null) {
            myButton.onClick.AddListener(OnButtonClicked);
        }
    }

    private void OnButtonClicked() {

        if (UIPopUpManager.instance != null)
        {
            UIPopUpManager.instance.ClearAllPopUps();
        }

        // Perform map cleanup when returning to main menu
        if (targetScene == "Game_MainMenu" && MapPlayerTracker.Instance != null)
        {
            MapPlayerTracker.Instance.CleanUpMap();
        }
        
        SceneManager.LoadScene(targetScene);
    }

    public void SetTargetScene(string sceneName)
    {
        targetScene = sceneName;
    }
}