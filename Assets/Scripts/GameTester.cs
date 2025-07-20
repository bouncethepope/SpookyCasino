using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameTester : MonoBehaviour
{
    [Header("References")]
    public BallLauncher ballLauncher;
    public WheelSpinner wheelSpinner;
    public BetManager betManager;
    public BetEvaluator betEvaluator;
    public BetCutoffManager betCutoffManager;
    public WinningSlotDisplay slotDisplay;

    [Header("Dynamic Launch Settings")]
    [Tooltip("Random range added to the wheel spin speed (\u00b1 value).")]
    public float spinSpeedVariance = 50f;

    [Tooltip("Random range added to the ball launch force (\u00b1 value).")]
    public float launchForceVariance = 2f;

    [Tooltip("Delay between starting the wheel spin and launching the ball.")]
    public float launchDelay = 0.5f;

    [Tooltip("Possible spawn positions for the ball.")]
    public Transform[] launchPositions;

    private float baseSpinSpeed;
    private float baseLaunchForce;

    private Vector3 ballStartPos;
    private Quaternion ballStartRot;

    private bool isResetting = false;

    private void Awake()
    {
        if (ballLauncher != null)
        {
            ballStartPos = ballLauncher.transform.position;
            ballStartRot = ballLauncher.transform.rotation;
            baseLaunchForce = ballLauncher.launchForce;
        }

        if (wheelSpinner != null)
        {
            baseSpinSpeed = wheelSpinner.initialSpinSpeed;
        }
    }

    [ContextMenu("Launch Ball")]
    public void LaunchBall()
    {
        if (ballLauncher != null)
        {
            ballLauncher.launchNow = true;
        }
    }

    [ContextMenu("Spin Wheel")]
    public void SpinWheel()
    {
        wheelSpinner?.StartSpin();
    }

    [ContextMenu("Spin & Launch")]
    public void SpinAndLaunch()
    {
        StartCoroutine(SpinAndLaunchRoutine());
    }

    private System.Collections.IEnumerator SpinAndLaunchRoutine()
    {
        if (wheelSpinner != null)
        {
            float spin = baseSpinSpeed + Random.Range(-spinSpeedVariance, spinSpeedVariance);
            wheelSpinner.initialSpinSpeed = spin;
            wheelSpinner.StartSpin();
        }

        yield return new WaitForSeconds(launchDelay);

        if (ballLauncher != null)
        {
            if (launchPositions != null && launchPositions.Length > 0)
            {
                int index = Random.Range(0, launchPositions.Length);
                Transform pos = launchPositions[index];
                ballLauncher.transform.position = pos.position;
                ballLauncher.transform.rotation = pos.rotation;
            }

            ballLauncher.launchForce = baseLaunchForce + Random.Range(-launchForceVariance, launchForceVariance);
            ballLauncher.launchNow = true;
        }
    }


    [ContextMenu("Reset Game")]
    public void ResetGame()
    {
        ResetGame(true);
    }

    public void ResetGame(bool resetWheel)
    {
        if (isResetting)
            return;

        isResetting = true;

        if (ballLauncher != null)
        {
            ballLauncher.ResetLaunch();
            ballLauncher.transform.position = ballStartPos;
            ballLauncher.transform.rotation = ballStartRot;
            ballLauncher.launchForce = baseLaunchForce;
        }

        if (wheelSpinner != null)
        {
            if (resetWheel)
            {
                wheelSpinner.ResetSpin();
            }

            // Always restore the base spin speed for the next round
            wheelSpinner.initialSpinSpeed = baseSpinSpeed;
        }

        // Destroy all chips in the scene using a snapshot to avoid collection
        // modification issues when many chips are present
        var chips = FindObjectsByType<BettingChipDragger>(FindObjectsSortMode.None);
        foreach (var chip in chips)
        {
            Destroy(chip.gameObject);
        }

        betManager?.ClearAllBets();
        betEvaluator?.placedChips.Clear();

        if (betCutoffManager != null)
        {
            betCutoffManager.UnlockBets();
            betCutoffManager.ResetCutoff();
        }

        slotDisplay?.ResetDisplay();

        isResetting = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            LaunchBall();
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            SpinWheel();
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            SpinAndLaunch();
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            ResetGame();
        }
    }
}

