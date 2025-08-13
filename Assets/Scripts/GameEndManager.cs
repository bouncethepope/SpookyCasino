using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles showing the end game UI, triggering final statistics, and resetting the scene.
/// Also integrates session win state and reapplies win visuals after reload.
/// </summary>
public class GameEndManager : MonoBehaviour
{
    public static GameEndManager Instance { get; private set; }

    [Header("End Game UI")]
    public GameObject winScreen;
    public GameObject loseScreen;

    [Header("Stats")]
    public StatsHandler statsHandler;

    [Header("Win Visuals (optional)")]
    [Tooltip("Invoked after a scene reload if the player has previously won this session.")]
    public UnityEvent onApplyWinVisuals;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (winScreen != null) winScreen.SetActive(false);
        if (loseScreen != null) loseScreen.SetActive(false);

        if (statsHandler == null)
            statsHandler = StatsHandler.Instance ?? FindFirstObjectByType<StatsHandler>();
    }

    /// <summary>
    /// Public trigger: call when the player wins.
    /// </summary>
    public void TriggerWin()
    {
        ShowEndScreen(playerWon: true);
    }

    /// <summary>
    /// Public trigger: call when the player loses.
    /// </summary>
    public void TriggerLose()
    {
        ShowEndScreen(playerWon: false);
    }

    /// <summary>
    /// Core handler for end-state presentation and bookkeeping.
    /// </summary>
    public void ShowEndScreen(bool playerWon)
    {
        ChipBag.betsLocked = true;
        BettingChipDragger.betsLocked = true;

        ShowEndStats();

        if (playerWon)
        {
            // Record the win for this session (persists across scene reloads)
            if (PersistentGameState.Instance != null)
                PersistentGameState.Instance.PlayerHasWon();

            if (winScreen != null) winScreen.SetActive(true);
            if (loseScreen != null) loseScreen.SetActive(false);
            Debug.Log("Game Win");
        }
        else
        {
            if (loseScreen != null) loseScreen.SetActive(true);
            if (winScreen != null) winScreen.SetActive(false);
            Debug.Log("Game Loss");
        }
    }

    /// <summary>
    /// Displays end-of-round chip statistics via the StatsHandler.
    /// </summary>
    public void ShowEndStats()
    {
        if (statsHandler != null)
            statsHandler.ShowStats();
        else
            StatsHandler.Instance?.ShowStats();
    }

    /// <summary>
    /// Reloads the current scene, restarting the game from the beginning.
    /// Re-applies win visuals after reload if the player has already won this session.
    /// </summary>
    public void ResetGame()
    {
        statsHandler?.Hide();

        ChipBag.betsLocked = false;
        BettingChipDragger.betsLocked = false;

        // Re-apply win visuals once the scene is fully loaded
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        if (PersistentGameState.Instance != null && PersistentGameState.Instance.PlayerHasWonFlag)
        {
            ApplyWinVisuals();
        }
    }

    /// <summary>
    /// Apply your win-dependent content (e.g., swap in hat-prefabs, enable celebratory objects).
    /// Wire these up in the Inspector via the UnityEvent.
    /// </summary>
    private void ApplyWinVisuals()
    {
        if (onApplyWinVisuals != null)
            onApplyWinVisuals.Invoke();
    }
}