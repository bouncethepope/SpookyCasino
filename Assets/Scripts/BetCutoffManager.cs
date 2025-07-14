using UnityEngine;

/// <summary>
/// Locks chip interaction once the wheel slows below a configured speed.
/// Attach to an object like the GameManager.
/// </summary>
public class BetCutoffManager : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Wheel spinner used to determine current speed")] public WheelSpinner wheelSpinner;
    [Tooltip("UI element shown when bets are locked")] public GameObject noMoreBetsUI;

    [Header("Cutoff Settings")]
    [Tooltip("When the wheel speed goes below this value, betting is locked")]
    public float cutoffSpeed = 200f;

    private bool betsLocked = false;

    private void Start()
    {
        if (noMoreBetsUI != null)
            noMoreBetsUI.SetActive(false);
    }

    private void Update()
    {
        if (betsLocked || wheelSpinner == null)
            return;

        if (Mathf.Abs(wheelSpinner.GetCurrentSpinSpeed()) <= cutoffSpeed)
        {
            LockBets();
        }
    }

    /// <summary>
    /// Locks all chip and bag interactions and shows the "no more bets" prompt.
    /// </summary>
    public void LockBets()
    {
        betsLocked = true;
        ChipBag.betsLocked = true;
        BettingChipDragger.betsLocked = true;

        foreach (var dragger in FindObjectsByType<BettingChipDragger>(FindObjectsSortMode.None))
        {
            dragger.ForceEndDrag();
        }

        if (noMoreBetsUI != null)
            noMoreBetsUI.SetActive(true);
    }

    /// <summary>
    /// Unlocks betting so chips can be moved again.
    /// </summary>
    public void UnlockBets()
    {
        betsLocked = false;
        ChipBag.betsLocked = false;
        BettingChipDragger.betsLocked = false;

        if (noMoreBetsUI != null)
            noMoreBetsUI.SetActive(false);
    }
}
