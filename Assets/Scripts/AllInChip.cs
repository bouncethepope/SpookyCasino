using UnityEngine;

/// <summary>
/// Tag component used to identify the all-in betting chip.
/// Tracks whether an all-in chip is currently present in the scene.
/// </summary>
public class AllInChip : MonoBehaviour
{
    /// <summary>
    /// True when an all-in chip exists in the scene.
    /// </summary>
    public static bool IsInScene { get; private set; }

    private void OnEnable()
    {
        IsInScene = true;
    }

    private void OnDisable()
    {
        IsInScene = false;
    }
}