using UnityEngine;

public class BetZone : MonoBehaviour
{
    [Tooltip("The slot this bet zone is tied to (e.g., Slot_3)")]
    public GameObject linkedSlot;

    // Optionally allow grouping bets like "Red", "Even", etc. later
}
