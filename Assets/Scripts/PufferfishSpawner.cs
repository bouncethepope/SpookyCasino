using UnityEngine;
using System.Collections;

public class PufferfishSpawner : MonoBehaviour
{
    public Pufferfish pufferfishPrefab;
    public Transform startPoint;
    public Transform endPoint;

    private Pufferfish current;

    public void Spawn()
    {
        if (pufferfishPrefab == null || startPoint == null || endPoint == null)
            return;

        if (current != null)
            return;

        current = Instantiate(pufferfishPrefab, startPoint.position, Quaternion.identity);
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
