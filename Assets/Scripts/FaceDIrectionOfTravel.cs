using UnityEngine;

/// <summary>
/// Rotates an object to face the direction it is currently travelling.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class FaceDirectionOfTravel : MonoBehaviour
{
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void LateUpdate()
    {
        Vector2 v = rb.linearVelocity;
        if (v.sqrMagnitude > 0.0001f)
        {
            float angle = Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;
            rb.rotation = angle;
        }
    }
}