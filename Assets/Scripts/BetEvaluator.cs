using System.Collections.Generic;
using UnityEngine;

public class BetEvaluator : MonoBehaviour
{
    [Header("References")]
    public RouletteBall rouletteBall;

    [Header("Chips")]
    public List<GameObject> placedChips = new List<GameObject>();

    [ContextMenu("Evaluate Bets")]
    public void EvaluateBets()
    {
        if (rouletteBall == null)
        {
            Debug.LogError("❌ BetEvaluator is missing a reference to RouletteBall.");
            return;
        }

        if (placedChips.Count == 0)
        {
            Debug.LogWarning("⚠️ No chips assigned, attempting to gather from scene.");
            GatherChipsFromScene();
        }

        GameObject winningSlot = rouletteBall.GetWinningSlot();
        if (winningSlot == null)
        {
            Debug.LogWarning("❌ No winning slot available yet.");
            return;
        }

        RouletteSlot slotComponent = winningSlot.GetComponent<RouletteSlot>();
        if (slotComponent == null)
        {
            Debug.LogWarning("❌ Winning slot has no RouletteSlot component.");
            return;
        }

        foreach (var chip in placedChips)
        {
            if (chip == null)
            {
                Debug.LogWarning("Found a null entry in placedChips list. Skipping.");
                continue;
            }

            Collider2D chipCollider = chip.GetComponent<Collider2D>();
            if (chipCollider == null) continue;

            Collider2D[] hits = Physics2D.OverlapCircleAll(chipCollider.bounds.center, 0.1f);
            bool isWinning = false;

            foreach (var hit in hits)
            {
                if (!hit.CompareTag("BetZone")) continue;
                BetZone betZone = hit.GetComponent<BetZone>();
                if (betZone == null) continue;

                // Direct match
                if (betZone.linkedSlot == winningSlot)
                {
                    isWinning = true;
                    break;
                }

                // Grouped match
                if (betZone.groupType != BetGroupType.None)
                {
                    isWinning = betZone.groupType switch
                    {
                        BetGroupType.Red => slotComponent.color == RouletteColor.Red,
                        BetGroupType.Black => slotComponent.color == RouletteColor.Black,
                        BetGroupType.Even => slotComponent.parity == RouletteParity.Even,
                        BetGroupType.Odd => slotComponent.parity == RouletteParity.Odd,
                        BetGroupType.First_1_12 => slotComponent.dozen == RouletteDozen.First_1_12,
                        BetGroupType.Second_13_24 => slotComponent.dozen == RouletteDozen.Second_13_24,
                        BetGroupType.Third_25_36 => slotComponent.dozen == RouletteDozen.Third_25_36,
                        BetGroupType.Low_1_18 => slotComponent.half == RouletteHalf.Low_1_18,
                        BetGroupType.High_19_36 => slotComponent.half == RouletteHalf.High_19_36,
                        BetGroupType.Top => slotComponent.line == RouletteLine.Top,
                        BetGroupType.Middle => slotComponent.line == RouletteLine.Middle,
                        BetGroupType.Bottom => slotComponent.line == RouletteLine.Bottom,
                        _ => false
                    };

                    if (isWinning) break;
                }
            }

            Debug.Log($"💰 Chip '{chip.name}' => {(isWinning ? "WIN ✅" : "LOSE ❌")}");
        }
    }

    [ContextMenu("Gather Chips From Scene")]
    public void GatherChipsFromScene()
    {
        placedChips.Clear();
        foreach (var dragger in FindObjectsByType<BettingChipDragger>(FindObjectsSortMode.None))
        {
            placedChips.Add(dragger.gameObject);
        }
        Debug.Log($"Collected {placedChips.Count} chips from the scene.");
    }

}
