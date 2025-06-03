using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Collider2D))]
public class BettingChipDragger : MonoBehaviour
{
    [Header("Drag Settings")]
    public float heldScaleMultiplier = 1.2f;

    private bool isDragging = false;
    private Vector3 originalScale;
    private Vector3 offset;

    private Camera mainCamera;
    private Collider2D selfCollider;

    private void Awake()
    {
        mainCamera = Camera.main;
        originalScale = transform.localScale;
        selfCollider = GetComponent<Collider2D>();
    }

    void OnMouseDown()
    {
        Debug.Log($"🖱️ Clicked on chip: {gameObject.name}");
        isDragging = true;
        offset = transform.position - GetMouseWorldPosition();
        transform.localScale = originalScale * heldScaleMultiplier;
    }

    void OnMouseDrag()
    {
        if (isDragging)
        {
            transform.position = GetMouseWorldPosition() + offset;
        }
    }

    void OnMouseUp()
    {
        isDragging = false;
        transform.localScale = originalScale;

        // Check for overlapping colliders
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
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = 0f;
        return mainCamera.ScreenToWorldPoint(mousePos).WithZ(0f);
    }
}

// Extension to set Z to 0
public static class Vector3Extensions
{
    public static Vector3 WithZ(this Vector3 v, float z)
    {
        return new Vector3(v.x, v.y, z);
    }
}
