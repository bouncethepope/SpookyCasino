using UnityEngine;

public class ChipDragInputManager : MonoBehaviour
{
    private Camera mainCamera;
    private BettingChipDragger activeChip;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (BettingChipDragger.betsLocked)
                return;

            Vector2 worldPoint = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            Collider2D[] hits = Physics2D.OverlapPointAll(worldPoint);
            foreach (var hit in hits)
            {
                if (hit != null && hit.TryGetComponent(out BettingChipDragger chip))
                {
                    activeChip = chip;
                    activeChip.BeginDrag();
                    break;
                }
            }
        }
        else if (Input.GetMouseButton(0))
        {
            if (activeChip != null)
            {
                activeChip.DragUpdate();
                if (activeChip == null || !activeChip.IsDragging)
                {
                    activeChip = null;
                }
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (activeChip != null)
            {
                activeChip.EndDrag();
                activeChip = null;
            }
        }
    }
}