using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ChipBag : MonoBehaviour
{
    [Header("Chip Settings")]
    public GameObject chipPrefab;
    public int chipValue = 1;
    public Transform chipParent;

    /// <summary>
    /// Global flag to lock chip bag interaction when bets are closed.
    /// </summary>
    public static bool betsLocked = false;

    private BettingChipDragger currentDragger;
    private GameObject currentChip;
    private Collider2D bagCollider;

    private void Awake()
    {
        bagCollider = GetComponent<Collider2D>();
    }

    private void OnMouseDown()
    {
        if (betsLocked)
            return;

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
        if (betsLocked) return;
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
