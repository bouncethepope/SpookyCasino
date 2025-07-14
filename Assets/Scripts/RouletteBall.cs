using UnityEngine;

public class RouletteBall : MonoBehaviour
{
    [Header("Detection Settings")]
    [Tooltip("Seconds the ball must remain in a slot before locking in.")]
    public float timeToConfirm = 3f;
    public WheelSpinner wheelSpinner; // Optional reference to spinner
    [Tooltip("Wheel spin speed below which final slot will be checked (deg/sec)")]
    public float spinLockThreshold = 30f;
    [Tooltip("Velocity magnitude that counts as the ball starting to move")] 
    public float movementStartThreshold = 0.1f;

    private readonly System.Collections.Generic.List<Collider2D> touchingSlots = new();
    private int lastSlotCount = -1;

    private bool hasStartedMoving = false;
    private Vector3 initialScale;

    private Collider2D currentSlot = null;
    private float timeInSlot = 0f;
    private bool resultSent = false;
    private bool isLocked = false;
    private GameObject lockedSlot = null;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        initialScale = transform.localScale;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isLocked || !hasStartedMoving) return;

        if (other.gameObject.name.StartsWith("Slot_"))
        {
            if (!touchingSlots.Contains(other))
            {
                touchingSlots.Add(other);
            }
        }
    }

    // OnTriggerStay left intentionally empty; locking handled in Update based on
    // wheel speed and slot contact count.

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
        if (isLocked) return;

        if (!hasStartedMoving)
        {
            if (rb.velocity.magnitude >= movementStartThreshold)
            {
                hasStartedMoving = true;
            }
            else
            {
                return;
            }
        }

        if (wheelSpinner != null && Mathf.Abs(wheelSpinner.GetCurrentSpinSpeed()) > spinLockThreshold)
        {
            timeInSlot = 0f;
            return;
        }

        int count = touchingSlots.Count;
        if (count != lastSlotCount && count != 1)
        {
            if (count == 0)
                Debug.Log("⚠️ Wheel slow but no slot detected. Waiting...");
            else
                Debug.Log("⚠️ Wheel slow but multiple slots detected. Waiting...");
        }
        lastSlotCount = count;

        if (count == 1)
        {
            currentSlot = touchingSlots[0];
            timeInSlot += Time.deltaTime;
            if (timeInSlot >= timeToConfirm)
            {
                LockIntoSlot();
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

        transform.SetParent(currentSlot.transform, true);
        transform.localPosition = Vector3.zero;
        transform.localScale = initialScale;

        Debug.Log($"✅ Ball locked in slot: {currentSlot.gameObject.name}");

        if (currentSlot.TryGetComponent(out RouletteSlot slot))
        {
            Debug.Log($"Final result number: {slot.number}");

            // 🔽 Simple call to BetEvaluator
            var evaluator = FindAnyObjectByType<BetEvaluator>();
            if (evaluator != null)
            {
                evaluator.GatherChipsFromScene(); // Optional: ensures fresh chip list
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
