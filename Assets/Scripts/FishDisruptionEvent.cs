using UnityEngine;

/// <summary>
/// Controls a single fish disruption event that moves from a start point to an end point with a
/// sine wave offset. The fish uses a Rigidbody2D for movement so it can push betting chips out of
/// the way.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class FishDisruptionEvent : MonoBehaviour
{
    private Vector2 startPoint;
    private Vector2 endPoint;
    private float speed;
    private float waveAmplitude;
    private float waveFrequency;
    private Rigidbody2D rb;
    private Collider2D col;
    private Vector2 direction;
    private Vector2 perpendicular;
    private float time;
    private bool ignoreCollisions;
    [Tooltip("Speed used once the fish drops its collider and heads straight to the exit")]
    [SerializeField] private float autoMoveSpeed = 3f;

    /// <summary>
    /// Initializes the fish disruption event with movement settings.
    /// </summary>
    public void Initialize(Vector2 start, Vector2 end, float swimSpeed, float amplitude, float frequency, Collider2D[] ignoreColliders = null)
    {
        startPoint = start;
        endPoint = end;
        speed = swimSpeed;
        waveAmplitude = amplitude;
        waveFrequency = frequency;

        transform.position = startPoint;
        direction = (endPoint - startPoint).normalized;
        perpendicular = new Vector2(-direction.y, direction.x); // 90 degree rotation

        if (ignoreColliders != null)
        {
            foreach (var other in ignoreColliders)
            {
                if (other != null)
                    Physics2D.IgnoreCollision(col, other, true);
            }
        }
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
    }

    private void FixedUpdate()
    {
        time += Time.fixedDeltaTime;

        if (time >= 10f && !ignoreCollisions)
        {
            ignoreCollisions = true;
            col.enabled = false;
            speed = autoMoveSpeed;
        }

        // Calculate velocity for straight movement plus sine wave offset
        if (ignoreCollisions)
        {
            Vector2 toEnd = (endPoint - (Vector2)transform.position).normalized;
            rb.linearVelocity = toEnd * speed;
        }
        else
        {
            float wave = Mathf.Cos(time * waveFrequency) * waveFrequency * waveAmplitude;
            Vector2 velocity = direction * speed + perpendicular * wave;
            rb.linearVelocity = velocity;
        }

        // Destroy once past end point
        float travelled = Vector2.Dot((Vector2)transform.position - startPoint, direction);
        float totalDistance = Vector2.Distance(startPoint, endPoint);
        if (travelled >= totalDistance)
            Destroy(gameObject);
    }
}