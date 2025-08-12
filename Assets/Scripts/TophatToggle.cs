using UnityEngine;

public class TophatToggle : MonoBehaviour
{
    [Tooltip("Reference to the Tophat GameObject under this Dolly.")]
    public GameObject tophat;

    private void Start()
    {
        if (tophat == null)
        {
            Debug.LogWarning("Tophat reference is missing on " + gameObject.name);
            return;
        }

        // Check if the PersistentGameState exists and whether the player has won
        if (PersistentGameState.Instance != null && PersistentGameState.Instance.playerHasWon)
        {
            tophat.SetActive(true);
        }
        else
        {
            tophat.SetActive(false);
        }
    }
}