using UnityEngine;

public class FishDisruptionSpawnerController : MonoBehaviour
{
    [Tooltip("Base probability that the fish disruption event triggers.")]
    [Range(0f, 1f)]
    public float baseChance = 0.1f;

    [Tooltip("Component that increases the chance for each active chip.")]
    public FishDisruptionChanceIncrementer chanceIncrementer;

    [Tooltip("Component responsible for spawning the fish disruption event.")]
    public FishDisruptionSpawner fishDisruptionSpawner;

    /// <summary>
    /// Registers a chip entering play and rolls for the disruption event.
    /// </summary>
    public void RegisterChip()
    {
        if (chanceIncrementer != null)
        {
            chanceIncrementer.RegisterChip();
        }
        RollForFishDisruption();
    }

    /// <summary>
    /// Unregisters a chip leaving play.
    /// </summary>
    public void UnregisterChip()
    {
        if (chanceIncrementer != null)
        {
            chanceIncrementer.UnregisterChip();
        }
    }

    /// <summary>
    /// Attempts to trigger the fish disruption event using a random roll.
    /// </summary>
    private void RollForFishDisruption()
    {
        Debug.Log($"Pre check chance: {baseChance}");
        float chance = baseChance;
        if (chanceIncrementer != null)
        {
            chance = chanceIncrementer.ModifyChance(chance);
            Debug.Log($"POST check chance: {chance}");
        }

        if (Random.value <= chance && fishDisruptionSpawner != null)
        {
            fishDisruptionSpawner.SpawnFishDisruptions();
        }

    }
}
