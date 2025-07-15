using UnityEngine;
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

    private Transform followAnchor = null;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        initialScale = transform.localScale;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isLocked || !hasStartedMoving) return;

        if (other.gameObject.name.StartsWith("Slot_") && !touchingSlots.Contains(other))
        {
            touchingSlots.Add(other);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (isLocked || !hasStartedMoving) return;

        if (other.gameObject.name.StartsWith("Slot_"))
        {
            touchingSlots.Remove(other);
        }
    }

    private void Update()
    {
        if (isLocked && followAnchor != null)
        {
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
        resultSent = true;
        lockedSlot = currentSlot.gameObject;

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;

        touchingSlots.Clear();
        lastSlotCount = -1;

        // 🔍 Look for BallAnchor under the slot
        Transform anchor = currentSlot.transform.Find("BallAnchor");

        if (anchor != null)
        {
            followAnchor = anchor;
            transform.SetParent(null); // Stay in world space
            transform.position = anchor.position;
            transform.rotation = Quaternion.identity;
            transform.localScale = initialScale;

            Debug.Log($"✅ Ball locked to anchor under: {currentSlot.name}");
        }
        else
        {
            followAnchor = null;
            transform.SetParent(null);
            transform.position = currentSlot.transform.position;
            transform.rotation = Quaternion.identity;
            transform.localScale = initialScale;

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
        touchingSlots.Clear();
        lastSlotCount = -1;

        transform.SetParent(null, true);
        transform.localScale = initialScale;
    }

    public GameObject GetWinningSlot()
    {
        return resultSent ? lockedSlot : null;
    }
}
