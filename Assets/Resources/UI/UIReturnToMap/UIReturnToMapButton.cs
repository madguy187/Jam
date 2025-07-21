using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIReturnToMapButton : MonoBehaviour {
    // Assign this in the Inspector or via code
    public Button myButton;

    private void Start() {
        if (myButton != null) {
            myButton.onClick.AddListener(OnButtonClicked);
        }
    }

    private void OnButtonClicked() {
        SceneManager.LoadScene("Game_Map");
    }
}