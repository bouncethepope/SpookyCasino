using System;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class BubbleChipCarrier : MonoBehaviour
{
    [Header("Bubble Settings")]
    public float riseSpeed = 1f;
    public float swayAmplitude = 0.2f;
    public float swayFrequency = 1f;
    public ParticleSystem popEffect;

    [Header("Audio")]
    [SerializeField] private BubblePopSound popSound; // ← reference to the audio helper

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

        // Auto-find if not wired in Inspector
        if (popSound == null) popSound = FindAnyObjectByType<BubblePopSound>();
    }

    private void Update()
    {
        elapsed += Time.deltaTime;
        transform.Translate(Vector3.up * riseSpeed * Time.deltaTime);
        var pos = transform.position;
        pos.x = initialX + Mathf.Sin(elapsed * swayFrequency) * swayAmplitude;
        transform.position = pos;

        if (carriedChip != null)
            carriedChip.position = transform.position + chipOffset;

        if (mainCamera != null)
        {
            Vector3 vp = mainCamera.WorldToViewportPoint(transform.position);
            if (vp.y > 1.1f) Pop();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (carriedChip != null) return;
        if (other.TryGetComponent<BetChip>(out _))
        {
            carriedChip = other.transform;
            chipOffset = carriedChip.position - transform.position;
        }
    }

    public void Pop()
    {
        if (carriedChip != null) carriedChip = null;

        // play sound regardless of the particle existing
        AudioManager.Instance?.PlayBubblePop();

        if (popEffect != null)
            Instantiate(popEffect, transform.position, Quaternion.identity);

        onDestroyed?.Invoke();
        Destroy(gameObject);
    }
}
