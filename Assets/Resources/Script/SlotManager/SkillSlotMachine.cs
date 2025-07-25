using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillSlotMachine : MonoBehaviour
{
    public static SkillSlotMachine instance { get; private set; }

    [Header("Columns (Left → Right)")]
    [SerializeField] private SkillSlotGrid leftColumn;
    [SerializeField] private SkillSlotGrid centreColumn;
    [SerializeField] private SkillSlotGrid rightColumn;

    [Header("Roll Settings")]
    [Tooltip("Optional stagger between starting each column (seconds)")]
    [SerializeField] private float columnStartDelay = 0f;

    private readonly List<SkillSlotGrid> columns = new List<SkillSlotGrid>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }
        if (leftColumn != null) columns.Add(leftColumn);
        if (centreColumn != null) columns.Add(centreColumn);
        if (rightColumn != null) columns.Add(rightColumn);
    }

    public bool IsRolling()
    {
        foreach (var col in columns)
        {
            if (col != null && col.IsRolling())
            {
                return true;
            }
        }
        return false;
    }

    public void Spin()
    {
        // Spend gold via SlotController logic
        if (SlotController.instance != null)
        {
            int cost = SlotController.instance.GetCurrentSpinCost();
            if (cost > 0)
            {
                if (!GoldManager.instance.SpendGold(cost))
                {
                    Debug.Log("[SkillSlotMachine] Not enough gold to spin. Cost: " + cost);
                    return;
                }
            }

            // Record that we used a spin this turn
            SlotController.instance.IncrementSpins();
        }

        if (IsRolling()) return;

        StartCoroutine(SpinRoutine());
    }

    private IEnumerator SpinRoutine()
    {
        // Trigger columns sequentially with optional delay
        foreach (var col in columns)
        {
            if (col != null)
            {
                col.Spin();
                if (columnStartDelay > 0f)
                {
                    yield return new WaitForSeconds(columnStartDelay);
                }
            }
        }

        // Wait until all columns finish
        while (IsRolling())
        {
            yield return null;
        }

        // Build resulting 3×3 archetype grid (rows: top→bottom, cols: left→right)
        eUnitArchetype[,] archetypeGrid = new eUnitArchetype[3, 3];
        var columnRefs = new[] { leftColumn, centreColumn, rightColumn };
        for (int c = 0; c < 3; c++)
        {
            var col = columnRefs[c];
            if (col == null) continue;

            List<eUnitArchetype> visibles = col.GetVisibleArchetypes();
            for (int r = 0; r < visibles.Count && r < 3; r++)
            {
                archetypeGrid[r, c] = visibles[r];
            }
        }

        // Convert to SymbolType array (length 9) for MatchDetector / SlotController
        SymbolType[] symbols = new SymbolType[9];
        for (int r = 0; r < 3; r++)
        {
            for (int c = 0; c < 3; c++)
            {
                symbols[r * 3 + c] = ArchetypeToSymbol(archetypeGrid[r, c]);
            }
        }

        Debug.Log("[SkillSlotMachine] Spin complete – symbols: " + string.Join(",", symbols));

        // Feed results back into SlotController for match detection & rewards
        if (SlotController.instance != null)
        {
            SlotController.instance.ApplyExternalSpin(symbols);
        }
    }

    // Enemy/preset spin support
    public IEnumerator SpinWithPresetSymbols(SymbolType[] presetSymbols)
    {
        if (IsRolling()) yield break;

        // Trigger columns visually (no gold spend / spin count)
        foreach (var col in columns)
        {
            if (col != null)
            {
                col.Spin();
                if (columnStartDelay > 0f)
                {
                    yield return new WaitForSeconds(columnStartDelay);
                }
            }
        }

        while (IsRolling())
        {
            yield return null;
        }

        // Apply the forced symbols to the game state
        if (SlotController.instance != null)
        {
            SlotController.instance.ApplyExternalSpin(presetSymbols);
        }
    }

    private SymbolType ArchetypeToSymbol(eUnitArchetype archetype)
    {
        switch (archetype)
        {
            case eUnitArchetype.HOLY:
                return SymbolType.HOLY;
            case eUnitArchetype.UNDEAD:
                return SymbolType.UNDEAD;
            case eUnitArchetype.ELF:
                return SymbolType.ELF;
            default:
                return SymbolType.EMPTY;
        }
    }
} 