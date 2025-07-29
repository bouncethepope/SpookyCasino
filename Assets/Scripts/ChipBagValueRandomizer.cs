using UnityEngine;

[RequireComponent(typeof(ChipBag))]
public class ChipBagValueRandomizer : MonoBehaviour
{
    [Header("Chip Bag Value Range")]
    public int minValue = 1;
    public int maxValue = 10;

    private ChipBag bag;

    private void Awake()
    {
        bag = GetComponent<ChipBag>();
    }

    private void Start()
    {
        RandomizeValue();
    }

    [ContextMenu("Randomize Value")]
    public void RandomizeValue()
    {
        if (bag == null)
            return;
        bag.chipValue = Random.Range(minValue, maxValue + 1);
    }
}