using UnityEngine;

/// <summary>
/// Attach to a betting chip that should trigger a roll for the fish disruption event
/// when it comes into play.
/// </summary>
public class FishDisruptionChipTrigger : MonoBehaviour
{
    [Tooltip("Controller that manages the fish disruption chance.")]
    public FishDisruptionSpawnerController controller;

    void OnEnable()
    {
        if (controller != null)
        {
            controller.RollForFishDisruption();
        }
    }
}
