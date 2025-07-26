using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UIRevive : MonoBehaviour {
    public static UIRevive instance;

    [SerializeField] GameObject prefabUnitSlot = null;
    [SerializeField] UnitObject _unit = null;
    [SerializeField] Button _button = null;

    void Awake() {
        if (instance != null) {
            Destroy(instance);
        }
        instance = this;
        //Instantiate(prefabUnitSlot, transform);

        _button.onClick.AddListener(ReviveUnit);
    }

    void Start() {
        Action<MockItemType, MockInventoryItem> onAddAction = (MockItemType itemType, MockInventoryItem item) => {
            _unit = item.unitData;
        };
        ItemTracker.Instance.AddOnAdd(TrackerType.Revive, onAddAction);

        Action<MockItemType, MockInventoryItem> onRemoveAction = (MockItemType itemType, MockInventoryItem item) => {
            _unit = null;
        };
        ItemTracker.Instance.AddOnRemove(TrackerType.Revive, onRemoveAction);
    }

    public void ReviveUnit() {
        if (_unit == null) {
            return;
        }

        if (!_unit.IsDead()) {
            return;
        }

        int cost = GetCost();
        if (cost <= 0) {
            return;
        }

        if (!GoldManager.instance.HasEnoughGold(cost)) {
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
