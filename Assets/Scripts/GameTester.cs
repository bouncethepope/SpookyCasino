using UnityEngine;

public class GameTester : MonoBehaviour
{
    [Header("References")]
    public BallLauncher ballLauncher;
    public WheelSpinner wheelSpinner;
    public BetManager betManager;
    public BetEvaluator betEvaluator;
    public BetCutoffManager betCutoffManager; 

    private Vector3 ballStartPos;
    private Quaternion ballStartRot;

    private void Awake()
    {
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

        // Destroy all chips in the scene
        foreach (var chip in FindObjectsByType<BettingChipDragger>(FindObjectsSortMode.None))
        {
            if (chip != null)
            {
                Destroy(chip.gameObject);
            }
        }

        betManager?.ClearAllBets();
        betEvaluator?.placedChips.Clear();

        if (betCutoffManager != null)
        {
            betCutoffManager.UnlockBets(); 
        }
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

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            ResetGame();
        }
    }
}
