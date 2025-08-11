using UnityEngine;
using System.Collections;

/// <summary>
/// Moves an arm into view after a delay and listens for the player pressing the
/// down arrow key. If the key is pressed before the arm appears, the object is
/// destroyed immediately. Otherwise the arm moves from a start position to an
/// end position, jiggles in place, and then retreats and destroys itself when
/// the key is pressed.
/// </summary>
public class DownKeyArmScript : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("Optional override for the arm's starting position.")]
    public Transform startPoint;
    [Tooltip("Position the arm will move to before waiting for the down key.")]
    public Transform endPoint;
    [Tooltip("Time in seconds to move between start and end points.")]
    public float moveDuration = 1f;

    [Header("Appearance Timing")]
    [Tooltip("Delay before the arm moves on screen.")]
    public float enterDelay = 1f;

    [Header("Hover Jiggle")]
    [Tooltip("How far the arm jiggles while waiting at the end position.")]
    public float jiggleAmplitude = 0.1f;
    [Tooltip("How fast the arm jiggles while waiting at the end position.")]
    public float jiggleFrequency = 2f;

    private Vector3 startPos;
    private Vector3 endPos;
    private bool atEnd = false;
    private bool moving = false;
    private bool downPressed = false;

    private void Awake()
    {
        startPos = startPoint ? startPoint.position : transform.position;
        endPos = endPoint ? endPoint.position : transform.position;
        transform.position = startPos;

        StartCoroutine(EnterRoutine());
    }

    private void Update()
    {
        if (!downPressed && Input.GetKeyDown(KeyCode.DownArrow))
        {
            downPressed = true;
            if (atEnd && !moving)
            {
                StartCoroutine(ExitRoutine());
            }
        }

        if (atEnd && !moving)
        {
            float x = Mathf.Sin(Time.time * jiggleFrequency) * jiggleAmplitude;
            float y = Mathf.Cos(Time.time * jiggleFrequency) * jiggleAmplitude;
            transform.position = endPos + new Vector3(x, y, 0f);
        }
    }

    private IEnumerator EnterRoutine()
    {
        yield return new WaitForSeconds(enterDelay);

        if (downPressed)
        {
            Destroy(gameObject);
            yield break;
        }

        yield return MoveTo(endPos);
        atEnd = true;

        if (downPressed)
        {
            StartCoroutine(ExitRoutine());
        }
    }

    private IEnumerator ExitRoutine()
    {
        atEnd = false;
        yield return MoveTo(startPos);
        Destroy(gameObject);
    }

    private IEnumerator MoveTo(Vector3 target)
    {
        moving = true;
        Vector3 initial = transform.position;
        float elapsed = 0f;
        while (elapsed < moveDuration)
        {
            transform.position = Vector3.Lerp(initial, target, elapsed / moveDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = target;
        moving = false;
        yield return null;
    }
}