using TMPro;
using UnityEngine;

public class StatsHandler : MonoBehaviour
{
    public static StatsHandler Instance { get; private set; }

    [Header("UI")]
    public TMP_Text statsText; // Assign the TMP text element in Inspector

    [Header("References")]
    public BetEvaluator betEvaluator;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Try to auto-find TMP text if not assigned
        if (statsText == null)
            statsText = GetComponent<TMP_Text>();

        // Try to auto-find BetEvaluator if not assigned
        if (betEvaluator == null)
            betEvaluator = FindFirstObjectByType<BetEvaluator>();

        // Hide stats text GameObject initially
        if (statsText != null)
            statsText.gameObject.SetActive(false);
    }

    public void ShowStats()
    {
        if (betEvaluator == null)
        {
            Debug.LogWarning("StatsHandler is missing a reference to BetEvaluator.");
            return;
        }

        int successful = betEvaluator.totalSuccessful;
        int unsuccessful = betEvaluator.totalUnsuccessful;
        int rejected = betEvaluator.totalRejected;
        int total = successful + unsuccessful + rejected;

        if (statsText != null)
        {
            statsText.text =
                $"Successful Bets: {successful}\n" +
                $"Unsuccessful Bets: {unsuccessful}\n" +
                $"Rejected Chips: {rejected}\n" +
                $"Total Chips Used: {total}";

            statsText.gameObject.SetActive(true); // Enable GameObject
        }
    }

    public void Hide()
    {
        if (statsText != null)
            statsText.gameObject.SetActive(false); // Disable GameObject
    }
}
