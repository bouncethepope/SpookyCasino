using UnityEngine;

public class FishAnimatorTrigger : MonoBehaviour
{
    [Header("Setup")]
    public Animator animator;

    [Tooltip("Name of the flop state in the Animator")]
    public string flopStateName = "FishFlopV2";

    [Tooltip("Name of the swim state in the Animator")]
    public string swimStateName = "FishSwim";

    [Header("Settings")]
    [Tooltip("How many times the fish should flop")]
    public int flopCount = 2;

    private bool isFlopping = false;

    [ContextMenu("Trigger Flops")]
    public void TriggerFlops()
    {
        if (!isFlopping)
            StartCoroutine(PlayFlops());
    }

    private System.Collections.IEnumerator PlayFlops()
    {
        isFlopping = true;
        int played = 0;

        while (played < flopCount)
        {
            animator.CrossFade(flopStateName, 0f, 0, 0f); // force restart
            yield return new WaitUntil(() =>
                animator.GetCurrentAnimatorStateInfo(0).IsName(flopStateName) &&
                animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f
            );
            played++;
        }

        animator.Play(swimStateName);
        isFlopping = false;
    }
}
