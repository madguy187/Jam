using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIPopUp : MonoBehaviour {

    [SerializeField] TMP_Text objText = null;
    [SerializeField] float fMoveTime = 1.0f;
    [SerializeField] Image imgIcon = null;
    [SerializeField] Animator anim = null;
    [SerializeField] RectTransform objPanel = null;

    bool bPrepToDestroy = false;

    float fGoalY = 0.0f;
    float fStartY = 0.0f;

    float fCurrentTime = 0.0f;
    float fCurrentValue = 0.0f;

    public void Init(Vector3 vecStartingPos, string text, Sprite icon = null) {
        objPanel.anchoredPosition = vecStartingPos;

        fStartY = objPanel.anchoredPosition.y;
        fGoalY = objPanel.anchoredPosition.y;
        objText.text = text;

        if (imgIcon != null) {
            if (icon != null) {
                imgIcon.enabled = true;
                imgIcon.sprite = icon;
            } else {
                imgIcon.enabled = false;
            }
        }
    }

    public void SetGoalY(float y) {
        fStartY = objPanel.anchoredPosition.y;
        fGoalY = y;
    }

    public void Update() {
        UpdatePosition();
        UpdatePopUpBehavior();
    }

    public void Hide() {
        anim.SetBool("IsHide", true);
    }

    void UpdatePosition() {
        fCurrentTime += Time.deltaTime;
        float t = Mathf.Clamp01(fCurrentTime / fMoveTime);
        fCurrentValue = Mathf.Lerp(fStartY, fGoalY, t);

        Vector3 pos = objPanel.anchoredPosition;
        pos.y = fCurrentValue;

        objPanel.anchoredPosition = pos;
    }

    void UpdatePopUpBehavior() {
        if (AnimatorIsPlaying("HideRight")) {
            bPrepToDestroy = true;
        }

        if (!bPrepToDestroy) {
            return;
        }

        if (AnimatorIsPlaying()) {
            return;
        }

        Destroy(gameObject);
        UIPopUpManager.instance.UpdateAllPopUpPosition();
    }

    bool AnimatorIsPlaying() {
        return anim.GetCurrentAnimatorStateInfo(0).length >
                anim.GetCurrentAnimatorStateInfo(0).normalizedTime;
    }
    
    bool AnimatorIsPlaying(string stateName){
        return anim.GetCurrentAnimatorStateInfo(0).IsName(stateName);
    }
}
