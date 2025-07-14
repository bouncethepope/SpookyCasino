using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ChipBag : MonoBehaviour
{
    [Header("Chip Settings")]
    public GameObject chipPrefab;
    public int chipValue = 1;
    public Transform chipParent;

    private BettingChipDragger currentDragger;
    private GameObject currentChip;
    private Collider2D bagCollider;

    private void Awake()
    {
        bagCollider = GetComponent<Collider2D>();
    }

    private void OnMouseDown()
    {
        if (currentChip != null) return;

        if (PlayerCurrency.Instance != null && !PlayerCurrency.Instance.TrySpend(chipValue))
        {
            Debug.Log("Not enough currency to take a chip.");
            return;
        }

        currentChip = Instantiate(chipPrefab, transform.position, Quaternion.identity, chipParent);

        if (currentChip.TryGetComponent(out BetChip betChip))
        {
            betChip.chipValue = chipValue;
        }
        else
        {
            betChip = currentChip.AddComponent<BetChip>();
            betChip.chipValue = chipValue;
        }

        currentDragger = currentChip.GetComponent<BettingChipDragger>();
        if (currentDragger == null)
            currentDragger = currentChip.AddComponent<BettingChipDragger>();

        currentDragger.bagCollider = bagCollider;
        currentDragger.BeginDrag();
    }

    private void OnMouseDrag()
    {
        if (currentDragger != null)
        {
            currentDragger.DragUpdate();
        }
    }

    private void OnMouseUp()
    {
        if (currentDragger != null)
        {
            currentDragger.EndDrag();

<<<<<<< HEAD
=======
            bool returned = false;
            if (currentChip != null && bagCollider != null)
            {
                Collider2D chipCol = currentChip.GetComponent<Collider2D>();
                if (chipCol != null && chipCol.bounds.Intersects(bagCollider.bounds))
                {
                    PlayerCurrency.Instance?.AddCurrency(chipValue);
                    Destroy(currentChip);
                    returned = true;
                }
            }

            if (!returned)
            {
                // allow chip to remain in scene
            }

>>>>>>> 84072e044300087380675e99b853ced5af16d6a6
            currentDragger = null;
            currentChip = null;
        }
    }
}