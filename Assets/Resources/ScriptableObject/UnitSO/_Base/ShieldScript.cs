using TMPro;
using UnityEngine;

public class ShieldScript : MonoBehaviour
{
    public UnitObject unit;
    [SerializeField] TMP_Text text;

    // Update is called once per frame
    void Update() {
        if (unit == null) {
            return;
        }

        float fCurrentShield = unit._currentShield.GetVal();
        text.text = Mathf.FloorToInt(fCurrentShield).ToString();
    }
}
