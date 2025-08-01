using UnityEngine;
using TMPro;
using System.Collections;

public class WinningSlotDisplay : MonoBehaviour
{
    [Header("UI")]
    [Tooltip("Text element used to show the winning number")]
    public TMP_Text resultText;

    [Header("Dolly Movement")]
    [Tooltip("Controls movement and aiming of the dolly")]
    public DollyMover dolly;
    [Tooltip("Delay before dolly moves")]
    public float dollyMoveDelay = 0.5f;
    [Tooltip("Delay before dolly returns home")]
    public float dollyReturnDelay = 1f;

    [Header("Effects")]
    [Tooltip("Prefab spawned at the winning bet location")]
    public GameObject flashPrefab;
    [Tooltip("Delay after dolly arrives before showing flash effect")]
    public float flashSpawnDelay = 0.3f;
    [Tooltip("Transforms representing bet positions indexed by number")]
    public Transform[] tablePositions;

    private GameObject currentFlash;
    private Coroutine routine;

    private void Awake()
    {
        if (resultText != null)
            resultText.gameObject.SetActive(false);
    }

    public void ResetDisplay()
    {
        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }

        if (currentFlash != null)
        {
            Destroy(currentFlash);
            currentFlash = null;
        }

        if (resultText != null)
            resultText.gameObject.SetActive(false);

        if (dolly != null)
            dolly.ResetToHome();
    }

    public void ShowResult(RouletteSlot slot)
    {
        if (slot == null) return;

        if (routine != null)
            StopCoroutine(routine);

        routine = StartCoroutine(DisplayRoutine(slot));
    }

    private IEnumerator DisplayRoutine(RouletteSlot slot)
    {
        Transform target = null;

        if (tablePositions != null && slot.number >= 0 && slot.number < tablePositions.Length)
            target = tablePositions[slot.number];

        if (target != null && dolly != null)
        {
            yield return new WaitForSeconds(dollyMoveDelay);
            dolly.MoveTo(target.position);
            yield return new WaitForSeconds(dolly.moveDuration);
        }

        yield return new WaitForSeconds(flashSpawnDelay);

        if (flashPrefab != null && target != null)
            currentFlash = Instantiate(flashPrefab, target.position, Quaternion.identity);

        if (resultText != null)
        {
            resultText.text = $"Result: {slot.number}";
            resultText.gameObject.SetActive(true);
        }

        if (dolly != null)
        {
            yield return new WaitForSeconds(dollyReturnDelay);
            dolly.ReturnHome();
            yield return new WaitForSeconds(dolly.moveDuration);
        }

        routine = null;
    }
}
