using TMPro;
using UnityEngine;

public class UIDynamicNumber : MonoBehaviour {

    [SerializeField] TMP_Text objText = null;
    public float fOffsetPos = 1.0f;

    CustomTimer timer = new CustomTimer();

    bool IsInit = false;

    public void Init(Vector3 posStarting, float fAliveTime, string text) {
        timer.SetTime(fAliveTime);
        timer.SetFunc(DestroyAfterTime);
        timer.Reset();

        objText.text = text;

        gameObject.transform.position = posStarting;

        IsInit = true;
    }

    void Update() {
        if (!IsInit) {
            return;
        }

        timer.Update();
    }

    // Update is called once per frame
    void FixedUpdate() {
        if (!IsInit) {
            return;
        }

        Vector3 pos = gameObject.transform.position;
        pos.y += fOffsetPos * Time.fixedDeltaTime;
        gameObject.transform.position = pos;
    }

    void DestroyAfterTime() {
        Destroy(gameObject);
    }
}
