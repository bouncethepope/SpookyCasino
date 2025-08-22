using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using PixelCrushers.DialogueSystem; // <-- Needed for DialogueLua

public class TutorialSpotlight : MonoBehaviour
{
    [System.Serializable]
    public class SpotlightStep
    {
        [Header("Movement")]
        [Tooltip("Destination position for this step")] public Transform location;
        [Tooltip("Seconds to remain at this location before moving on")] public float waitTime = 0f;

        [Header("Events")]
        [Tooltip("Invoked when the spotlight reaches this location")] public UnityEvent onReached;

        [Header("Dialogue System (optional)")]
        [Tooltip("If true, set a Dialogue System Lua variable when this step is reached")]
        public bool setDialogueBool = false;
        [Tooltip("Lua variable name, e.g. IngameValues.TutorialStep1 or Tutorial.Step1Reached")]
        public string dialogueVariable;
        [Tooltip("Value to assign to the Lua variable")]
        public bool dialogueValue = true;
    }

    [Tooltip("Transform of the spotlight to move")] public Transform spotlight;
    [Tooltip("Ordered list of locations the spotlight will visit")] public SpotlightStep[] steps;
    [Tooltip("Units per second movement speed")] public float moveSpeed = 5f;
    [Tooltip("How fast the spotlight fades out after the last step")] public float fadeOutSpeed = 1f;
    [Tooltip("Invoked after the spotlight has faded out completely")] public UnityEvent onSequenceComplete;

    private int currentStep = 0;
    private SpriteRenderer spriteRenderer;
    private Light spotLight;

    private void Awake()
    {
        if (spotlight != null)
        {
            spriteRenderer = spotlight.GetComponent<SpriteRenderer>();
            spotLight = spotlight.GetComponent<Light>();
        }
        else
        {
            Debug.LogWarning("TutorialSpotlight: No spotlight transform assigned.", this);
        }
    }

    private void Start()
    {
        if (spotlight != null && steps != null && steps.Length > 0)
        {
            StartCoroutine(RunSequence());
        }
    }

    private IEnumerator RunSequence()
    {
        while (currentStep < steps.Length)
        {
            var step = steps[currentStep];
            Transform target = step.location;

            if (target != null)
            {
                while (Vector3.Distance(spotlight.position, target.position) > 0.01f)
                {
                    spotlight.position = Vector3.MoveTowards(spotlight.position, target.position, moveSpeed * Time.deltaTime);
                    yield return null;
                }
            }

            // Fire UnityEvent
            step.onReached?.Invoke();

            // Optionally set a Dialogue System boolean
            if (step.setDialogueBool && !string.IsNullOrWhiteSpace(step.dialogueVariable))
            {
                DialogueLua.SetVariable(step.dialogueVariable, step.dialogueValue);
                // (Optional) Debug:
                // Debug.Log($"TutorialSpotlight: Set Dialogue var '{step.dialogueVariable}' = {step.dialogueValue}");
            }

            // Optional wait
            if (step.waitTime > 0f)
            {
                yield return new WaitForSeconds(step.waitTime);
            }

            currentStep++;
        }

        yield return StartCoroutine(FadeOut());
        onSequenceComplete?.Invoke();
    }

    private IEnumerator FadeOut()
    {
        if (spriteRenderer != null)
        {
            Color c = spriteRenderer.color;
            while (c.a > 0f)
            {
                c.a = Mathf.MoveTowards(c.a, 0f, fadeOutSpeed * Time.deltaTime);
                spriteRenderer.color = c;
                yield return null;
            }
        }
        else if (spotLight != null)
        {
            while (spotLight.intensity > 0f)
            {
                spotLight.intensity = Mathf.MoveTowards(spotLight.intensity, 0f, fadeOutSpeed * Time.deltaTime);
                yield return null;
            }
        }
        else
        {
            yield return null;
        }
    }
}
