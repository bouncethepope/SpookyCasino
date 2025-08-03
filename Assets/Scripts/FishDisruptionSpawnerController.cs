using UnityEngine;

public class FishDisruptionSpawnerController : MonoBehaviour
{
    [Tooltip("Base probability that the fish disruption event triggers.")]
    [Range(0f, 1f)]
    public float baseChance = 0.1f;

    [Tooltip("Optional modifier that can adjust the chance in real time.")]
    public RandomChanceChanger randomChanceChanger;

    [Tooltip("Component responsible for spawning the fish disruption event.")]
    public MonoBehaviour fishDisruptionSpawner;

    /// <summary>
    /// Attempts to trigger the fish disruption event using a random roll.
    /// </summary>
    public void RollForFishDisruption()
    {
        float chance = baseChance;
        if (randomChanceChanger != null)
        {
            chance = randomChanceChanger.ModifyChance(baseChance);
        }

        if (Random.value <= chance && fishDisruptionSpawner != null)
        {
            // Call a Spawn method on the spawner if it exists.
            fishDisruptionSpawner.SendMessage("Spawn", SendMessageOptions.DontRequireReceiver);
        }
    }
}
