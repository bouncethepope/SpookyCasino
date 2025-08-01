using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Controls random chip movements by hidden crabs during a round.
/// Attach to a GameManager object.
/// </summary>
public class CrabMovementManager : MonoBehaviour
{
    public static CrabMovementManager Instance { get; private set; }

    [Header("Crab Chances")]
    [Tooltip("Chance a chip moves when the ball starts spinning")]
    [Range(0f, 1f)] public float spinStartChance = 0.3f;
    [Tooltip("Chance a chip moves when bets are locked")]
    [Range(0f, 1f)] public float noMoreBetsChance = 0.2f;

    [Header("Crab Settings")]
    [Tooltip("Number of crabs to check when an event occurs")]
    public int crabsToCheck = 1;

    private void Awake()
    {
        Instance = this;
    }

    public void OnBallLaunched()
    {
        TryTriggerCrabs(spinStartChance);
    }

    public void OnNoMoreBets()
    {
        TryTriggerCrabs(noMoreBetsChance);
    }

    private void TryTriggerCrabs(float chance)
    {
        var movers = FindObjectsByType<ChipCrabMover>(FindObjectsSortMode.None);
        if (movers == null || movers.Length == 0)
            return;

        int count = Mathf.Min(crabsToCheck, movers.Length);
        List<int> chosenIndices = new List<int>();

        while (chosenIndices.Count < count)
        {
            int randomIndex = Random.Range(0, movers.Length);
            if (chosenIndices.Contains(randomIndex))
                continue;

            chosenIndices.Add(randomIndex);

            if (Random.value <= chance)
            {
                movers[randomIndex].MoveWithCrab();
            }
        }
    }
}
