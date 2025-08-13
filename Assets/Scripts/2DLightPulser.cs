using UnityEngine;
using UnityEngine.Rendering.Universal; // Required for Light2D

[RequireComponent(typeof(Light2D))]
public class Light2DPulser : MonoBehaviour
{
    [Header("Pulse Settings")]
    [Tooltip("Minimum radius of the light during the pulse.")]
    public float minRadius = 2f;

    [Tooltip("Maximum radius of the light during the pulse.")]
    public float maxRadius = 5f;

    [Tooltip("Speed of the pulse cycle.")]
    public float pulseSpeed = 2f;

    [Tooltip("Phase offset for the sine wave, if you want multiple lights out of sync.")]
    public float phaseOffset = 0f;

    [Tooltip("If true, uses smooth sine-wave pulsing. If false, uses ping-pong linear pulsing.")]
    public bool smoothPulse = true;

    private Light2D light2D;
    private float baseRadius;

    private void Awake()
    {
        light2D = GetComponent<Light2D>();
        baseRadius = light2D.pointLightOuterRadius;
    }

    private void Update()
    {
        float t;

        if (smoothPulse)
        {
            // Smooth sine-based pulsing
            t = (Mathf.Sin(Time.time * pulseSpeed + phaseOffset) + 1f) / 2f; // 0–1
        }
        else
        {
            // Linear ping-pong pulsing
            t = Mathf.PingPong(Time.time * pulseSpeed, 1f);
        }

        light2D.pointLightOuterRadius = Mathf.Lerp(minRadius, maxRadius, t);
    }
}
