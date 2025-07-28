using TMPro;
using UnityEngine;

public class UIPopUp : MonoBehaviour {

    [SerializeField] TMP_Text objText = null;
    [SerializeField] float fMoveTime = 1.0f;

    Animator anim = null;
    bool bPrepToDestroy = false;

    float fGoalY = 0.0f;
    float fStartY = 0.0f;

    float fCurrentTime = 0.0f;
    float fCurrentValue = 0.0f;

    void Start() {
        anim = GetComponent<Animator>();
    }

    public void Init(string text) {
        fStartY = transform.position.y;
        fGoalY = transform.position.y;
        objText.text = text;
    }

    public void SetGoalY(float y) {
        fStartY = transform.position.y;
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

        Vector3 pos = transform.position;
        pos.y = fCurrentValue;

        transform.position = pos;
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
