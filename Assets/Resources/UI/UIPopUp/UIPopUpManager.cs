using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIPopUpManager : MonoBehaviour {
    public static UIPopUpManager instance;

    [SerializeField] RectTransform transDefault = null;
    [SerializeField] GameObject prefabPopUp = null;
    [SerializeField] float fGap = 5.0f;
    [SerializeField] float fLifeTime = 3.0f;

    [Header("Spacing")]
     // height of one popup panel
    [SerializeField] float popupHeight = 50f;

    float _fCurrentTime = 0.0f;

    List<UIPopUp> _listCurrentPopUp = new List<UIPopUp>();

    public void Awake() {
        if (instance != null) {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
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
        UIPopUp popUpComp = obj.GetComponent<UIPopUp>();
        popUpComp.Init(GetStartingPos(_listCurrentPopUp.Count), text);
        _listCurrentPopUp.Add(popUpComp);
    }

    public void CreatePopUp(string text, Sprite icon)
    {
        GameObject obj = Instantiate(prefabPopUp, transform);
        UIPopUp pop = obj.GetComponent<UIPopUp>();
        pop.Init(GetStartingPos(_listCurrentPopUp.Count), text, icon);
        _listCurrentPopUp.Add(pop);
    }

    public Vector3 GetStartingPos(int index) {
        float yOffset = popupHeight + fGap;
        Vector3 pos = transDefault.anchoredPosition;
        Debug.Log(pos);
        pos.y -= yOffset * index;

        return pos;
    }

    public void ClearAllPopUps()
    {
        foreach (var pop in _listCurrentPopUp)
        {
            if (pop != null)
            {
                Destroy(pop.gameObject);
            }
        }
        _listCurrentPopUp.Clear();
        _fCurrentTime = 0f;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ClearAllPopUps();
    }

    void OnDestroy()
    {
        if (instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            instance = null;
        }
    }
}
