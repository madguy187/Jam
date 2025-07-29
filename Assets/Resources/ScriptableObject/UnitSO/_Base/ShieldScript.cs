using TMPro;
using UnityEngine;

public class ShieldScript : MonoBehaviour
{
    public UnitObject unit;
    [SerializeField] TMP_Text text;

    // Update is called once per frame
    void Update() {
        float fCurrentShield = unit._currentShield.GetVal();
        text.text = fCurrentShield.ToString();
    }
}
