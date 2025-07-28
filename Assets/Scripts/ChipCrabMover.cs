using UnityEngine;
using DG.Tweening;

/// <summary>
/// Animates a chip as if a crab underneath it moves the chip to a new spot.
/// Can only trigger before the ball locks into its final slot.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class ChipCrabMover : MonoBehaviour
{
    [Header("Crab Movement")]
    [Tooltip("Sprite to use when the crab is moving the chip")] public Sprite crabSprite;
    [Tooltip("Offset applied when moving the chip")] public Vector2 moveOffset = new Vector2(0.5f, 0f);
    [Tooltip("Duration of the move animation")] public float moveDuration = 1f;

    [Header("Debug")]
    [Tooltip("Tick to trigger the crab move once")] public bool triggerMove = false;

    private SpriteRenderer spriteRenderer;
    private Sprite originalSprite;
    private bool moved = false;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalSprite = spriteRenderer.sprite;
    }

    private void Update()
    {
        if (triggerMove)
        {
            triggerMove = false;
            MoveWithCrab();
        }
    }

    /// <summary>
    /// Triggers the crab animation if the ball hasn't locked into a slot.
    /// </summary>
    [ContextMenu("Move With Crab")]
    public void MoveWithCrab()
    {
        if (moved || RouletteBall.BallLocked)
            return;

        moved = true;

        if (crabSprite != null)
            spriteRenderer.sprite = crabSprite;

        Vector3 target = transform.position + (Vector3)moveOffset;
        transform.DOMove(target, moveDuration).OnComplete(() =>
        {
            if (originalSprite != null)
                spriteRenderer.sprite = originalSprite;
            if (TryGetComponent(out BetChip chip))
                chip.UpdateBetZones();
        });
    }
}