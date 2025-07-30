using UnityEngine;
using UnityEngine.UI;

public enum AttackBehaviour { PlayerCombatOnly, PlayerAndEnemy }

[RequireComponent(typeof(Button))]
public class UIAttack : MonoBehaviour
{
    private Button button;
    [SerializeField] private AttackBehaviour behaviour = AttackBehaviour.PlayerAndEnemy;

    private void Awake()
    {
        button = GetComponent<Button>();

        if (button == null)
        {
            Debug.LogError("[UIAttack] Button component missing!");
            enabled = false;
            return;
        }

        button.onClick.AddListener(OnAttackClicked);
    }

    private void OnAttackClicked() {
        SkillSlotMachine.instance.ExecuteFullCombatButton();
    }

    public void SetInteractable(bool interactable)
    {
        button.interactable = interactable;
    }

    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(OnAttackClicked);
        }
    }
} 