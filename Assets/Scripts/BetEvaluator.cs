// Bet Evaluator checks the physics of all bets, to see if they are winners or not.


using UnityEngine;

public class BetEvaluator : MonoBehaviour
{
    [Tooltip("All chips currently placed on the table")]
    public BettingChipDragger[] placedChips;

    [ContextMenu("🧮 Evaluate Bets (Manual)")]
    public void EvaluateBetsManually()
    {
        // Use modern Unity-safe method
        var ball = FindFirstObjectByType<RouletteBall>();
        if (ball == null)
        {
            Debug.LogWarning("No RouletteBall found in the scene.");
            return;
        }

        GameObject winningSlot = ball.GetWinningSlot();
        if (winningSlot == null)
        {
            Debug.Log("❓ No winning slot registered yet.");
            return;
        }

        EvaluateBets(winningSlot);
    }

    public void EvaluateBets(GameObject winningSlot)
    {
        Debug.Log($"🎲 Evaluating bets against winning slot: {winningSlot.name}");

        foreach (var chip in placedChips)
        {
            Collider2D chipCollider = chip.GetComponent<Collider2D>();
            if (chipCollider == null)
            {
                Debug.LogWarning($"⚠️ Chip '{chip.name}' has no Collider2D.");
                continue;
            }

            Collider2D[] hits = Physics2D.OverlapCircleAll(chipCollider.bounds.center, 0.1f);
            bool isWinning = false;

            foreach (var hit in hits)
            {
                BetZone betZone = hit.GetComponent<BetZone>();
                if (betZone != null && betZone.linkedSlot == winningSlot)
                {
                    isWinning = true;
                    break;
                }
            }

            Debug.Log($"💰 Chip '{chip.name}' => {(isWinning ? "WIN ✅" : "LOSE ❌")}");
        }
    }
}
