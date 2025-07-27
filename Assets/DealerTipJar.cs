using UnityEngine;

/// <summary>
/// Collects betting chips deposited as tips for the dealer.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class DealerTipJar : MonoBehaviour
{
    [Tooltip("Total value of chips currently in the jar.")]
    public int tipTotal = 0;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out BetChip chip))
        {
            tipTotal += chip.chipValue;
            Debug.Log($"💰 Dealer tip jar updated: {tipTotal}");
            Destroy(other.gameObject);
        }
    }
}