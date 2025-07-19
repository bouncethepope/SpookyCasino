using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class BetEvaluator : MonoBehaviour
{
    [Header("References")]
    public RouletteBall rouletteBall;
    [Tooltip("Game tester used to reset the table")] public GameTester gameTester;

    [Header("Table Reset")]
    [Tooltip("Delay before resetting the game once chips are collected")]
    public float tableResetDelay = 1f;
    [Tooltip("Skip resetting the wheel when resetting from this script")]
    public bool skipWheelReset = true;

    [Header("Chips")]
    public List<GameObject> placedChips = new List<GameObject>();

    [Header("Chip Collection")]
    [Tooltip("Target transform chips move to when the bet wins")]
    public Transform winningChipDestination;
    [Tooltip("Target transform chips move to when the bet loses")]
    public Transform losingChipDestination;
    [Tooltip("Delay after evaluation before chips start moving")]
    public float chipCollectDelay = 1f;
    [Tooltip("Duration of the chip movement tween")]
    public float chipMoveDuration = 0.5f;
    [Tooltip("Delay between collecting winning chips and losing chips")]
    public float chipLossCollectDelay = 0.5f;

    // Store rewards per chip to process after movement
    private Dictionary<GameObject, int> rewardAmounts = new();

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

        List<GameObject> winningChips = new List<GameObject>();
        List<GameObject> losingChips = new List<GameObject>();
        rewardAmounts.Clear();

        foreach (var chip in placedChips)
        {
            if (chip == null)
            {
                Debug.LogWarning("Found a null entry in placedChips list. Skipping.");
                continue;
            }

            List<BetZone> zones = new List<BetZone>();
            BetChip chipInfo = chip.GetComponent<BetChip>();
            if (chipInfo != null)
            {
                chipInfo.UpdateBetZones();
                zones.AddRange(chipInfo.betZones);
            }
            else
            {
                Collider2D chipCollider = chip.GetComponent<Collider2D>();
                if (chipCollider == null) continue;
                Collider2D[] hits = Physics2D.OverlapCircleAll(chipCollider.bounds.center, 0.1f);
                foreach (var hit in hits)
                {
                    if (!hit.CompareTag("BetZone")) continue;
                    if (hit.TryGetComponent(out BetZone zone)) zones.Add(zone);
                }
            }

            int nonSlotExclusive = 0;
            foreach (var zone in zones)
            {
                if (zone.linkedSlot == null && !zone.allowOverlap)
                {
                    nonSlotExclusive++;
                }
            }

            if (nonSlotExclusive > 1)
            {
                Debug.LogWarning($"⚠️ Chip {chip.name} overlapped multiple incompatible zones. Ignoring bet.");
                continue;
            }

            bool isWinning = false;
            int payoutMultiplier = 0;

            // ----- Number Bets -----
            HashSet<GameObject> numberSlots = new HashSet<GameObject>();
            foreach (var z in zones)
            {
                if (z != null && z.linkedSlot != null)
                    numberSlots.Add(z.linkedSlot);
            }

            if (numberSlots.Count > 0)
            {
                payoutMultiplier = Mathf.RoundToInt(36f / numberSlots.Count);
                isWinning = numberSlots.Contains(winningSlot);
            }

            // ----- Group Bets -----
            if (!isWinning)
            {
                foreach (var betZone in zones)
                {
                    if (betZone == null || betZone.groupType == BetGroupType.None)
                        continue;

                    bool groupWin = betZone.groupType switch
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

                    if (groupWin)
                    {
                        isWinning = true;
                        payoutMultiplier = GetGroupPayout(betZone.groupType);
                        break;
                    }
                }
            }

            if (isWinning && payoutMultiplier > 0)
            {
                int chipValue = chipInfo != null ? chipInfo.chipValue : 1;
                int reward = chipValue * payoutMultiplier;
                rewardAmounts[chip] = reward;
                winningChips.Add(chip);
            }
            else
            {
                losingChips.Add(chip);
            }

            Debug.Log($"💰 Chip '{chip.name}' => {(isWinning ? "WIN ✅" : "LOSE ❌")}");
        }

        StartCoroutine(CollectChipsRoutine(winningChips, losingChips));
    }

    private IEnumerator CollectChipsRoutine(List<GameObject> winners, List<GameObject> losers)
    {
        yield return new WaitForSeconds(chipCollectDelay);

        foreach (var chip in winners)
        {
            if (chip == null) continue;
            if (winningChipDestination != null)
            {
                chip.transform.DOMove(winningChipDestination.position, chipMoveDuration)
                    .OnComplete(() =>
                    {
                        if (rewardAmounts.TryGetValue(chip, out int reward))
                            RewardPlayer(chip, reward);
                        Destroy(chip);
                    });
            }
            else
            {
                if (rewardAmounts.TryGetValue(chip, out int reward))
                    RewardPlayer(chip, reward);
                Destroy(chip);
            }
        }

        yield return new WaitForSeconds(chipLossCollectDelay);

        foreach (var chip in losers)
        {
            if (chip == null) continue;
            if (losingChipDestination != null)
            {
                chip.transform.DOMove(losingChipDestination.position, chipMoveDuration)
                    .OnComplete(() => Destroy(chip));
            }
            else
            {
                Destroy(chip);
            }
        }

        placedChips.Clear();

        TableReset();
    }

    private void TableReset()
    {
        if (gameTester == null)
            return;

        StartCoroutine(TableResetRoutine());
    }

    private IEnumerator TableResetRoutine()
    {
        yield return new WaitForSeconds(tableResetDelay);

        gameTester.ResetGame(!skipWheelReset);
    }

    private void RewardPlayer(GameObject chip, int amount)
    {
        PlayerCurrency.Instance?.AddCurrency(amount);
        Debug.Log($"💵 Rewarded {amount} for chip '{chip.name}'.");
    }

    private int GetGroupPayout(BetGroupType type)
    {
        return type switch
        {
            BetGroupType.Red => 2,
            BetGroupType.Black => 2,
            BetGroupType.Even => 2,
            BetGroupType.Odd => 2,
            BetGroupType.First_1_12 => 3,
            BetGroupType.Second_13_24 => 3,
            BetGroupType.Third_25_36 => 3,
            BetGroupType.Low_1_18 => 2,
            BetGroupType.High_19_36 => 2,
            BetGroupType.Top => 3,
            BetGroupType.Middle => 3,
            BetGroupType.Bottom => 3,
            _ => 0
        };
    }

    [ContextMenu("Gather Chips From Scene")]
    public void GatherChipsFromScene()
    {
        placedChips.Clear();
        var chips = FindObjectsByType<BettingChipDragger>(FindObjectsSortMode.None);
        foreach (var dragger in chips)
        {
            placedChips.Add(dragger.gameObject);
        }
        Debug.Log($"Collected {placedChips.Count} chips from the scene.");
    }
}
