using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class TextSizeOscillator : MonoBehaviour
{
    [Header("Oscillation Settings")]
    [Tooltip("Speed of the size oscillation.")]
    public float oscillationSpeed = 2f;

    [Tooltip("Minimum font size during oscillation.")]
    public float minSize = 10f;

    [Tooltip("Maximum font size during oscillation.")]
    public float maxSize = 20f;

    private TextMeshProUGUI tmpText;
    private float baseSize;

    void Awake()
    {
        tmpText = GetComponent<TextMeshProUGUI>();
        baseSize = tmpText.fontSize;
    }

    void Update()
    {
        float t = (Mathf.Sin(Time.time * oscillationSpeed) + 1f) / 2f; // 0 → 1
        tmpText.fontSize = Mathf.Lerp(minSize, maxSize, t);
    }
}
