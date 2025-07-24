using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Simple helper that records the result of each spin and displays it
/// in a scrolling history list. Attach this to a UI object and assign a
/// TMP_Text component for <see cref="historyText"/>. Call
/// <see cref="StartRound"/> when a new betting round begins (e.g., when
/// bets are unlocked) and <see cref="EndRound"/> once rewards have been
/// paid out.
/// </summary>
public class BetHistorySign : MonoBehaviour
{
    public static BetHistorySign Instance { get; private set; }

    [Header("UI")]
    [Tooltip("Text element used to display bet history entries")]
    public TMP_Text historyText;
    [Tooltip("Maximum number of entries to keep in history")] public int maxEntries = 10;

    private readonly List<string> entries = new();
    private int startCurrency = 500;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// Capture the player's currency at the beginning of a round.
    /// </summary>
    public void StartRound()
    {
        startCurrency = PlayerCurrency.Instance?.CurrentCurrency ?? 0;
    }

    /// <summary>
    /// Record the final result of the round.
    /// </summary>
    /// <param name="slot">Winning roulette slot.</param>
    public void EndRound(RouletteSlot slot)
    {
        if (slot == null)
            return;

        int endCurrency = PlayerCurrency.Instance?.CurrentCurrency ?? 0;
        int delta = endCurrency - startCurrency;

        // choose a hex color so the number text matches the slot color
        string hex = slot.color switch
        {
            RouletteColor.Red => "#ff0000",
            RouletteColor.Black => "#000000",
            _ => "#00ff00" // treat 0 as green
        };

        // format currency change, showing minus numbers when appropriate
        string deltaText = delta > 0 ? $"+{delta}" : delta.ToString();

        // apply rich text color tags to the winning number
        string entry = $"<color={hex}>{slot.number}</color>  {deltaText}";
        entries.Insert(0, entry);
        if (entries.Count > maxEntries)
            entries.RemoveAt(entries.Count - 1);

        if (historyText != null)
            historyText.text = string.Join("\n", entries);
    }
}

