using UnityEngine;

[CreateAssetMenu(fileName = "SlotConfig", menuName = "Game/Slot Configuration")]
public class SlotConfig : ScriptableObject
{
    [Header("Grid Settings")]
    public int gridRows = 3;
    public int gridColumns = 3;

    [Header("Spin Settings")]
    public int freeSpinsPerTurn = 1;
    public int baseSpinCost = 2;

    [Header("Animation Settings")]
    public float spinAnimationDuration = 1.0f;
    public float matchAnimationScale = 1.2f;
    public int TotalGridSize => GetTotalGridSize();

    public int GetTotalGridSize()
    {
        return gridRows * gridColumns;
    } 
} 