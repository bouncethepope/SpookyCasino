using UnityEngine;

public class TrackedObject : MonoBehaviour
{
    [Tooltip("Logical name for this type, e.g., 'Crab', 'Chip', 'Enemy'")]
    public string key = "FinalBetChip";

    void OnEnable() { if (TrackedObjectCounter.Instance) TrackedObjectCounter.Instance.Adjust(key, +1); }
    void OnDisable() { if (TrackedObjectCounter.Instance) TrackedObjectCounter.Instance.Adjust(key, -1); }
}
