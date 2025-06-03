using UnityEngine;

public enum RouletteColor { None, Red, Black }
public enum RouletteParity { None, Odd, Even }
public enum RouletteHalf { None, Low_1_18, High_19_36 }
public enum RouletteDozen { None, First_1_12, Second_13_24, Third_25_36 }
public enum RouletteLine { None, Top, Middle, Bottom }

public class RouletteSlot : MonoBehaviour
{
    [Header("Slot Info")]
    [Tooltip("The number this slot represents (0-36)")]
    public int number;

    [Header("Colour")]
    public RouletteColor color = RouletteColor.None;

    [Header("Parity")]
    public RouletteParity parity = RouletteParity.None;

    [Header("Halves")]
    public RouletteHalf half = RouletteHalf.None;

    [Header("Dozens")]
    public RouletteDozen dozen = RouletteDozen.None;

    [Header("Line")]
    public RouletteLine line = RouletteLine.None;

    public bool IsInGroup(BetGroupType group)
    {
        // Existing logic (if still needed)
        return false;
    }
}
