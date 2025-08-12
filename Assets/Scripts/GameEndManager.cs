using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles showing the end game UI and resetting the scene.
/// </summary>
public class GameEndManager : MonoBehaviour
{
    public static GameEndManager Instance { get; private set; }

    [Header("End Game UI")]
    public GameObject winScreen;
    public GameObject loseScreen;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (winScreen != null)
            winScreen.SetActive(false);
        if (loseScreen != null)
            loseScreen.SetActive(false);
    }

    /// <summary>
    /// Displays either the win or lose screen based on the result.
    /// </summary>
    public void ShowEndScreen(bool playerWon)
    {
        ChipBag.betsLocked = true;
        BettingChipDragger.betsLocked = true;

        if (playerWon)
        {
            if (winScreen != null)
                winScreen.SetActive(true);
            Debug.Log("Game Win");
        }
        else
        {
            if (loseScreen != null)
                loseScreen.SetActive(true);
            Debug.Log("Game Loss");
        }
    }

    /// <summary>
    /// Reloads the current scene, restarting the game from the beginning.
    /// </summary>
    public void ResetGame()
    {
        ChipBag.betsLocked = false;
        BettingChipDragger.betsLocked = false;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}