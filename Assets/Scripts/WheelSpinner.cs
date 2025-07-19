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


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        initialRotation = transform.rotation;
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

            // Stop spinning when speed is near zero
            if (Mathf.Approximately(currentSpinSpeed, 0f))
                isSpinning = false;
        }
    }

    public void StartSpin()
    {
        currentSpinSpeed = initialSpinSpeed;
        isSpinning = true;
    }

    public void ResetSpin()
    {
        isSpinning = false;
        currentSpinSpeed = 0f;
        rb.angularVelocity = 0f;
        transform.rotation = initialRotation;
    }
}
