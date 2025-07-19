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

        // Only lock bets if the wheel has started spinning and is now slowing down
        if (wheelSpinner.IsSpinning() && Mathf.Abs(wheelSpinner.GetCurrentSpinSpeed()) <= cutoffSpeed)
        {
            LockBets();
        }
    }



    /// Locks all chip and bag interactions and shows the "no more bets" prompt.
    public void LockBets()
    {
        betsLocked = true;
        ChipBag.betsLocked = true;
        BettingChipDragger.betsLocked = true;

        var chips = FindObjectsByType<BettingChipDragger>(FindObjectsSortMode.None);
        foreach (var dragger in chips)
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