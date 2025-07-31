using System.Collections;
using UnityEngine;

/// <summary>
/// Centralizes timing for post-spin events such as bet evaluation,
/// dolly movement and particle effects. Other systems can delay or
/// override these timings (e.g. a dialogue system).
/// </summary>
public class EventTimingSystem : MonoBehaviour
{
    [Header("References")]
    public BetEvaluator betEvaluator;
    public WinningSlotDisplay slotDisplay;

    [Header("Event Delays")]
    public float evaluationDelay = 0f;
    public float displayDelay = 0f;

    /// <summary>
    /// Begins the sequence of post-spin events for the provided slot.
    /// </summary>
    public void HandlePostSpin(RouletteSlot slot)
    {
        if (slot == null)
            return;
        StartCoroutine(PostSpinRoutine(slot));
    }

    private IEnumerator PostSpinRoutine(RouletteSlot slot)
    {
        if (evaluationDelay > 0f)
            yield return new WaitForSeconds(evaluationDelay);

        if (betEvaluator != null)
        {
            betEvaluator.GatherChipsFromScene();
            betEvaluator.EvaluateBets();
        }

        if (displayDelay > 0f)
            yield return new WaitForSeconds(displayDelay);

        if (slotDisplay != null)
            slotDisplay.ShowResult(slot);
    }
}
