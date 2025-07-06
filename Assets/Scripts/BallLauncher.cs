using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BallLauncher : MonoBehaviour
{
    [Header("Launch Settings")]
    public float launchForce = 500f;
    public bool launchNow = false;

    private bool hasLaunched = false;
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        FreezeBall(); // Start frozen
    }

    void FixedUpdate()
    {
        if (launchNow && !hasLaunched)
        {
            LaunchBall();
        }
    }

    private void FreezeBall()
    {
        rb.bodyType = RigidbodyType2D.Static;
    }

    private void LaunchBall()
    {
        rb.bodyType = RigidbodyType2D.Dynamic; // Enable physics
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.AddForce(Vector2.up * launchForce, ForceMode2D.Impulse);

        hasLaunched = true;
        Debug.Log("🚀 Ball launched!");
    }

    // Optional: call this to reset the ball
    public void ResetLaunch()
    {
        hasLaunched = false;
        launchNow = false;
        FreezeBall();

        if (TryGetComponent(out RouletteBall ball))
        {
            ball.ResetBall();
        }
    }
}
