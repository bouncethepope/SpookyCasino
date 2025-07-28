using PixelCrushers.DialogueSystem;
using TMPro;
using UnityEngine;

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
        DialogueLua.SetVariable("IngameValues.Currency", CurrentCurrency);
        UpdateUI();
    }

    public bool TrySpend(int amount)
    {
        if (CurrentCurrency < amount)
            return false;

        CurrentCurrency -= amount;
        UpdateUI();
        DialogueLua.SetVariable("IngameValues.Currency", CurrentCurrency);
        return true;
    }

    public void AddCurrency(int amount)
    {
        CurrentCurrency += amount;
        UpdateUI();
        DialogueLua.SetVariable("IngameValues.Currency", CurrentCurrency);
    }

    private void UpdateUI()
    {
        if (currencyText != null)
        {
            currencyText.text = $"Currency: {CurrentCurrency}";
        }
    }

    //Opens Currency Addition in Lua
    private void OnEnable()
    {
        Lua.RegisterFunction("AddCurrency", this, SymbolExtensions.GetMethodInfo(() => AddCurrencyLua(0)));
    }

    private void OnDisable()
    {
        Lua.UnregisterFunction("AddCurrency");
    }

    // Wrapper for Lua — must take a single `double` argument
    public void AddCurrencyLua(double amount)
    {
        AddCurrency((int)amount);
    }


}
