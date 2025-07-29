using UnityEngine;

using DG.Tweening;

public class ChipBagDispenser : MonoBehaviour
{
    [Header("Bag Setup")]
    [Tooltip("Possible prefabs used when spawning chip bags")] public ChipBagValueRandomizer[] bagPrefabs;

    [Tooltip("Where bags start off screen before moving in")] public Transform startPoint;

    [Tooltip("All available bag slots")] public Transform[] bagSlots;

    [Tooltip("Slots filled at the beginning of the game")] public Transform[] startingSlots;

    [Tooltip("Seconds bags take to move from the start point")] public float moveDuration = 1f;

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
            int index = FindSlotIndex(slot);
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

    private int FindSlotIndex(Transform slot)
    {
        if (bagSlots == null || slot == null)
            return -1;
        for (int i = 0; i < bagSlots.Length; i++)
        {
            if (bagSlots[i] == slot)
                return i;
        }
        return -1;
    }

    private void SpawnBagAtSlot(Transform slot, int slotIndex)
    {
        if (bagPrefabs == null || slotIndex < 0 || slotIndex >= bagPrefabs.Length)
            return;

        ChipBagValueRandomizer prefab = bagPrefabs[slotIndex];

        ChipBagValueRandomizer bag = Instantiate(prefab, startPoint.position, startPoint.rotation);
        spawnedBags[slotIndex] = bag.gameObject;
        bag.RandomizeValue();
        bag.transform.DOMove(slot.position, moveDuration);
    }
}