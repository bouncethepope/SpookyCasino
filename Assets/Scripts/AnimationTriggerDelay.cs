using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Allows an animation event to trigger logic after a configurable delay.
/// Invoke <see cref="Trigger"/> from an animation to wait for the delay
/// before invoking any assigned events.
/// </summary>
public class AnimationTriggerDelay : MonoBehaviour
{
    [Tooltip("Seconds to wait after the animation event before executing logic.")]
    public float delay = 0.5f;

    [Tooltip("Events to run after the delay passes.")]
    public UnityEvent onDelayedTrigger;

    /// <summary>
    /// Called by an animation event to begin the delayed invocation.
    /// </summary>
    public void Trigger()
    {
        StartCoroutine(DelayRoutine());
    }

    private System.Collections.IEnumerator DelayRoutine()
    {
        if (delay > 0f)
            yield return new WaitForSeconds(delay);
        else
            yield return null;

        onDelayedTrigger?.Invoke();
    }
}
