using UnityEngine;
using System.Collections;

public class PufferfishSpawner : MonoBehaviour
{
    [Header("Pufferfish Prefabs")]
    public Pufferfish pufferfishPrefab;
    [Tooltip("Optional: Special pufferfish with hat for win condition.")]
    public Pufferfish pufferfishWithHatPrefab;

    [Header("Path Points")]
    public Transform startPoint;
    public Transform endPoint;

    private Pufferfish current;

    public void Spawn()
    {
        if (startPoint == null || endPoint == null)
            return;

        if (current != null)
            return;

        // Decide which prefab to use
        Pufferfish prefabToSpawn = pufferfishPrefab;

        if (pufferfishWithHatPrefab != null &&
            PersistentGameState.Instance != null &&
            PersistentGameState.Instance.playerHasWon)
        {
            prefabToSpawn = pufferfishWithHatPrefab;
        }

        if (prefabToSpawn == null)
            return;

        current = Instantiate(prefabToSpawn, startPoint.position, Quaternion.identity);
        current.startPoint = startPoint;
        current.endPoint = endPoint;

        // Fix: Delay Activate by one frame
        StartCoroutine(ActivateNextFrame(current));
    }

    private IEnumerator ActivateNextFrame(Pufferfish pf)
    {
        yield return null; // Wait one frame
        pf.Activate();
    }
}
