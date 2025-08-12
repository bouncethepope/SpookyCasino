using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles showing the end game UI and resetting the scene.
/// Also integrates session win state + reapplies win visuals after reload.
/// </summary>
public class GameEndManager : MonoBehaviour
{
    public static GameEndManager Instance { get; private set; }

    [Header("End Game UI")]
    public GameObject winScreen;
    public GameObject loseScreen;

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
    }

    /// <summary>
    /// Displays either the win or lose screen based on the result.
    /// Also records win state if the player won.
    /// </summary>
    public void ShowEndScreen(bool playerWon)
    {
        ChipBag.betsLocked = true;
        BettingChipDragger.betsLocked = true;

        if (playerWon)
        {
            // Record the win for this session (persists across scene reloads)
            PersistentGameState.Instance.PlayerHasWon();

            if (winScreen != null) winScreen.SetActive(true);
            Debug.Log("Game Win");
        }
        else
        {
            if (loseScreen != null) loseScreen.SetActive(true);
            Debug.Log("Game Loss");
        }
    }

    /// <summary>
    /// Reloads the current scene, restarting the game from the beginning.
    /// Re-applies win visuals after reload if the player has already won this session.
    /// </summary>
    public void ResetGame()
    {
        ChipBag.betsLocked = false;
        BettingChipDragger.betsLocked = false;

        // Re-apply win visuals once the scene is fully loaded
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        if (PersistentGameState.Instance.PlayerHasWonFlag)
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
