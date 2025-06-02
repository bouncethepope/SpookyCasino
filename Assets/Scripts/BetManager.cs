using System.Collections.Generic;
using UnityEngine;

public class BetManager : MonoBehaviour
{
    [Header("Balls in Play")]
    public List<RouletteBall> balls;

    [Header("Placed Bets")]
    public List<RouletteBet> placedBets;

    private HashSet<RouletteBall> evaluatedBalls = new HashSet<RouletteBall>();

    void Update()
    {
        foreach (var ball in balls)
        {
            GameObject winningSlot = ball.GetWinningSlot();

            if (winningSlot != null && !evaluatedBalls.Contains(ball))
            {
                Debug.Log($"🎉 Ball settled in: {winningSlot.name}");

                foreach (var bet in placedBets)
                {
                    bet.EvaluateBet(winningSlot);
                }

                evaluatedBalls.Add(ball);
            }
        }
    }

    public void ClearAllBets()
    {
        placedBets.Clear();
        evaluatedBalls.Clear();
    }
}
