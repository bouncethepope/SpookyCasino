using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Collider2D))]
public class BettingChipDragger : MonoBehaviour
{
    [Header("Drag Settings")]
    public float heldScaleMultiplier = 1.2f;

    /// Global flag used to lock chip movement when bets are closed.
    public static bool betsLocked = false;


    private bool isDragging = false;
    private Vector3 originalScale;
    private Vector3 offset;

    private Camera mainCamera;
    private Collider2D selfCollider;
    [Tooltip("Collider of the chip bag this chip can be returned to")]
    public Collider2D bagCollider;

    private void Awake()
    {
        mainCamera = Camera.main;
        originalScale = transform.localScale;
        selfCollider = GetComponent<Collider2D>();
    }

    public void BeginDrag()
    {
        if (betsLocked)
            return;
        isDragging = true;
        offset = transform.position - GetMouseWorldPosition();
        transform.localScale = originalScale * heldScaleMultiplier;
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
            transform.position = GetMouseWorldPosition() + offset;
        }
    }

    public void EndDrag()
    {
        isDragging = false;
        transform.localScale = originalScale;

        // Update zones this chip covers
        BetChip betChip = GetComponent<BetChip>();
        if (betChip == null)
        {
            betChip = gameObject.AddComponent<BetChip>();
        }
        betChip.UpdateBetZones();

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 0.1f);
        Debug.Log($"🔍 Chip dropped at {transform.position}, detecting {hits.Length} overlaps:");
        int count = 0;
        foreach (var hit in hits)
        {
            if (hit != selfCollider && hit.CompareTag("BetZone"))
            {
                count++;
                Debug.Log($"    [{count}] {hit.gameObject.name} (Tag: {hit.gameObject.tag}, Layer: {LayerMask.LayerToName(hit.gameObject.layer)})");
            }
        }

        // Check if the chip should return to the bag
        TryReturnToBag();
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

    void OnMouseDown()
    {
        if (betsLocked)
            return;

        Debug.Log($"🖱️ Clicked on chip: {gameObject.name}");
        BeginDrag();
    }

    void OnMouseDrag()
    {
        DragUpdate();
    }

    void OnMouseUp()
    {
        EndDrag();
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = 0f;
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(mousePos);
        worldPos.z = 0f;
        return worldPos;

    }
}