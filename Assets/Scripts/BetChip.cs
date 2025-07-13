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

        Collider2D[] hits = Physics2D.OverlapCircleAll(col.bounds.center, 0.1f);
        foreach (var hit in hits)
        {
            if (!hit.CompareTag("BetZone")) continue;
            if (hit.TryGetComponent(out BetZone zone))
            {
                betZones.Add(zone);
            }
        }
    }
}
