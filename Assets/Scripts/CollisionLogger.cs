using UnityEngine;

public class CollisionLogger : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log($"{gameObject.name} was touched by {collision.collider.name}");
    }
}
