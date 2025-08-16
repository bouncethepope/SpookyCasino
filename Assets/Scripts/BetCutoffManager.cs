using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Locks chip interaction once the wheel slows below a configured speed.
/// Attach to an object like the GameManager.
/// </summary>
public class BetCutoffManager : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Wheel spinner used to determine current speed")] public WheelSpinner wheelSpinner;
    [Tooltip("UI element shown when bets are locked")] public GameObject noMoreBetsUI;
    [UnityEngine.Serialization.FormerlySerializedAs("noMoreBetsUI")]
    [Tooltip("Sign shown when bets are locked")] public NoMoreBetsSign noMoreBetsSign;

    [Header("Cutoff Settings")]
    [Tooltip("When the wheel speed goes below this value, betting is locked")]
    public float cutoffSpeed = 200f;

    private bool betsLocked = false;
    private bool cutoffTriggered = false;
    private bool monitorSpin = false;

    private void Start()
    {
        if (noMoreBetsUI != null)
            noMoreBetsUI.SetActive(false);
        if (noMoreBetsSign != null)
            noMoreBetsSign.ResetSign();

    }

    private void Update()
    {
        if (betsLocked || wheelSpinner == null || cutoffTriggered || !monitorSpin)
            return;

        if (wheelSpinner.IsSpinning() && Mathf.Abs(wheelSpinner.GetCurrentSpinSpeed()) <= cutoffSpeed)
        {
            LockBets();
        }
    }

    public void LockBets()
    {
        betsLocked = true;
        cutoffTriggered = true;
        monitorSpin = false;
        ChipBag.betsLocked = true;
        BettingChipDragger.betsLocked = true;

        var chips = FindObjectsByType<BettingChipDragger>(FindObjectsSortMode.None);
        foreach (var dragger in chips)
        {
            dragger.ForceEndDrag();
        }

        CrabMovementManager.Instance?.OnNoMoreBets();

        // bets are locked; chips can't be moved anymore

        if (noMoreBetsUI != null)
            noMoreBetsUI.SetActive(true);
        if (noMoreBetsSign != null)
            noMoreBetsSign.Show();
    }

    public void UnlockBets()
    {
        betsLocked = false;
        ChipBag.betsLocked = false;
        BettingChipDragger.betsLocked = false;

        if (noMoreBetsUI != null)
            noMoreBetsUI.SetActive(false);
        if (noMoreBetsSign != null)
            noMoreBetsSign.ResetSign();

        // capture currency baseline for the upcoming round
        BetHistorySign.Instance?.StartRound();
    }

    public void ResetCutoff()
    {
        cutoffTriggered = false;
        monitorSpin = false;
    }

    /// <summary>
    /// Called when the ball is launched to begin checking for cutoff conditions.
    /// </summary>
    public void BeginCutoffMonitoring()
    {
        monitorSpin = true;
        //FinalChipBag.LockBag();
    }
}