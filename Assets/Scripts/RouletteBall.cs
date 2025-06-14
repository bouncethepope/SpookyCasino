using UnityEngine;

public class RouletteBall : MonoBehaviour
{
    [Header("Detection Settings")]
    [Tooltip("Seconds the ball must remain in a slot before locking in.")]
    public float timeToConfirm = 3f;
    public WheelSpinner wheelSpinner; // Optional reference to spinner

    private Collider2D currentSlot = null;
    private float timeInSlot = 0f;
    private bool resultSent = false;
    private bool isLocked = false;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isLocked) return;

        if (other.gameObject.name.StartsWith("Slot_"))
        {
            Debug.Log($"🎯 Entered slot collider: {other.gameObject.name}");
            currentSlot = other;
            timeInSlot = 0f;
            resultSent = false;
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (isLocked) return;

        if (other == currentSlot)
        {
            if (wheelSpinner != null && wheelSpinner.IsSpinning())
            {
                timeInSlot = 0f;
                return;
            }

            timeInSlot += Time.deltaTime;

            if (timeInSlot >= timeToConfirm)
            {
                LockIntoSlot();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (isLocked) return;

        if (other == currentSlot)
        {
            Debug.Log($"❌ Exited slot: {other.gameObject.name}");
            currentSlot = null;
            timeInSlot = 0f;
            resultSent = false;
        }
    }

    private void LockIntoSlot()
    {
        if (currentSlot == null || isLocked)
            return;

        isLocked = true;
        resultSent = true;

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;

        transform.SetParent(currentSlot.transform, true);
        transform.localPosition = Vector3.zero;

        Debug.Log($"✅ Ball locked in slot: {currentSlot.gameObject.name}");

        if (currentSlot.TryGetComponent(out RouletteSlot slot))
        {
            Debug.Log($"Final result number: {slot.number}");
        }
    }

    public GameObject GetWinningSlot()
    {
        return resultSent ? currentSlot?.gameObject : null;
    }
}
