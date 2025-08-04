using UnityEngine;

/// <summary>
/// Tracks active chips and adds a fixed increment to the base chance per chip.
/// </summary>
public class FishDisruptionChanceIncrementer : MonoBehaviour
{
    [Tooltip("Chance added for each active chip.")]
    public float chancePerChip = 0.1f;

    private int activeChipCount = 0;

    /// <summary>
    /// Register an active chip in play.
    /// </summary>
    public void RegisterChip()
    {
        activeChipCount++;
    }

    /// <summary>
    /// Unregister a chip that has left play.
    /// </summary>
    public void UnregisterChip()
    {
        activeChipCount = Mathf.Max(0, activeChipCount - 1);
    }

    /// <summary>
    /// Returns the modified chance based on the current number of chips.
    /// </summary>
    public float ModifyChance(float baseChance)
    {
        return Mathf.Clamp01(baseChance + activeChipCount * chancePerChip);
    }
}