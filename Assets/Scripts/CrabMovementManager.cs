using UnityEngine;

/// <summary>
/// Controls random chip movements by hidden crabs during a round.
/// Attach to a GameManager object.
/// </summary>
public class CrabMovementManager : MonoBehaviour
{
    public static CrabMovementManager Instance { get; private set; }

    [Header("Crab Chances")]
    [Tooltip("Chance a chip moves when the ball starts spinning")] 
    [Range(0f,1f)] public float spinStartChance = 0.3f;
    [Tooltip("Chance a chip moves when bets are locked")] 
    [Range(0f,1f)] public float noMoreBetsChance = 0.2f;

    private void Awake()
    {
        Instance = this;
    }

    public void OnBallLaunched()
    {
        TryTriggerCrab(spinStartChance);
    }

    public void OnNoMoreBets()
    {
        TryTriggerCrab(noMoreBetsChance);
    }

    private void TryTriggerCrab(float chance)
    {
        if (Random.value > chance)
            return;

        var movers = FindObjectsByType<ChipCrabMover>(FindObjectsSortMode.None);
        if (movers == null || movers.Length == 0)
            return;

        var mover = movers[Random.Range(0, movers.Length)];
        if (mover != null)
            mover.MoveWithCrab();
    }
}
