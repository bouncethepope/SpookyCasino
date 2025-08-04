using UnityEngine;

[RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
public class BettingChipDragger : MonoBehaviour
{
    [Header("Drag Settings")]
    public float heldScaleMultiplier = 1.2f;

    /// Global flag used to lock chip movement when bets are closed.
    public static bool betsLocked = false;

    [Header("Physics Settings")]
    [Tooltip("Multiplier applied to the chip's release velocity")] public float releaseVelocityMultiplier = 1f;
    [Tooltip("Linear damping applied to the chip's Rigidbody2D after release")] public float chipDrag = 5f;
    [Tooltip("When enabled chips will collide with one another")]
    public bool enableChipCollisions = false;

    private bool isDragging = false;
    private Vector3 originalScale;
    private Vector3 offset;

    private Vector3 lastPosition;
    private Vector2 currentVelocity;

    private Camera mainCamera;
    private Collider2D selfCollider;
    private Rigidbody2D rb;

    [Tooltip("Collider of the chip bag this chip can be returned to")]
    public Collider2D bagCollider;

    private void Awake()
    {
        mainCamera = Camera.main;
        originalScale = transform.localScale;
        selfCollider = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();

        // Configure Rigidbody for top-down sliding behaviour.
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.linearDamping = chipDrag;
        rb.bodyType = RigidbodyType2D.Kinematic;

        UpdateCollisionSettings();
    }

    public bool IsDragging => isDragging;

    public void BeginDrag()
    {
        if (betsLocked)
            return;
        isDragging = true;
        offset = transform.position - GetMouseWorldPosition();
        transform.localScale = originalScale * heldScaleMultiplier;

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;
        lastPosition = transform.position;
    }

    public void DragUpdate()
    {
        if (betsLocked && isDragging)
        {
            EndDrag();
            return;
        }

        if (isDragging)
        {
            Vector3 newPos = GetMouseWorldPosition() + offset;
            currentVelocity = (newPos - lastPosition) / Time.deltaTime;
            transform.position = newPos;
            lastPosition = newPos;
        }
    }

    public void EndDrag()
    {
        if (!isDragging)
            return;

        isDragging = false;
        transform.localScale = originalScale;

        // Update zones this chip covers
        BetChip betChip = GetComponent<BetChip>();
        if (betChip == null)
        {
            betChip = gameObject.AddComponent<BetChip>();
        }
        betChip.UpdateBetZones();

        // If the chip was dropped back in the bag, don't apply momentum.
        if (TryReturnToBag())
            return;

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.linearVelocity = currentVelocity * releaseVelocityMultiplier;
    }

    public void ForceEndDrag()
    {
        if (isDragging)
            EndDrag();
    }

    private bool TryReturnToBag()
    {
        if (bagCollider == null)
            return false;

        if (selfCollider != null && selfCollider.bounds.Intersects(bagCollider.bounds))
        {
            if (TryGetComponent(out BetChip betChip))
            {
                PlayerCurrency.Instance?.AddCurrency(betChip.chipValue);
            }
            Destroy(gameObject);
            return true;
        }
        return false;
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = 0f;
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(mousePos);
        worldPos.z = 0f;
        return worldPos;
    }

    private void UpdateCollisionSettings()
    {
        // When interactions are disabled, ignore collisions between chips (same layer).
        Physics2D.IgnoreLayerCollision(gameObject.layer, gameObject.layer, !enableChipCollisions);

        // Always ignore physics collisions with bet zones so chips can be placed freely.
        IgnoreBetZoneCollisions();
    }

    private void OnValidate()
    {
        // Ensure collision settings update when toggled in inspector.
        UpdateCollisionSettings();

        if (rb == null)
            rb = GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.linearDamping = chipDrag;
    }

    private void IgnoreBetZoneCollisions()
    {
        if (selfCollider == null)
            selfCollider = GetComponent<Collider2D>();

        // Ignore collisions with all objects tagged as BetZone
        GameObject[] betZones = GameObject.FindGameObjectsWithTag("BetZone");
        foreach (var zone in betZones)
        {
            if (zone.TryGetComponent(out Collider2D zoneCollider))
            {
                Physics2D.IgnoreCollision(selfCollider, zoneCollider, true);
            }
        }

        // Also ignore by layer if a BetZone layer exists
        int betZoneLayer = LayerMask.NameToLayer("BetZone");
        if (betZoneLayer != -1)
        {
            Physics2D.IgnoreLayerCollision(gameObject.layer, betZoneLayer, true);
        }
    }
}
