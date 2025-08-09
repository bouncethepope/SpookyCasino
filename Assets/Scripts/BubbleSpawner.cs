using UnityEngine;

/// <summary>
/// Spawns floating bubbles that can carry chips upward.
/// Controls quantity and size range of spawned bubbles.
/// </summary>
public class BubbleSpawner : MonoBehaviour
{
    [Header("Bubble Prefab")]
    [Tooltip("Prefab of the bubble with a BubbleChipCarrier component")] public GameObject bubblePrefab;

    [Header("Spawn Settings")]
    [Tooltip("Seconds between bubble spawns")] public float spawnInterval = 2f;
    [Tooltip("Maximum number of bubbles at once")] public int maxBubbles = 5;
    [Tooltip("Range of bubble size in world units")] public Vector2 sizeRange = new Vector2(0.5f, 1.5f);
    [Tooltip("Range of bubble upward speed")] public Vector2 speedRange = new Vector2(0.5f, 2f);

    [Header("Activation")]
    [Tooltip("Whether bubbles should currently spawn")]
    public bool spawnActive = false;

    private float timer = 0f;
    private int activeBubbles = 0;
    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
            TryPopBubbleUnderCursor();

        if (!spawnActive || bubblePrefab == null || activeBubbles >= maxBubbles)
            return;

        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            SpawnBubble();
            timer = 0f;
        }
    }

    private void SpawnBubble()
    {
        float xMin = mainCamera.ViewportToWorldPoint(new Vector3(0f, 0f, 0f)).x;
        float xMax = mainCamera.ViewportToWorldPoint(new Vector3(1f, 0f, 0f)).x;
        float y = mainCamera.ViewportToWorldPoint(new Vector3(0.5f, 0f, 0f)).y - 1f; // start slightly below screen

        Vector3 pos = new Vector3(Random.Range(xMin, xMax), y, 0f);
        GameObject bubbleObj = Instantiate(bubblePrefab, pos, Quaternion.identity);

        float size = Random.Range(sizeRange.x, sizeRange.y);
        bubbleObj.transform.localScale = Vector3.one * size;

        if (bubbleObj.TryGetComponent<BubbleChipCarrier>(out var carrier))
        {
            carrier.riseSpeed = Random.Range(speedRange.x, speedRange.y);
            carrier.onDestroyed += () => { activeBubbles--; };
        }

        activeBubbles++;
    }

    private void TryPopBubbleUnderCursor()
    {
        if (mainCamera == null)
            return;

        var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        var hits = Physics2D.GetRayIntersectionAll(ray);
        foreach (var hit in hits)
        {
            if (hit.collider != null && hit.collider.TryGetComponent<BubbleChipCarrier>(out var bubble))
            {
                bubble.Pop();
                break;
            }
        }
    }

    /// <summary>
    /// Enables bubble spawning.
    /// </summary>
    public void Activate()
    {
        spawnActive = true;
    }
}