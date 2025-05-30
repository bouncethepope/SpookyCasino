using System.Collections.Generic;
using UnityEngine;

public class RouletteWheelManager : MonoBehaviour
{
    [Header("Wheel Configuration")]
    [Range(2, 100)]
    public int numberOfSlots = 39;
    public float wheelRadius = 5f;

    [Header("Slot & Barrier Setup")]
    public List<SlotData> slotTemplates = new();
    public GameObject barrierPrefab;
    public GameObject numberLabelPrefab;

    [Header("Generated Parents")]
    public Transform slotParent;
    public Transform barrierParent;
    public Transform labelParent;

    private List<GameObject> currentSlots = new();
    private List<GameObject> currentBarriers = new();
    private List<GameObject> currentLabels = new();

    [System.Serializable]
    public class SlotData
    {
        public string label = "Slot";
        public GameObject prefab;
        public Color slotColor = Color.white;
        public bool isBlocked = false;
    }

    private void Start()
    {
        RebuildWheel();
    }

    public void RebuildWheel()
    {
        ClearWheel();

        float angleStep = 360f / numberOfSlots;

        for (int i = 0; i < numberOfSlots; i++)
        {
            float angle = i * angleStep;
            Quaternion rotation = Quaternion.Euler(0, 0, -angle);
            Vector3 position = rotation * Vector3.up * wheelRadius;

            // Slot prefab selection
            SlotData template = slotTemplates[i % slotTemplates.Count];
            GameObject slot = Instantiate(template.prefab, position, rotation, slotParent);
            slot.name = $"Slot_{i}";

            // Scale slot width to span arc length
            float arcLength = 2f * Mathf.PI * wheelRadius * (angleStep / 360f);
            float baseWidth = 1f; // assumes prefab is 1 unit wide at scale.x = 1
            Vector3 scale = slot.transform.localScale;
            scale.x = arcLength / baseWidth;
            slot.transform.localScale = scale;

            // Apply slot color
            if (slot.TryGetComponent(out SpriteRenderer renderer))
            {
                renderer.color = template.isBlocked ? Color.gray : template.slotColor;
            }

            // Store slot reference
            currentSlots.Add(slot);

            // Barrier instantiation
            if (barrierPrefab != null)
            {
                GameObject barrier = Instantiate(barrierPrefab, Vector3.zero, Quaternion.identity, barrierParent);
                float barrierAngle = -angle + (angleStep / 2f);
                barrier.transform.position = Quaternion.Euler(0, 0, barrierAngle) * Vector3.up * wheelRadius;
                barrier.transform.rotation = Quaternion.Euler(0, 0, barrierAngle);
                currentBarriers.Add(barrier);
            }

            // Detached number label
            if (numberLabelPrefab != null)
            {
                GameObject label = Instantiate(numberLabelPrefab, labelParent);
                label.transform.position = Quaternion.Euler(0, 0, -angle) * Vector3.up * (wheelRadius + 0.8f);
                label.transform.rotation = Quaternion.identity;
                label.name = $"Label_{i}";

                TextMesh tm = label.GetComponent<TextMesh>();
                if (tm != null)
                    tm.text = template.label;

                currentLabels.Add(label);
            }
        }
    }

    private void ClearWheel()
    {
        foreach (var s in currentSlots) DestroyImmediate(s);
        foreach (var b in currentBarriers) DestroyImmediate(b);
        foreach (var l in currentLabels) DestroyImmediate(l);

        currentSlots.Clear();
        currentBarriers.Clear();
        currentLabels.Clear();
    }

    // Runtime modification methods
    public void SetSlotBlocked(int index, bool blocked)
    {
        if (index >= 0 && index < slotTemplates.Count)
        {
            slotTemplates[index].isBlocked = blocked;
            RebuildWheel();
        }
    }

    public void SetSlotLabel(int index, string newLabel)
    {
        if (index >= 0 && index < slotTemplates.Count)
        {
            slotTemplates[index].label = newLabel;
            RebuildWheel();
        }
    }

    public void SetSlotColor(int index, Color newColor)
    {
        if (index >= 0 && index < slotTemplates.Count)
        {
            slotTemplates[index].slotColor = newColor;
            RebuildWheel();
        }
    }
}
