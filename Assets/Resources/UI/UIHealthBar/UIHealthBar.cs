using UnityEngine;
using UnityEngine.UI;

public class UIHealthBar : MonoBehaviour
{
    [SerializeField] UnitObject _unit = null;
    [SerializeField] Slider _slider = null;

    public void SetUnit(UnitObject unit) {
        _unit = unit;
    }

    // Update is called once per frame
    void Update()
    {
        if (_unit == null) {
            Destroy(gameObject);
            return;
        }

        _slider.value = _unit.GetHealthPercentage();
    }
}
