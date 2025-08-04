using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Button))]
public class BagDistributionButton : MonoBehaviour
{
    [Tooltip("Spawns chip bags when purchased")] public ChipBagDispenser bagDispenser;
    [Tooltip("Currency cost for each bag")] public int[] bagCosts;
    [Tooltip("UI text showing the cost of the next bag")] public TMP_Text costText;

    private int nextCostIndex = 0;
    private Button uiButton;
    private bool usedThisRound = false;

    private void Awake()
    {
        uiButton = GetComponent<Button>();
    }

    private void Start()
    {
        ResetButtonState();
    }

    public void TryDispenseBag()
    {
        if (usedThisRound || bagDispenser == null || bagCosts == null || PlayerCurrency.Instance == null)
            return;

        if (nextCostIndex >= bagCosts.Length)
            return;

        int cost = bagCosts[nextCostIndex];
        if (PlayerCurrency.Instance.TrySpend(cost))
        {
            bagDispenser.DispenseNextBag();
            nextCostIndex++;
            usedThisRound = true;
            if (uiButton != null)
                uiButton.interactable = false;
            UpdateCostDisplay();
        }
    }

    private void UpdateCostDisplay()
    {
        if (costText == null)
            return;

        if (bagCosts == null || nextCostIndex >= bagCosts.Length)
            costText.text = string.Empty;
        else
            costText.text = $"Cost: {bagCosts[nextCostIndex]}";
    }

    public void ResetButtonState()
    {
        usedThisRound = false;
        if (uiButton != null)
            uiButton.interactable = nextCostIndex < (bagCosts?.Length ?? 0);
        UpdateCostDisplay();
    }
}