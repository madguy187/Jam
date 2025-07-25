using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIAttack : MonoBehaviour
{
    private Button button;

    void Awake()
    {
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnAttackClicked);
        }
        else
        {
            Debug.LogError("[UIAttack] No Button component found!");
        }
    }

    void OnAttackClicked()
    {
        SkillSlotMachine machine = FindObjectOfType<SkillSlotMachine>();
        if (machine != null)
        {
            machine.EndPlayerTurn();
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
            button.onClick.RemoveListener(OnAttackClicked);
        }
    }
} 