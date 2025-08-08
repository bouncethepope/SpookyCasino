using UnityEngine;
using System.Collections;

/// <summary>
/// Controls the no-more-bets sign animation. Moves between two points
/// and jitters slightly while displayed.
/// </summary>
public class NoMoreBetsSign : MonoBehaviour
{
    [Header("Movement Points")]
    [Tooltip("Starting position for the sign")]
    public Transform startPoint;
    [Tooltip("End position for the sign")]
    public Transform endPoint;

    [Header("Timing")]
    [Tooltip("Duration of the move between points")]
    public float moveDuration = 0.5f;
    [Tooltip("Time to stay at the end position before returning")]
    public float returnDelay = 2f;

    [Header("Jitter")]
    [Tooltip("Amount of positional jitter while holding at the end position")]
    public float jitterAmount = 0.02f;
    [Tooltip("Maximum rotational jitter in degrees")]
    public float rotationJitter = 2f;
    [Tooltip("Speed of the jitter animation")]
    public float jitterSpeed = 5f;

    private Coroutine routine;

    private void Awake()
    {
        if (startPoint != null)
        {
            transform.position = startPoint.position;
            transform.rotation = startPoint.rotation;
        }
    }

    /// <summary>
    /// Triggers the sign animation.
    /// </summary>
    public void Show()
    {
        if (routine != null)
            StopCoroutine(routine);
        routine = StartCoroutine(SignRoutine());
    }

    /// <summary>
    /// Immediately returns the sign to its start position.
    /// </summary>
    public void ResetSign()
    {
        if (routine != null)
            StopCoroutine(routine);
        routine = null;
        if (startPoint != null)
        {
            transform.position = startPoint.position;
            transform.rotation = startPoint.rotation;
        }
    }

    private IEnumerator SignRoutine()
    {
        yield return Move(startPoint.position, endPoint.position, startPoint.rotation, endPoint.rotation);

        float elapsed = 0f;
        while (elapsed < returnDelay)
        {
            elapsed += Time.deltaTime;
            Vector3 offset = new Vector3(
                (Mathf.PerlinNoise(Time.time * jitterSpeed, 0f) - 0.5f) * jitterAmount,
                (Mathf.PerlinNoise(0f, Time.time * jitterSpeed) - 0.5f) * jitterAmount,
                0f);
            float rot = Mathf.Sin(Time.time * jitterSpeed) * rotationJitter;
            transform.position = endPoint.position + offset;
            transform.rotation = endPoint.rotation * Quaternion.Euler(0f, 0f, rot);
            yield return null;
        }

        yield return Move(endPoint.position, startPoint.position, endPoint.rotation, startPoint.rotation);
        ResetSign();
    }

    private IEnumerator Move(Vector3 fromPos, Vector3 toPos, Quaternion fromRot, Quaternion toRot)
    {
        float time = 0f;
        while (time < 1f)
        {
            time += Time.deltaTime / moveDuration;
            transform.position = Vector3.Lerp(fromPos, toPos, time);
            transform.rotation = Quaternion.Lerp(fromRot, toRot, time);
            yield return null;
        }
    }
}
