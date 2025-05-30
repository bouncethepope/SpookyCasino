using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class BarrierDebug : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log($"🧱 Barrier '{gameObject.name}' hit by '{collision.collider.name}'");
    }
}
