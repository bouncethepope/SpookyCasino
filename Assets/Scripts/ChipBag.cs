using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Collider2D))]
public class ChipBag : MonoBehaviour
{
    [Header("Chip Settings")]
    public GameObject chipPrefab;
    public int chipValue = 1;
    public Transform chipParent;

    /// Global flag to lock chip bag interaction when bets are closed.
    public static bool betsLocked = false;

    private BettingChipDragger currentDragger;
    private GameObject currentChip;
    private Collider2D bagCollider;

    [Header("Insufficient Funds Feedback")]
    [Tooltip("Show an outline when the player can't afford a chip.")]
    public bool showOutlineOnInsufficientFunds = true;
    [Tooltip("Outline GameObject to toggle when player can't afford a chip.")]
    public GameObject insufficientFundsOutline;

    // Track multiple chips when dragging with right click
    private readonly List<BettingChipDragger> multiDraggers = new();
    private bool isMultiDrag = false;

    [Header("Chip Scale Range")]
    public bool useChipScaleRange = false;
    public float minChipScale = 1f;
    public float maxChipScale = 1f;

    [Header("Right Click Settings")]
    public bool enableRightClickDrag = true; // Toggle for right-click drag
    [Tooltip("Force applied to chips when dropping multiple chips with right click.")]
    public float rightClickDropForce = 2f;
    [Tooltip("Radius of the fan spread when dragging multiple chips.")]
    public float rightClickFanRadius = 0.2f;
    [Tooltip("Angle step in degrees between chips in the fan spread.")]
    public float rightClickFanAngleStep = 10f;

    [Header("Final Chip Bag Mode")]
    [Tooltip("If true, this bag cannot give out chips.")]
    public bool isFinalChipBag = false;

    private void Awake()
    {
        bagCollider = GetComponent<Collider2D>();
        if (insufficientFundsOutline != null)
            insufficientFundsOutline.SetActive(false);
    }

    private void OnMouseEnter()
    {
        UpdateOutline();
    }

    private void OnMouseOver()
    {
        // Always update outline while hovering
        UpdateOutline();

        // Stop if final bag or locked
        if (isFinalChipBag || betsLocked || !enableRightClickDrag)
            return;

        // Handle starting a right-click multi-drag
        if (!isMultiDrag && Input.GetMouseButtonDown(1))
        {
            StartMultiDrag();
        }
    }

    private void OnMouseExit()
    {
        if (insufficientFundsOutline != null)
            insufficientFundsOutline.SetActive(false);
    }

    private void OnMouseDown()
    {
        if (isFinalChipBag || betsLocked)
            return;

        // Only handle left click in this method
        if (!Input.GetMouseButtonDown(0))
            return;

        if (currentChip != null) return;

        if (PlayerCurrency.Instance != null && !PlayerCurrency.Instance.TrySpend(chipValue))
        {
            Debug.Log("Not enough currency to take a chip.");
            return;
        }

        currentChip = Instantiate(chipPrefab, transform.position, Quaternion.identity, chipParent);
        if (useChipScaleRange)
        {
            float scale = Random.Range(minChipScale, maxChipScale);
            currentChip.transform.localScale = Vector3.one * scale;
        }

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

    private void UpdateOutline()
    {
        if (!showOutlineOnInsufficientFunds || insufficientFundsOutline == null)
            return;

        bool insufficient = PlayerCurrency.Instance != null &&
                             PlayerCurrency.Instance.CurrentCurrency < chipValue;
        insufficientFundsOutline.SetActive(insufficient);
    }

    private void StartMultiDrag()
    {
        if (isMultiDrag || isFinalChipBag) return;

        const int multiCount = 5;
        int totalCost = chipValue * multiCount;
        if (PlayerCurrency.Instance != null && !PlayerCurrency.Instance.TrySpend(totalCost))
        {
            Debug.Log("Not enough currency to take multiple chips.");
            return;
        }

        multiDraggers.Clear();

        Vector3 center = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        center.z = 0f;

        float startAngle = -rightClickFanAngleStep * (multiCount - 1) / 2f;
        for (int i = 0; i < multiCount; i++)
        {
            GameObject chip = Instantiate(chipPrefab, center, Quaternion.identity, chipParent);

            if (chip.TryGetComponent(out BetChip betChip))
            {
                betChip.chipValue = chipValue;
            }
            else
            {
                betChip = chip.AddComponent<BetChip>();
                betChip.chipValue = chipValue;
            }

            var dragger = chip.GetComponent<BettingChipDragger>();
            if (dragger == null)
                dragger = chip.AddComponent<BettingChipDragger>();

            dragger.bagCollider = bagCollider;

            float angle = startAngle + rightClickFanAngleStep * i;
            Vector3 offset = new(
                Mathf.Cos(angle * Mathf.Deg2Rad) * rightClickFanRadius,
                Mathf.Sin(angle * Mathf.Deg2Rad) * rightClickFanRadius,
                0f);

            chip.transform.position = center + offset;

            dragger.BeginDrag();
            multiDraggers.Add(dragger);
        }

        isMultiDrag = true;
    }

    private void Update()
    {
        if (isFinalChipBag || betsLocked || !enableRightClickDrag)
            return;

        if (isMultiDrag)
        {
            foreach (var dragger in multiDraggers)
            {
                dragger.DragUpdate();
            }

            if (Input.GetMouseButtonUp(1))
            {
                EndMultiDrag();
            }
        }
    }

    private void OnMouseDrag()
    {
        if (isFinalChipBag || betsLocked) return;

        if (currentDragger != null)
        {
            currentDragger.DragUpdate();
        }
    }

    private void OnMouseUp()
    {
        if (isMultiDrag)
        {
            EndMultiDrag();
            return;
        }

        if (currentDragger != null)
        {
            currentDragger.EndDrag();

            currentDragger = null;
            currentChip = null;
        }
    }

    private void EndMultiDrag()
    {
        Vector3 center = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        center.z = 0f;

        foreach (var dragger in multiDraggers)
        {
            if (dragger == null) continue;

            var chipObj = dragger.gameObject;
            dragger.EndDrag();

            if (chipObj != null)
            {
                Rigidbody2D rb = chipObj.GetComponent<Rigidbody2D>();
                if (rb == null)
                    rb = chipObj.AddComponent<Rigidbody2D>();
                rb.gravityScale = 0f;

                Vector2 dir = ((Vector2)chipObj.transform.position - (Vector2)center).normalized;
                rb.AddForce(dir * rightClickDropForce, ForceMode2D.Impulse);
            }
        }

        multiDraggers.Clear();
        isMultiDrag = false;
    }
}
