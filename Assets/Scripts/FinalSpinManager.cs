using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class FinalSpinManager : MonoBehaviour
{
    public static FinalSpinManager Instance { get; private set; }
    public const string AllInPrefabName = "BC AllIn(Clone)";

    [Header("Camera")]
    public Camera mainCamera;
    public Camera finalSpinCamera;
    [Tooltip("Offset from the ball when following during the final spin")]
    public Vector3 cameraOffset = new Vector3(0f, 0f, -10f);
    [Tooltip("How quickly the final spin camera follows the ball")]
    public float followSmooth = 5f;

    [Header("Lighting")]
    [Tooltip("Lights that will be dimmed during the final spin")]
    public Light2D[] lightsToDim;
    [Range(0f, 1f)]
    [Tooltip("Multiplier applied to light intensity during final spin")]
    public float dimMultiplier = 0.2f;

    [Header("Activation")]
    [Tooltip("Objects enabled during the final spin and restored afterward")]
    public GameObject[] objectsToActivate;

    [Header("Slow Motion")]
    [Tooltip("Time scale applied while the final spin camera is active")]
    [Range(0f, 1f)]
    public float slowMotionScale = 0.3f;

    [Header("Audio")]
    [Tooltip("Audio source used to play final spin sounds")]
    public AudioSource audioSource;
    [Tooltip("Clip played once when the final spin begins")]
    public AudioClip launchClip;
    [Tooltip("Clip looped while the ball is spinning after the launch clip finishes")]
    public AudioClip loopClip;
    [Tooltip("Clip played once when the final spin ends")]
    public AudioClip endClip;

    private float originalTimeScale = 1f;
    private float originalFixedDelta = 0.02f;

    private float[] originalIntensities;
    private bool finalSpinActive = false;
    private Transform targetBall = null;
    private bool[] originalObjectStates;
    private Coroutine audioRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (finalSpinCamera != null)
            finalSpinCamera.gameObject.SetActive(false);

        if (lightsToDim != null && lightsToDim.Length > 0)
        {
            originalIntensities = new float[lightsToDim.Length];
            for (int i = 0; i < lightsToDim.Length; i++)
            {
                if (lightsToDim[i] != null)
                    originalIntensities[i] = lightsToDim[i].intensity;
            }
        }

        if (objectsToActivate != null && objectsToActivate.Length > 0)
        {
            originalObjectStates = new bool[objectsToActivate.Length];
            for (int i = 0; i < objectsToActivate.Length; i++)
            {
                if (objectsToActivate[i] != null)
                    originalObjectStates[i] = objectsToActivate[i].activeSelf;
            }
        }
    }

    private void LateUpdate()
    {
        if (!finalSpinActive || targetBall == null || finalSpinCamera == null)
            return;

        Vector3 desired = targetBall.position + cameraOffset;
        finalSpinCamera.transform.position = Vector3.Lerp(
            finalSpinCamera.transform.position,
            desired,
            followSmooth * Time.deltaTime);
    }

    public void OnBallLaunched(Transform ball)
    {
        // Only trigger the effect if the designated all-in prefab exists
        if (GameObject.Find(AllInPrefabName) == null)
            return;

        BeginFinalSpin(ball);
    }

    public void OnBallLocked()
    {
        if (finalSpinActive)
            EndFinalSpin();
    }

    private void BeginFinalSpin(Transform ball)
    {
        if (ball == null || finalSpinActive)
            return;

        targetBall = ball;
        finalSpinActive = true;

        // Apply slow motion
        originalTimeScale = Time.timeScale;
        originalFixedDelta = Time.fixedDeltaTime;
        Time.timeScale = slowMotionScale;
        Time.fixedDeltaTime = originalFixedDelta * slowMotionScale;

        if (mainCamera != null)
            mainCamera.enabled = false;
        if (finalSpinCamera != null)
            finalSpinCamera.gameObject.SetActive(true);

        if (lightsToDim != null)
        {
            for (int i = 0; i < lightsToDim.Length; i++)
            {
                if (lightsToDim[i] != null)
                    lightsToDim[i].intensity *= dimMultiplier;
            }
        }

        if (objectsToActivate != null)
        {
            for (int i = 0; i < objectsToActivate.Length; i++)
            {
                if (objectsToActivate[i] != null)
                    objectsToActivate[i].SetActive(true);
            }
        }

        if (audioRoutine != null)
            StopCoroutine(audioRoutine);
        audioRoutine = StartCoroutine(PlayFinalSpinAudio());
    }

    private void EndFinalSpin()
    {
        finalSpinActive = false;
        targetBall = null;

        if (finalSpinCamera != null)
            finalSpinCamera.gameObject.SetActive(false);
        if (mainCamera != null)
            mainCamera.enabled = true;

        if (lightsToDim != null && originalIntensities != null)
        {
            for (int i = 0; i < lightsToDim.Length && i < originalIntensities.Length; i++)
            {
                if (lightsToDim[i] != null)
                    lightsToDim[i].intensity = originalIntensities[i];
            }
        }

        if (objectsToActivate != null)
        {
            for (int i = 0; i < objectsToActivate.Length; i++)
            {
                if (objectsToActivate[i] != null)
                {
                    bool state = originalObjectStates != null && i < originalObjectStates.Length
                        ? originalObjectStates[i]
                        : false;
                    objectsToActivate[i].SetActive(state);
                }
            }
        }

        if (audioRoutine != null)
        {
            StopCoroutine(audioRoutine);
            audioRoutine = null;
        }
        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.loop = false;
            if (endClip != null)
            {
                audioSource.clip = endClip;
                audioSource.Play();
            }
        }

        // Restore time scale
        Time.timeScale = originalTimeScale;
        Time.fixedDeltaTime = originalFixedDelta;
    }

    private IEnumerator PlayFinalSpinAudio()
    {
        if (audioSource == null)
            yield break;

        if (launchClip != null)
        {
            audioSource.loop = false;
            audioSource.clip = launchClip;
            audioSource.Play();
            yield return new WaitWhile(() => audioSource.isPlaying);
        }

        if (loopClip != null && finalSpinActive)
        {
            audioSource.loop = true;
            audioSource.clip = loopClip;
            audioSource.Play();
        }
    }
}
