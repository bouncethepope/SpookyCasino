using UnityEngine;
using DG.Tweening;

public class DollyMover : MonoBehaviour
{
    [Tooltip("Where the dolly rests when idle")] public Transform home;
    [Tooltip("Time taken for the dolly to move to its destination")] public float moveDuration = 1f;

    public void MoveTo(Vector3 targetPosition)
    {
        AimTowards(targetPosition);
        transform.DOMove(targetPosition, moveDuration);
    }

    public void ReturnHome()
    {
        if (home == null) return;
        AimTowards(home.position);
        transform.DOMove(home.position, moveDuration).OnComplete(() =>
        {
            transform.rotation = home.rotation;
        });
    }

    public void ResetToHome()
    {
        if (home == null) return;
        transform.position = home.position;
        transform.rotation = home.rotation;
    }

    private void AimTowards(Vector3 targetPosition)
    {
        Vector3 direction = targetPosition - transform.position;
        if (direction.sqrMagnitude > 0.0001f)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }
    }
}