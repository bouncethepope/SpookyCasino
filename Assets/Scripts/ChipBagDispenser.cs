using UnityEngine;
using DG.Tweening;

public class ChipBagDispenser : MonoBehaviour
{
    [Header("Bag Setup")]
    [Tooltip("Possible prefabs used when spawning chip bags")]
    public ChipBagValueRandomizer[] bagPrefabs;

    [Tooltip("Specific bag prefab that triggers the pufferfish disruption when spawned")]
    public ChipBagValueRandomizer pufferfishBagPrefab;

    [Tooltip("Spawner used to trigger the pufferfish disruption")]
    public PufferfishSpawner pufferfishSpawner;

    [Tooltip("Where bags start off screen before moving in")]
    public Transform startPoint;

    [Tooltip("All available bag slots")]
    public Transform[] bagSlots;

    [Tooltip("Slots filled at the beginning of the game")]
    public Transform[] startingSlots;

    [Tooltip("Seconds bags take to move from the start point")]
    public float moveDuration = 1f;

    [Header("Disruption Events")]
    [Tooltip("Prefab that spawns bubbles for the disruption event")] public BubbleSpawner bubbleSpawnerPrefab;


    private GameObject[] spawnedBags;

    private void Awake()
    {
        if (bagSlots != null)
            spawnedBags = new GameObject[bagSlots.Length];
    }

    public void DispenseStartingBags()
    {
        if (bagPrefabs == null || bagPrefabs.Length == 0 || startPoint == null || startingSlots == null)
            return;

        foreach (Transform slot in startingSlots)
        {
            int index = System.Array.IndexOf(bagSlots, slot);
            if (index >= 0)
                SpawnBagAtSlot(slot, index);
        }
    }

    public void DispenseNextBag()
    {
        if (bagPrefabs == null || bagPrefabs.Length == 0 || bagSlots == null || startPoint == null)
            return;

        for (int i = 0; i < bagSlots.Length; i++)
        {
            if (bagSlots[i] != null && spawnedBags[i] == null)
            {
                SpawnBagAtSlot(bagSlots[i], i);
                break;
            }
        }
    }

    private void SpawnBagAtSlot(Transform slot, int slotIndex)
    {
        if (bagPrefabs == null || slotIndex < 0 || slotIndex >= bagPrefabs.Length)
            return;

        ChipBagValueRandomizer prefab = bagPrefabs[slotIndex];

        ChipBagValueRandomizer bag = Instantiate(prefab, startPoint.position, startPoint.rotation);
        spawnedBags[slotIndex] = bag.gameObject;

        if (prefab == pufferfishBagPrefab && pufferfishSpawner != null)
        {
            pufferfishSpawner.Spawn();
        }


        if (bag.triggerBubbleDisruption && bubbleSpawnerPrefab != null)
        {
            BubbleSpawner spawner = Instantiate(bubbleSpawnerPrefab);
            spawner.Activate();
        }

        bag.RandomizeValue();
        bag.transform.DOMove(slot.position, moveDuration);
    }
}
