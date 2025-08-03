using UnityEngine;

public class RandomChanceChanger : MonoBehaviour
{
    [Tooltip("Additive modifier applied to the base chance.")]
    public float chanceModifier = 0f;

    /// <summary>
    /// Returns the adjusted chance after applying the modifier.
    /// </summary>
    public float ModifyChance(float baseChance)
    {
        return Mathf.Clamp01(baseChance + chanceModifier);
    }
}
