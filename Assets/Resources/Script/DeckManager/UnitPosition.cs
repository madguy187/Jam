using UnityEngine;

public enum eUnitPosition {
    NONE,
    FRONT,
    BACK,
}

public class UnitPosition : MonoBehaviour {
    [SerializeField] eUnitPosition ePosition;

    public eUnitPosition GetUnitPosition() { return ePosition; }
}
