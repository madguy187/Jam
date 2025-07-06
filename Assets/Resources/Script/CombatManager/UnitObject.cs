using UnityEngine;

public class UnitObject : MonoBehaviour {
    [field: SerializeField] public UnitScriptableObject unitSO { get; private set; }

    public void SetUnitSO(UnitScriptableObject _unitSO) {
        unitSO = _unitSO;
    }
}
