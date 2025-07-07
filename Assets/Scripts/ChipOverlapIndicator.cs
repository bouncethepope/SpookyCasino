using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class ChipOverlapIndicator : MonoBehaviour
{
    [Tooltip("Material when placement is valid")] public Material normalMaterial;
    [Tooltip("Material when overlapping conflicting zones")] public Material conflictMaterial;
    [Tooltip("Radius used to check overlaps")] public float overlapRadius = 0.1f;

    private Collider2D selfCollider;
    private SpriteRenderer spriteRenderer;
    private Material defaultMaterial;

    private void Awake()
    {
        selfCollider = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        defaultMaterial = spriteRenderer.material;
        if (normalMaterial == null) normalMaterial = defaultMaterial;
    }

    private void Update()
    {
        bool hasConflict = DetectConflict();
        spriteRenderer.material = hasConflict && conflictMaterial != null ? conflictMaterial : normalMaterial;
    }

    private bool DetectConflict()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, overlapRadius);
        List<BetZone> zones = new List<BetZone>();
        foreach (var hit in hits)
        {
            if (hit == selfCollider) continue;
            if (!hit.CompareTag("BetZone")) continue;
            if (hit.TryGetComponent(out BetZone zone)) zones.Add(zone);
        }

        if (zones.Count <= 1) return false;
        foreach (var z in zones)
        {
            if (z != null && !z.allowOverlap) return true;
        }
        return false;
    }
}
