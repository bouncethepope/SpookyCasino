using UnityEngine;

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

    private AudioSource audioSource;




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

            // Stop spinning when speed is near zero
            if (Mathf.Approximately(currentSpinSpeed, 0f))
            {
                isSpinning = false;
                if (audioSource != null && audioSource.isPlaying)
                    audioSource.Stop();
            }
        }
        else if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
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
    }
}