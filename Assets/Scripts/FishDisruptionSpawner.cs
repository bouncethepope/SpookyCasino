using UnityEngine;

/// <summary>
/// Spawns fish disruption events that swim across the table and bump into betting chips.
/// Configure the ranges to control start/end points, speed, and wave motion.
/// Right-clicking the spawner will trigger a manual spawn at runtime.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class FishDisruptionSpawner : MonoBehaviour
{
    [Header("Fish Disruption Settings")]
    [Tooltip("Prefab for the fish disruption event to spawn")] public GameObject fishPrefab;
    [Tooltip("Minimum number of fish disruptions to spawn")] public int minFish = 1;
    [Tooltip("Maximum number of fish disruptions to spawn")] public int maxFish = 3;

    [Header("Start Area")]
    [Tooltip("Corner transform representing the minimum (bottom-left) of the start area")] public Transform startAreaMin;
    [Tooltip("Corner transform representing the maximum (top-right) of the start area")] public Transform startAreaMax;

    [Header("End Area")]
    [Tooltip("Corner transform representing the minimum (bottom-left) of the end area")] public Transform endAreaMin;
    [Tooltip("Corner transform representing the maximum (top-right) of the end area")] public Transform endAreaMax;

    [Header("Movement Ranges")]
    [Tooltip("Minimum swim speed")] public float minSpeed = 1f;
    [Tooltip("Maximum swim speed")] public float maxSpeed = 3f;
    [Tooltip("Minimum wave amplitude")] public float minWaveAmplitude = 0.2f;
    [Tooltip("Maximum wave amplitude")] public float maxWaveAmplitude = 0.5f;
    [Tooltip("Minimum wave frequency")] public float minWaveFrequency = 1f;
    [Tooltip("Maximum wave frequency")] public float maxWaveFrequency = 3f;

    [Header("Collision Exclusions")]
    [Tooltip("Colliders the spawned fish should ignore (e.g., bet zones)")] public Collider2D[] ignoreColliders;

    [Tooltip("Spawn fish disruptions automatically on start")] public bool spawnOnStart = true;

    private void Start()
    {
        if (spawnOnStart)
            SpawnFishDisruptions();
    }

    [ContextMenu("Spawn Fish Disruptions")]
    public void SpawnFishDisruptions()
    {
        if (fishPrefab == null)
        {
            Debug.LogWarning("FishDisruptionSpawner has no fish prefab assigned.");
            return;
        }

        if (startAreaMin == null || startAreaMax == null || endAreaMin == null || endAreaMax == null)
        {
            Debug.LogWarning("FishDisruptionSpawner is missing start or end area transforms.");
            return;
        }

        Vector2 startMin = startAreaMin.position;
        Vector2 startMax = startAreaMax.position;
        Vector2 endMin = endAreaMin.position;
        Vector2 endMax = endAreaMax.position;

        int count = Random.Range(minFish, maxFish + 1);
        for (int i = 0; i < count; i++)
        {
            Vector2 start = new Vector2(Random.Range(Mathf.Min(startMin.x, startMax.x), Mathf.Max(startMin.x, startMax.x)),
                                        Random.Range(Mathf.Min(startMin.y, startMax.y), Mathf.Max(startMin.y, startMax.y)));
            Vector2 end = new Vector2(Random.Range(Mathf.Min(endMin.x, endMax.x), Mathf.Max(endMin.x, endMax.x)),
                                      Random.Range(Mathf.Min(endMin.y, endMax.y), Mathf.Max(endMin.y, endMax.y)));
            float speed = Random.Range(minSpeed, maxSpeed);
            float amp = Random.Range(minWaveAmplitude, maxWaveAmplitude);
            float freq = Random.Range(minWaveFrequency, maxWaveFrequency);

            GameObject fishObj = Instantiate(fishPrefab, start, Quaternion.identity);
            FishDisruptionEvent disruption = fishObj.GetComponent<FishDisruptionEvent>();
            if (disruption == null)
                disruption = fishObj.AddComponent<FishDisruptionEvent>();

            disruption.Initialize(start, end, speed, amp, freq, ignoreColliders);
        }
    }

    private void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(1))
            SpawnFishDisruptions();
    }
}