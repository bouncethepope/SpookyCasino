using UnityEngine;

public class PhysicsParityBootstrap : MonoBehaviour
{
    public int targetFps = 120; // or 144; vSync off gives you headroom
    public int vSync = 0;

    void Awake()
    {
        QualitySettings.vSyncCount = vSync;
        Application.targetFrameRate = targetFps;

        Time.fixedDeltaTime = 0.004f;   // 250 Hz physics
        Time.maximumDeltaTime = 0.048f; // limit catch-up (≈12 steps max)

        Physics2D.velocityIterations = 12;
        Physics2D.positionIterations = 6;
    }
}
