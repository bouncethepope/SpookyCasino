using UnityEngine;
using TMPro;

public class PlayerCurrency : MonoBehaviour
{
    public static PlayerCurrency Instance { get; private set; }

    [Header("Starting Amount")]
    public int startingCurrency = 500;
    public TMP_Text currencyText;

    public int CurrentCurrency { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        CurrentCurrency = startingCurrency;
        UpdateUI();
    }

    public bool TrySpend(int amount)
    {
        if (CurrentCurrency < amount)
            return false;

        CurrentCurrency -= amount;
        UpdateUI();
        return true;
    }

    public void AddCurrency(int amount)
    {
        CurrentCurrency += amount;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (currencyText != null)
        {
            currencyText.text = $"Currency: {CurrentCurrency}";
        }
    }
}
