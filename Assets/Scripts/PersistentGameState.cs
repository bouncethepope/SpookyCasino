using UnityEngine;

/// <summary>
/// Simple persistent session state for your game.
/// Lives across scene reloads (session-only, no disk writes).
/// </summary>
public class PersistentGameState : MonoBehaviour
{
    private static PersistentGameState instance;
    public static PersistentGameState Instance
    {
        get
        {
            if (instance == null)
            {
                var go = new GameObject("PersistentGameState");
                instance = go.AddComponent<PersistentGameState>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    [Header("Flags (session only)")]
    [Tooltip("True if the player has reached the win state during this session.")]
    [SerializeField] public bool playerHasWon = false;

    /// <summary>Public read-only access. Use PlayerHasWon() / ResetWinState() to change.</summary>
    public bool PlayerHasWonFlag => playerHasWon;

    // Ensure the singleton exists before any scene loads (prevents race conditions).
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        _ = Instance;
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>Mark the game as won for the current session.</summary>
    public void PlayerHasWon()
    {
        playerHasWon = true;
    }

    /// <summary>Clear the win flag (e.g., starting a new run within the same session).</summary>
    public void ResetWinState()
    {
        playerHasWon = false;
    }
}
