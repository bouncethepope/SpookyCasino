using System;
using UnityEngine;

/// <summary>
/// Bubble that floats upward and can carry a single chip.
/// Clicking the bubble pops it, releasing the carried chip and
/// (optionally) playing a particle + sound effect.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class BubbleChipCarrier : MonoBehaviour
{
    [Header("Bubble Settings")]
    [Tooltip("Upward speed of the bubble")] public float riseSpeed = 1f;
    [Tooltip("Horizontal sway amplitude")] public float swayAmplitude = 0.2f;
    [Tooltip("Horizontal sway frequency")] public float swayFrequency = 1f;
    [Tooltip("Particle effect played when the bubble pops")] public ParticleSystem popEffect;

    /// <summary>Invoked when the bubble is destroyed.</summary>
    public Action onDestroyed;

    private Transform carriedChip;
    private Vector3 chipOffset;
    private Camera mainCamera;
    private Collider2D col;
    private float initialX;
    private float elapsed;

    private void Awake()
    {
        mainCamera = Camera.main;
        col = GetComponent<Collider2D>();
        col.isTrigger = true;
        initialX = transform.position.x;
    }

    private void Update()
    {
        // rise + sway
        elapsed += Time.deltaTime;
        transform.Translate(Vector3.up * riseSpeed * Time.deltaTime);
        var pos = transform.position;
        pos.x = initialX + Mathf.Sin(elapsed * swayFrequency) * swayAmplitude;
        transform.position = pos;

        if (carriedChip != null)
            carriedChip.position = transform.position + chipOffset;

        // Auto-pop when off the top of the screen (NO sound)
        if (mainCamera != null)
        {
            Vector3 vp = mainCamera.WorldToViewportPoint(transform.position);
            if (vp.y > 1.1f)
                Pop(playSound: false);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (carriedChip != null) return;

        // Carry any chip the bubble touches.
        if (other.TryGetComponent<BetChip>(out _))
        {
            carriedChip = other.transform;
            chipOffset = carriedChip.position - transform.position;
        }
    }

    // Mouse click pops the bubble (WITH sound)
    private void OnMouseDown()
    {
        Pop(playSound: true);
    }

    /// <summary>Pops the bubble, releasing any carried chip.</summary>
    /// <param name="playSound">If true, plays the bubble pop SFX.</param>
    public void Pop(bool playSound = true)
    {
        if (carriedChip != null)
            carriedChip = null;

        if (playSound)
            AudioManager.Instance?.PlayBubblePop();

        if (popEffect != null)
            Instantiate(popEffect, transform.position, Quaternion.identity);

        onDestroyed?.Invoke();
        Destroy(gameObject);
    }
}
