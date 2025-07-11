using UnityEngine;
using UnityEngine.UI;

public class UIHealthBar : MonoBehaviour
{
    [SerializeField] UnitObject _unit = null;
    [SerializeField] Slider _slider = null;

    // Update is called once per frame
    void Update()
    {
        if (_unit == null) {
            Destroy(gameObject);
            return;
        }

        RectTransform rect = GetComponent<RectTransform>();
        Vector3 pos = _unit.transform.position;
        pos.y += 0.8f;
        rect.position = pos;

        _slider.value = _unit.GetHealthPercentage();
    }
}
