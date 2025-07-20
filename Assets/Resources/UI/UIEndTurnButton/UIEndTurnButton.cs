using UnityEngine;
using UnityEngine.UI;

public class UIEndTurnButton : MonoBehaviour
{
    private Button endTurnButton;

    void Start()
    {
        endTurnButton = GetComponent<Button>();
        if (endTurnButton != null)
        {
            endTurnButton.onClick.AddListener(OnEndTurnClick);
        }
    }

    void OnEndTurnClick()
    {
        SpinResult spinResult = SlotController.instance.GetSpinResult();
        if (spinResult != null && spinResult.GetAllMatches().Count > 0)
        {
            CombatManager.instance.StartBattleLoop(spinResult.GetAllMatches());
        }

        SlotController.instance.ResetSpins();
        SlotController.instance.ClearSpinResult();
        GoldManager.instance.StartNewTurn();
    }

    void OnDestroy()
    {
        if (endTurnButton != null)
        {
            endTurnButton.onClick.RemoveListener(OnEndTurnClick);
        }
    }
} 