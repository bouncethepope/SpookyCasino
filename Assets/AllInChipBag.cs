using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class AllInChipBag : MonoBehaviour
{
    [Header("Chip Settings")]
    public GameObject chipPrefab;
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
        if (ChipBag.betsLocked)
            return;

        if (currentChip != null) return;

        int amount = PlayerCurrency.Instance != null ? PlayerCurrency.Instance.CurrentCurrency : 0;
        if (amount <= 0)
        {
            Debug.Log("No currency available for an all-in bet.");
            return;
        }

        if (PlayerCurrency.Instance != null && !PlayerCurrency.Instance.TrySpend(amount))
        {
            Debug.Log("Failed to spend currency for all-in chip.");
            return;
        }

        currentChip = Instantiate(chipPrefab, transform.position, Quaternion.identity, chipParent);

        if (currentChip.TryGetComponent(out BetChip betChip))
        {
            betChip.chipValue = amount;
        }
        else
        {
            betChip = currentChip.AddComponent<BetChip>();
            betChip.chipValue = amount;
        }

        currentDragger = currentChip.GetComponent<BettingChipDragger>();
        if (currentDragger == null)
            currentDragger = currentChip.AddComponent<BettingChipDragger>();

        currentDragger.bagCollider = bagCollider;
        currentDragger.BeginDrag();
    }

    private void OnMouseDrag()
    {
        if (ChipBag.betsLocked) return;
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

            currentDragger = null;
            currentChip = null;
        }
    }
}