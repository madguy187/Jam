using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIRevive : MonoBehaviour {
    public static UIRevive instance;

    [SerializeField] GameObject prefabUnitSlot = null;
    [SerializeField] UnitObject _unit = null;
    [SerializeField] Button _buttonRevive = null;
    [SerializeField] Button _buttonReturn = null;

    void Awake() {
        if (instance != null) {
            Destroy(instance);
        }
        instance = this;
        //Instantiate(prefabUnitSlot, transform);

        _buttonRevive.onClick.AddListener(ReviveUnit);
        _buttonReturn.onClick.AddListener(ChangeScene);
    }

    void Start() {
        UnitSettingLayout.instance.OpenLayout();
        Action<MockItemType, MockInventoryItem> onAddAction = (MockItemType itemType, MockInventoryItem item) => {
            _unit = item.unitData;
        };
        ItemTracker.Instance.AddOnAdd(TrackerType.Revive, onAddAction);

        Action<MockItemType, MockInventoryItem> onRemoveAction = (MockItemType itemType, MockInventoryItem item) => {
            _unit = null;
        };
        ItemTracker.Instance.AddOnRemove(TrackerType.Revive, onRemoveAction);
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            Debug.Log("enter");
            UnitSettingLayout.instance.OpenLayout();
        }
    }

    void ChangeScene() {
        UIFade.instance.FadeOut(2.0f);
        UIFade.instance.SetOnFadeFinish(ChangeSceneFunc);
    }

    void ChangeSceneFunc() {
        SceneManager.LoadScene("Game_Map");
        UIFade.instance.FadeIn(2.0f);
    }

    public void ReviveUnit() {
        if (_unit == null) {
            UIPopUpManager.instance.CreatePopUp("Put unit in slot");
            return;
        }

        if (!_unit.IsDead()) {
            UIPopUpManager.instance.CreatePopUp("Unit is not dead");
            return;
        }

        int cost = GetCost();
        if (cost <= 0) {
            return;
        }

        if (!GoldManager.instance.HasEnoughGold(cost)) {
            UIPopUpManager.instance.CreatePopUp("Not enough gold");
            return;
        }

        GoldManager.instance.SpendGold(cost);
        _unit.Revive();
    }

    int GetCost() {
        eUnitTier tier = _unit.unitSO.eTier;

        switch (tier) {
            case eUnitTier.STAR_1:
                return 1;
            case eUnitTier.STAR_2:
                return 3;
            case eUnitTier.STAR_3:
                return 5;
            default:
                break;
        }

        return 0;
    }
}
