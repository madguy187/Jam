using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIMainMenu : MonoBehaviour {
    [SerializeField] Button btnStart = null;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        btnStart.onClick.AddListener(StartGame);
        AudioManager.instance.PlayEssential(AudioManager.EssentialAudio.Main);
    }

    void StartGame() {
        UIFade.instance.FadeOut(2.0f);
        UIFade.instance.SetOnFadeFinish(FadeEnd);
    }

    void FadeEnd() {
        UIFade.instance.FadeIn(2.0f);
        SceneManager.LoadScene("Game_GuildHall");
    }
}
