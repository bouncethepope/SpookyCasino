using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BallLauncher : MonoBehaviour
{
    [Header("Launch Settings")]
    public float launchForce = 500f;
    public bool launchNow = false;

    private bool hasLaunched = false;
    private Rigidbody2D rb;

    [Header("Optional")]
    public BetCutoffManager cutoffManager; // Assign in inspector

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
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.AddForce(Vector2.up * launchForce, ForceMode2D.Impulse);

        hasLaunched = true;

        if (cutoffManager != null)
        {
            cutoffManager.ResetCutoff();          // Ensure it's clean
            cutoffManager.BeginCutoffMonitoring(); // Start watching wheel
        }

        Debug.Log("🚀 Ball launched!");
    }

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
