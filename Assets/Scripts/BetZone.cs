using UnityEngine;

public enum BetGroupType
{
    None,

    // Color
    Red,
    Black,

    // Parity
    Even,
    Odd,

    // Dozens
    First_1_12,
    Second_13_24,
    Third_25_36,

    // Halves
    Low_1_18,
    High_19_36,

    // Rows
    Top,
    Middle,
    Bottom,

    // You can add more grouping options here if needed
}

public class BetZone : MonoBehaviour
{
    [Header("Direct Slot Betting")]
    [Tooltip("The specific slot this bet zone is tied to (e.g., Slot_3). Leave null for grouped bets.")]
    public GameObject linkedSlot;

    [Header("Grouped Betting")]
    [Tooltip("Grouped bet type, like Red, Black, Even, Odd, etc.")]
    public BetGroupType groupType = BetGroupType.None;

    [Header("Overlap Rules")]
    [Tooltip("If false, chips may not overlap this zone with other exclusive zones.")]
    public bool allowOverlap = true;
}
