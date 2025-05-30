using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ConcaveForceZone : MonoBehaviour
{
    [Tooltip("Strength of pull toward the center of the zone.")]
    public float pullForce = 5f;

    [Tooltip("Target object to affect (e.g. the ball).")]
    public Rigidbody2D targetBody;

    private void OnTriggerStay2D(Collider2D other)
    {
        if (targetBody == null || other.attachedRigidbody != targetBody)
            return;

        Vector2 directionToCenter = (Vector2)transform.position - targetBody.position;
        targetBody.AddForce(directionToCenter.normalized * pullForce * Time.fixedDeltaTime, ForceMode2D.Force);
    }
}
