using UnityEngine;

/// <summary>
/// Attach to a betting chip that should trigger a roll for the fish disruption event
/// when it comes into play.
/// </summary>
public class FishDisruptionChipTrigger : MonoBehaviour
{
    private FishDisruptionSpawnerController controller;

    private void Awake()
    {
        controller = FindFirstObjectByType<FishDisruptionSpawnerController>();
    }

    private void OnEnable()
    {
        if (controller == null)
        {
            controller = FindFirstObjectByType<FishDisruptionSpawnerController>();
        }
        controller?.RegisterChip();
    }

    private void OnDisable()
    {
        controller?.UnregisterChip();
    }
}