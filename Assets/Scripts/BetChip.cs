using System.Collections.Generic;
using UnityEngine;

public class BetChip : MonoBehaviour
{
    [Header("Chip Value")]
    [Tooltip("Monetary value of this chip")]
    public int chipValue = 1;
    [HideInInspector]
    public List<BetZone> betZones = new List<BetZone>();

    public void UpdateBetZones()
    {
        betZones.Clear();
        Collider2D col = GetComponent<Collider2D>();
        if (col == null) return;

        float radius = Mathf.Max(col.bounds.extents.x, col.bounds.extents.y);
        Collider2D[] hits = Physics2D.OverlapCircleAll(col.bounds.center, radius);
        foreach (var hit in hits)
        {
            //Debug.Log(hit);  - Print line for testing chip table collisions
            if (!hit.CompareTag("BetZone")) continue;
            if (hit.TryGetComponent(out BetZone zone))
            {
                betZones.Add(zone);
            }
        }
    }
}
