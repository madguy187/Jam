using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIEndTurnButton : MonoBehaviour
{
    private Button button;

    void Awake()
    {
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnEndTurnClicked);
        }
        else
        {
            Debug.LogError("[UIEndTurnButton] No Button component found!");
        }
    }

    void OnEndTurnClicked()
    {
        if (SlotController.instance != null)
        {
            SlotController.instance.EndPlayerTurn();
        }
    }

    public void SetInteractable(bool interactable)
    {
        if (button != null)
        {
            button.interactable = interactable;
        }
    }

    void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(OnEndTurnClicked);
        }
    }
} 