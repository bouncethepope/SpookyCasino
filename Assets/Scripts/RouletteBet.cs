// Represents a single betting zone
using System.Collections.Generic;
using UnityEngine;

public class RouletteBet : MonoBehaviour
{
    [Header("Associated Winning Slots")]
    public List<GameObject> winningSlots; // Assign slot GameObjects (e.g. Slot_13, Slot_Black, etc.)

    public void EvaluateBet(GameObject winningSlot)
    {
        if (winningSlots.Contains(winningSlot))
        {
            Debug.Log($"✅ WIN on {gameObject.name}");
        }
        else
        {
            Debug.Log($"❌ LOSS on {gameObject.name}");
        }
    }
}
