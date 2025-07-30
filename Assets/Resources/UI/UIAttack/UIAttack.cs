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
        Debug.Log($"[UIAttack] Awake. Initial button interactable = {button.interactable}");
    }

    private void OnAttackClicked()
    {
        var sm = SkillSlotMachine.instance;
        if (sm == null) return;
        if (!sm.CanAttack()) return;
        sm.ExecuteFullCombatButton();
    }

    public void SetInteractable(bool interactable)
    {
        if (button == null) return;
        button.interactable = interactable;
        Debug.Log($"[UIAttack] SetInteractable({interactable}) called. Button state now: {button.interactable}");
    }

    public bool IsInteractable() => button != null && button.interactable;

    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(OnAttackClicked);
        }
    }
} 