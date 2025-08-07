using UnityEngine;

public class Pufferfish : MonoBehaviour
{
    [Header("Path")]
    public Transform startPoint;
    public Transform endPoint;
    public float moveSpeed = 2f;

    [Header("Sine Motion")]
    public float waveAmplitude = 0.5f;
    public float waveFrequency = 1f;

    [Header("Puff Settings")]
    [Tooltip("Random range in seconds before puffing up")]
    public Vector2 puffTimeRange = new Vector2(6f, 8f);
    [Tooltip("Multiplier applied to scale when puffed")]
    public float puffScaleMultiplier = 2f;
    [Tooltip("Force applied to nearby chips when puffed")]
    public float puffForce = 5f;

    [Header("Return To Normal Size")]
    [Tooltip("Should the fish shrink back after puffing?")]
    public bool shrinkBack = true;
    [Tooltip("Delay before shrinking back")]
    public float shrinkDelay = 1f;
    [Tooltip("Time it takes to shrink to the original size")]
    public float shrinkTime = 1f;

    [Header("Lifecycle")]
    [Tooltip("Continue moving back and forth between points while activated")]
    public bool loopPath = false;
    [Tooltip("Destroy the pufferfish when it reaches the start again")]
    public bool destroyOnReturn = false;

    private Vector3 direction;
    private Vector3 sideDirection;
    private float pathLength;
    private bool isPuffed = false;
    private Vector3 initialScale;
    private float travelled = 0f;
    private bool moving = false;
    private bool forward = true;

    void Start()
    {
        initialScale = transform.localScale;
        SetupPath();
    }

    private void SetupPath()
    {
        if (startPoint != null && endPoint != null)
        {
            direction = (endPoint.position - startPoint.position).normalized;
            sideDirection = Vector3.Cross(direction, Vector3.forward).normalized;
            pathLength = Vector3.Distance(startPoint.position, endPoint.position);
        }
        else
        {
            pathLength = 0f;
        }
    }

    public void Activate()
    {
        if (startPoint == null || endPoint == null)
            return;

        SetupPath();
        if (pathLength <= 0f)
            return;

        transform.position = startPoint.position;
        travelled = 0f;
        forward = true;
        moving = true;
        isPuffed = false;
        transform.localScale = initialScale;

        float puffDelay = Random.Range(puffTimeRange.x, puffTimeRange.y);
        CancelInvoke(nameof(PuffUp));
        Invoke(nameof(PuffUp), puffDelay);
    }

    void Update()
    {
        if (!moving || pathLength <= 0f)
            return;

        float step = moveSpeed * Time.deltaTime;
        travelled += step * (forward ? 1f : -1f);
        float clamped = Mathf.Clamp(travelled, 0f, pathLength);
        float t = clamped / pathLength;
        Vector3 basePos = Vector3.Lerp(startPoint.position, endPoint.position, t);
        float offset = Mathf.Sin(Time.time * waveFrequency) * waveAmplitude;
        transform.position = basePos + sideDirection * offset;

        if (forward && travelled >= pathLength)
        {
            forward = false;
        }
        else if (!forward && travelled <= 0f)
        {
            if (loopPath)
            {
                forward = true;
            }
            else
            {
                moving = false;
                CancelInvoke(nameof(PuffUp));
                if (destroyOnReturn)
                    Destroy(gameObject);
            }
        }
    }

    void PuffUp()
    {
        isPuffed = true;
        transform.localScale = initialScale * puffScaleMultiplier;

        if (shrinkBack)
            StartCoroutine(ReturnToSize());
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (!isPuffed)
            return;

        if (other.TryGetComponent(out BetChip betChip))
        {
            Rigidbody2D rb = other.attachedRigidbody;
            if (rb != null)
            {
                Vector2 dir = (other.transform.position - transform.position).normalized;
                rb.AddForce(dir * puffForce, ForceMode2D.Impulse);
            }
        }
    }

    System.Collections.IEnumerator ReturnToSize()
    {
        yield return new WaitForSeconds(shrinkDelay);

        Vector3 startScale = transform.localScale;
        float timer = 0f;
        while (timer < shrinkTime)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / shrinkTime);
            transform.localScale = Vector3.Lerp(startScale, initialScale, t);
            yield return null;
        }

        transform.localScale = initialScale;
        isPuffed = false;

        if (moving)
        {
            float puffDelay = Random.Range(puffTimeRange.x, puffTimeRange.y);
            Invoke(nameof(PuffUp), puffDelay);
        }
    }
}