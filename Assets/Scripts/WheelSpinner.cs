using UnityEngine;
using System.Collections;

public class WheelSpinner : MonoBehaviour
{
    [Header("Spin Settings")]
    public float initialSpinSpeed = 720f; // Degrees per second
    public float friction = 50f;          // Degrees per second squared
    public bool spinOnStart = false;

    public float spinThreshold = 5f;      // Minimum spin for result
    private Rigidbody2D rb;

    private float currentSpinSpeed = 0f;
    private bool isSpinning = false;
    private Quaternion initialRotation;

    [Header("Audio")]
    [Tooltip("Sound played while the wheel is spinning")] public AudioClip spinSound;
    [Tooltip("Minimum pitch when the wheel slows to a stop")]
    [Range(0.1f, 3f)] public float minSpinPitch = 0.5f;
    [Tooltip("Maximum pitch when the wheel is at full speed")]
    [Range(0.1f, 3f)] public float maxSpinPitch = 1f;

    [Header("Ball Hit Audio")]
    [Tooltip("Sound played when the ball first hits the wheel")]
    public AudioClip ballHitSound;
    [Tooltip("Maximum volume of the ball hit sound at full speed")]
    [Range(0f, 1f)] public float maxBallHitVolume = 1f;
    [Tooltip("Number of times to play the ball hit sound when spin starts")]
    public int ballHitLoopCount = 1;

    private AudioSource audioSource;
    private AudioSource ballHitAudioSource;
    private Coroutine ballHitRoutine;
    private float ballHitFade = 0f;




    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        initialRotation = transform.rotation;
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        if (audioSource != null)
        {
            audioSource.loop = true;
            audioSource.playOnAwake = false;
            audioSource.pitch = maxSpinPitch;
        }

        ballHitAudioSource = gameObject.AddComponent<AudioSource>();
        ballHitAudioSource.playOnAwake = false;
        ballHitAudioSource.loop = false;
    }

    public bool IsSpinning()
    {
        // Check the internally tracked spin speed rather than the rigidbody's
        // angular velocity since the wheel is rotated manually.
        return Mathf.Abs(currentSpinSpeed) > spinThreshold;
    }

    /// Current angular speed of the wheel in degrees per second.
    /// Useful for UI prompts or game rules based on wheel movement.
    public float GetCurrentSpinSpeed()
    {
        return currentSpinSpeed;
    }

    void Start()
    {
        if (spinOnStart)
            StartSpin();
    }

    void Update()
    {
        if (isSpinning)
        {
            // Rotate the wheel
            transform.Rotate(0f, 0f, -currentSpinSpeed * Time.deltaTime);

            // Apply friction
            currentSpinSpeed = Mathf.MoveTowards(currentSpinSpeed, 0f, friction * Time.deltaTime);
            float normalized = Mathf.Clamp01(Mathf.Abs(currentSpinSpeed) / Mathf.Max(1f, initialSpinSpeed));

            if (audioSource != null)
            {
                if (!audioSource.isPlaying && spinSound != null)
                {
                    audioSource.clip = spinSound;
                    audioSource.Play();
                }

                audioSource.volume = normalized;
                audioSource.pitch = Mathf.Lerp(minSpinPitch, maxSpinPitch, normalized);
            }

            if (ballHitAudioSource != null && ballHitAudioSource.isPlaying)
            {
                ballHitAudioSource.volume = normalized * maxBallHitVolume * ballHitFade;
            }

            // Stop spinning when speed is near zero
            if (Mathf.Approximately(currentSpinSpeed, 0f))
            {
                isSpinning = false;
                if (audioSource != null && audioSource.isPlaying)
                    audioSource.Stop();
            }
        }
        else
                {
                    if (audioSource != null && audioSource.isPlaying)
                        audioSource.Stop();
                    if (ballHitAudioSource != null && ballHitAudioSource.isPlaying)
                    {
                        ballHitAudioSource.Stop();
                        ballHitAudioSource.loop = false;
                        ballHitFade = 0f;
                        if (ballHitRoutine != null)
                        {
                            StopCoroutine(ballHitRoutine);
                            ballHitRoutine = null;
                        }
                    }
                }
    }

    public void StartSpin()
    {
        currentSpinSpeed = initialSpinSpeed;
        isSpinning = true;
        if (audioSource != null && spinSound != null)
        {
            audioSource.clip = spinSound;
            audioSource.volume = 1f;
            audioSource.pitch = maxSpinPitch;
            if (!audioSource.isPlaying)
                audioSource.Play();
        }

        if (ballHitAudioSource != null && ballHitSound != null && ballHitLoopCount > 0)
        {
            if (ballHitRoutine != null)
                StopCoroutine(ballHitRoutine);
            ballHitRoutine = StartCoroutine(PlayBallHitSound());
        }
    }

    public void ResetSpin()
    {
        isSpinning = false;
        currentSpinSpeed = 0f;
        rb.angularVelocity = 0f;
        transform.rotation = initialRotation;
        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.pitch = maxSpinPitch;
        }

        if (ballHitAudioSource != null)
        {
            ballHitAudioSource.Stop();
            ballHitAudioSource.loop = false;
            ballHitFade = 0f;
        }
        if (ballHitRoutine != null)
        {
            StopCoroutine(ballHitRoutine);
            ballHitRoutine = null;
        }
    }

    /// <summary>
    /// Stops the wheel from spinning without changing its current rotation.
    /// Useful when the wheel orientation should persist between rounds.
    /// </summary>
    public void StopSpin()
    {
        isSpinning = false;
        currentSpinSpeed = 0f;
        rb.angularVelocity = 0f;
        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.pitch = maxSpinPitch;
        }
        if (ballHitAudioSource != null)
        {
            ballHitAudioSource.Stop();
            ballHitAudioSource.loop = false;
            ballHitFade = 0f;
        }
        if (ballHitRoutine != null)
        {
            StopCoroutine(ballHitRoutine);
            ballHitRoutine = null;
        }
    }

    private IEnumerator PlayBallHitSound()
    {
        ballHitFade = 1f;
        ballHitAudioSource.clip = ballHitSound;
        ballHitAudioSource.loop = true;
        float normalized = Mathf.Clamp01(Mathf.Abs(currentSpinSpeed) / Mathf.Max(1f, initialSpinSpeed));
        ballHitAudioSource.volume = normalized * maxBallHitVolume;
        ballHitAudioSource.Play();

        float totalDuration = ballHitSound.length * ballHitLoopCount;
        float elapsed = 0f;
        while (elapsed < totalDuration)
        {
            elapsed += Time.deltaTime;
            ballHitFade = Mathf.Clamp01(1f - (elapsed / totalDuration));
            yield return null;
        }

        ballHitAudioSource.Stop();
        ballHitAudioSource.loop = false;
        ballHitFade = 0f;
        ballHitRoutine = null;
    }
}