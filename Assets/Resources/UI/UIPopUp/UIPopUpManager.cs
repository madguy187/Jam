using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UIPopUpManager : MonoBehaviour {
    public static UIPopUpManager instance;

    [SerializeField] Transform transDefault = null;
    [SerializeField] GameObject prefabPopUp = null;
    [SerializeField] float fGap = 5.0f;
    [SerializeField] float fLifeTime = 3.0f;

    float _fCurrentTime = 0.0f;

    List<UIPopUp> _listCurrentPopUp = new List<UIPopUp>();

    public void Awake() {
        if (instance != null) {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void UpdateAllPopUpPosition() {
        if (_listCurrentPopUp.Count <= 0) {
            return;
        }

        for (int i = 0; i < _listCurrentPopUp.Count; i++) {
            _listCurrentPopUp[i].SetGoalY(GetStartingPos(i).y);
        }
    }

    public void Update() {
        if (_listCurrentPopUp.Count <= 0) {
            return;
        }

        _fCurrentTime += Time.deltaTime;
        if (_fCurrentTime <= fLifeTime) {
            return;
        }

        UIPopUp objFirstPopUp = _listCurrentPopUp.First();
        _listCurrentPopUp.RemoveAt(0);
        objFirstPopUp.Hide();
        _fCurrentTime = 0.0f;
    }

    public void CreatePopUp(string text) {
        GameObject obj = Instantiate(prefabPopUp, transform);
        obj.transform.position = GetStartingPos(_listCurrentPopUp.Count);
        UIPopUp popUpComp = obj.GetComponent<UIPopUp>();
        popUpComp.Init(text);
        _listCurrentPopUp.Add(popUpComp);
    }

    public Vector3 GetStartingPos(int index) {
        RectTransform rect = transDefault.GetComponent<RectTransform>();
        float yOffset = rect.rect.height + fGap;
        Vector3 pos = transDefault.position;
        pos.y -= yOffset * index;

        return pos;
    }
}
