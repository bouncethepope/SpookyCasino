using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RouletteBall : MonoBehaviour
{
    [Header("Detection Settings")]
    [Tooltip("Seconds the ball must remain in a slot before locking in.")]
    public float timeToConfirm = 3f;
    public WheelSpinner wheelSpinner;

    [Tooltip("Wheel spin speed below which final slot will be checked (deg/sec)")]
    public float spinLockThreshold = 30f;

    [Tooltip("Velocity magnitude that counts as the ball starting to move")]
    public float movementStartThreshold = 0.1f;

    [Header("Snap Settings")]
    [Tooltip("Duration of the snap animation when locking into the winning slot.")]
    public float snapDuration = 0.1f;

    [Header("Audio")]
    [Tooltip("Possible clack sounds when the ball collides with something")] public AudioClip[] collisionClacks;
    [Tooltip("Possible sounds when the ball settles into a slot")] public AudioClip[] slotDropSounds;
    [Tooltip("Minimum seconds between consecutive clack sounds")]
    public float clackCooldown = 0.05f;

    private readonly List<Collider2D> touchingSlots = new();
    private int lastSlotCount = -1;

    private bool hasStartedMoving = false;
    private Vector3 initialScale;

    private Collider2D currentSlot = null;
    private float timeInSlot = 0f;
    private bool resultSent = false;
    private bool isLocked = false;
    private GameObject lockedSlot = null;
    private Rigidbody2D rb;
    private AudioSource audioSource;
    private float lastClackTime = -Mathf.Infinity;

    [Header("Result Display")]
    [Tooltip("Optional display for showing the winning number")] public WinningSlotDisplay slotDisplay;

    private Transform followAnchor = null;
    private Coroutine snapRoutine = null;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        initialScale = transform.localScale;
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isLocked || !hasStartedMoving) return;

        if (other.gameObject.name.StartsWith("Slot_") && !touchingSlots.Contains(other))
        {
            touchingSlots.Add(other);
        }
    }

    private IEnumerator SnapToSlot(Transform target)
    {
        // capture the current offset so we can account for the wheel still
        // moving during the snap animation
        Vector3 startOffset = transform.position - target.position;
        float elapsed = 0f;

        while (elapsed < snapDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / snapDuration);

            // continue to update the position relative to the moving target
            transform.position = target.position + Vector3.Lerp(startOffset, Vector3.zero, t);
            yield return null;
        }

        transform.position = target.position;
        followAnchor = target;
    }

    private void PlayRandomClip(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0 || audioSource == null)
            return;
        var clip = clips[Random.Range(0, clips.Length)];
        if (clip != null)
            audioSource.PlayOneShot(clip);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (isLocked || !hasStartedMoving) return;

        if (other.gameObject.name.StartsWith("Slot_"))
        {
            touchingSlots.Remove(other);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (Time.time - lastClackTime >= clackCooldown)
        {
            PlayRandomClip(collisionClacks);
            lastClackTime = Time.time;
        }
    }

    private void Update()
    {
        if (isLocked)
        {
            if (followAnchor != null)
                transform.position = followAnchor.position;
            return;
        }

        if (!hasStartedMoving && rb.linearVelocity.magnitude > movementStartThreshold)
        {
            hasStartedMoving = true;
        }

        float wheelSpeed = 0f;

        // If you implement GetCurrentSpeed() in wheelSpinner, use it here:
        // wheelSpeed = wheelSpinner?.GetCurrentSpeed() ?? 0f;

        if (wheelSpeed > spinLockThreshold || touchingSlots.Count != 1)
        {
            timeInSlot = 0f;
            currentSlot = null;
            return;
        }

        if (touchingSlots.Count == 1)
        {
            if (currentSlot == touchingSlots[0])
            {
                timeInSlot += Time.deltaTime;

                if (timeInSlot >= timeToConfirm)
                {
                    LockIntoSlot();
                }
            }
            else
            {
                currentSlot = touchingSlots[0];
                timeInSlot = 0f;
            }
        }
        else
        {
            timeInSlot = 0f;
        }
    }

    private void LockIntoSlot()
    {
        if (currentSlot == null || isLocked)
            return;

        isLocked = true;
        PlayRandomClip(slotDropSounds);
        resultSent = true;
        lockedSlot = currentSlot.gameObject;

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;

        touchingSlots.Clear();
        lastSlotCount = -1;

        // 🔍 Look for BallAnchor under the slot
        Transform anchor = currentSlot.transform.Find("BallAnchor");
        Transform target = anchor != null ? anchor : currentSlot.transform;

        transform.SetParent(null); // Stay in world space
        transform.rotation = Quaternion.identity;
        transform.localScale = initialScale;

        if (snapRoutine != null)
            StopCoroutine(snapRoutine);
        snapRoutine = StartCoroutine(SnapToSlot(target));

        if (anchor != null)
        {
            Debug.Log($"✅ Ball locked to anchor under: {currentSlot.name}");
        }
        else
        {
            Debug.LogWarning($"⚠️ BallAnchor not found under {currentSlot.name}, snapped to slot center.");
        }

        if (currentSlot.TryGetComponent(out RouletteSlot slot))
        {
            Debug.Log($"🎯 Final result number: {slot.number}");

            var evaluator = FindAnyObjectByType<BetEvaluator>();
            if (evaluator != null)
            {
                evaluator.GatherChipsFromScene();
                evaluator.EvaluateBets();
            }
            else
            {
                Debug.LogWarning("⚠️ No BetEvaluator found in the scene.");
            }

            if (slotDisplay != null)
            {
                slotDisplay.ShowResult(slot);
            }
        }
    }

    public void ResetBall()
    {
        currentSlot = null;
        lockedSlot = null;
        timeInSlot = 0f;
        resultSent = false;
        isLocked = false;
        hasStartedMoving = false;
        followAnchor = null;
        if (snapRoutine != null)
        {
            StopCoroutine(snapRoutine);
            snapRoutine = null;
        }
        touchingSlots.Clear();
        lastSlotCount = -1;

        transform.SetParent(null, true);
        transform.localScale = initialScale;

        if (slotDisplay != null)
            slotDisplay.ResetDisplay();
    }

    public GameObject GetWinningSlot()
    {
        return resultSent ? lockedSlot : null;
    }
}
