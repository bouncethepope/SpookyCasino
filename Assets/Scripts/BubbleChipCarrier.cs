using System;
using UnityEngine;

/// <summary>
/// Bubble that floats upward and can carry a single chip.
/// Clicking the bubble pops it, releasing the carried chip and
/// playing an optional particle effect.
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
        elapsed += Time.deltaTime;
        transform.Translate(Vector3.up * riseSpeed * Time.deltaTime);
        float x = initialX + Mathf.Sin(elapsed * swayFrequency) * swayAmplitude;
        Vector3 pos = transform.position;
        pos.x = x;
        transform.position = pos;

        if (carriedChip != null)
        {
            carriedChip.position = transform.position + chipOffset;
        }

        // If the bubble leaves the screen, pop it automatically.
        if (mainCamera != null)
        {
            Vector3 viewportPos = mainCamera.WorldToViewportPoint(transform.position);
            if (viewportPos.y > 1.1f)
            {
                Pop();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (carriedChip != null)
            return;

        // Carry any chip the bubble touches.
        if (other.TryGetComponent<BetChip>(out _))
        {
            carriedChip = other.transform;
            chipOffset = carriedChip.position - transform.position;
        }
    }

    /// <summary>Pops the bubble, releasing any carried chip.</summary>
    public void Pop()
    {
        if (carriedChip != null)
        {
            carriedChip = null;
        }

        if (popEffect != null)
        {
            Instantiate(popEffect, transform.position, Quaternion.identity);
        }

        onDestroyed?.Invoke();
        Destroy(gameObject);
    }
}