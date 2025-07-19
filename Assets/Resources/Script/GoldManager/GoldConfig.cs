using UnityEngine;

[CreateAssetMenu(fileName = "GoldConfig", menuName = "Game/Gold Configuration")]
public class GoldConfig : ScriptableObject
{
    [Header("Match Pattern Rewards")]
    public int horizontalReward = 3;
    public int verticalReward = 3;
    public int diagonalReward = 4;
    public int zigzagReward = 5;
    public int xShapeReward = 6;
    public int fullGridReward = 10;

    [Header("Economy Settings")]
    public int baseIncomePerRound = 5;
    public int winRoundBonus = 3;
    public int goldPerInterest = 10;
    public int maxInterest = 5;
} 