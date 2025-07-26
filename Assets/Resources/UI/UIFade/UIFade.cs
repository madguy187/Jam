using System;
using UnityEngine;
using UnityEngine.UI;

public class UIFade : MonoBehaviour {
    public static UIFade instance;
    Image imageComp;

    bool bIsActive = false;

    float fStartValue = 0.0f;
    float fEndValue = 0.0f;
    float fCurrentValue = 0.0f;

    float fTimeToFade = 2.0f;
    float fCurrentTime = 0.0f;

    Action onFadeFinish = null;
    public void SetOnFadeFinish(Action action) { onFadeFinish = action; }

    void Awake() {
        if (instance != null) {
            Destroy(gameObject);
        } else {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        imageComp = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update() {
        if (!bIsActive) {
            return;
        }

        fCurrentTime += Time.deltaTime;
        float t = Mathf.Clamp01(fCurrentTime / fTimeToFade);
        fCurrentValue = Mathf.Lerp(fStartValue, fEndValue, t);
        SetAlpha();

        if (t >= 1f) {
            bIsActive = false; // Disable the script or stop the coroutine
            if (onFadeFinish != null) {
                onFadeFinish();
                onFadeFinish = null;
            }
        }
    }

    public void FadeIn(float timeToFade) {
        fStartValue = 1.0f;
        fEndValue = 0.0f;

        fCurrentTime = 0.0f;
        fTimeToFade = timeToFade;
        bIsActive = true;
        SetAlpha();
    }

    public void FadeOut(float timeToFade) {
        fStartValue = 0.0f;
        fEndValue = 1.0f;

        fCurrentTime = 0.0f;
        fTimeToFade = timeToFade;
        bIsActive = true;
        SetAlpha();
    }

    void SetAlpha() {
        Color imageColor = imageComp.color; // Get the current color
        imageColor.a = fCurrentValue;       // Modify the alpha component
        imageComp.color = imageColor;       // Assign the modified color back
    }
}
