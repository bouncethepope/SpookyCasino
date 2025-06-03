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

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public bool IsSpinning()
    {
        return Mathf.Abs(rb.angularVelocity) > spinThreshold;
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
}
