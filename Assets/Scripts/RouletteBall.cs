using UnityEngine;

public class RouletteBall : MonoBehaviour
{
    [Header("Detection Settings")]
    public float timeToConfirm = 3f; // How long the ball must stay in a slot
    public WheelSpinner wheelSpinner; // Optional reference to spinner (not enforced yet)

    private Collider2D currentSlot = null;
    private float timeInSlot = 0f;
    private bool resultSent = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.name.StartsWith("Slot_"))
        {
            Debug.Log($"🎯 Entered slot collider: {other.gameObject.name}");

            currentSlot = other;
            timeInSlot = 0f;
            resultSent = false;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log($"Ball collided with: {collision.collider.name}");
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Script is alive!");
        }
    }



    private void OnTriggerStay2D(Collider2D other)
    {
        if (other == currentSlot)
        {
            // Optional: skip result if the wheel is still spinning
            if (wheelSpinner != null && wheelSpinner.IsSpinning())
            {
                timeInSlot = 0f;
                return;
            }

            timeInSlot += Time.deltaTime;

            if (!resultSent && timeInSlot >= timeToConfirm)
            {
                Debug.Log($"✅ Ball settled in slot: {currentSlot.gameObject.name}");
                resultSent = true;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other == currentSlot)
        {
            Debug.Log($"❌ Exited slot: {other.gameObject.name}");

            currentSlot = null;
            timeInSlot = 0f;
            resultSent = false;
        }
    }
}
