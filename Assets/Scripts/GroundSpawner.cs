using UnityEngine;

public class GroundSpawner : MonoBehaviour
{
    void Start()
    {
        GameObject ground = new GameObject("TestGround");
        ground.transform.position = Vector3.zero;

        // Add visual representation (optional)
        var sr = ground.AddComponent<SpriteRenderer>();
        sr.sprite = GenerateWhitePixel();
        sr.color = Color.gray;
        ground.transform.localScale = new Vector3(5f, 0.5f, 1f); // wider platform

        // Add physics
        var box = ground.AddComponent<BoxCollider2D>();
        box.isTrigger = false;

        var rb = ground.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Static;

        ground.AddComponent<CollisionLogger>();
    }

    // Creates a 1x1 white texture sprite
    Sprite GenerateWhitePixel()
    {
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), Vector2.one * 0.5f);
    }
}
