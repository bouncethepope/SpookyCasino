using System.Collections.Generic;
using UnityEngine;

public class GameTester : MonoBehaviour
{
    [Header("References")]
    public BallLauncher ballLauncher;
    public WheelSpinner wheelSpinner;
    public BetManager betManager;
    public BetEvaluator betEvaluator;

    private List<(Transform tf, Vector3 pos, Quaternion rot)> chipStarts = new();
    private Vector3 ballStartPos;
    private Quaternion ballStartRot;

    private void Awake()
    {
        foreach (var chip in FindObjectsOfType<BettingChipDragger>())
        {
            chipStarts.Add((chip.transform, chip.transform.position, chip.transform.rotation));
        }

        if (ballLauncher != null)
        {
            ballStartPos = ballLauncher.transform.position;
            ballStartRot = ballLauncher.transform.rotation;
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

    [ContextMenu("Reset Game")]
    public void ResetGame()
    {
        if (ballLauncher != null)
        {
            ballLauncher.ResetLaunch();
            ballLauncher.transform.position = ballStartPos;
            ballLauncher.transform.rotation = ballStartRot;
        }

        if (wheelSpinner != null)
        {
            wheelSpinner.ResetSpin();
        }

        foreach (var info in chipStarts)
        {
            if (info.tf != null)
            {
                info.tf.position = info.pos;
                info.tf.rotation = info.rot;
            }
        }

        betManager?.ClearAllBets();
        betEvaluator?.placedChips.Clear();
    }
}
