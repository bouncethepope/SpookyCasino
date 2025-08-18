using PixelCrushers.DialogueSystem;
using UnityEngine;
using System.Collections;

public class CurrencyUsableToggle : MonoBehaviour
{
    [Header("Assign the target Usable here (drag from hierarchy)")]
    [SerializeField] private Usable usable;

    private bool subscribed;

    private void Reset()
    {
        // Try to auto-fill if on this object or a child
        if (usable == null) usable = GetComponent<Usable>();
        if (usable == null) usable = GetComponentInChildren<Usable>(true);
    }

    private void Awake()
    {
        // Safety: try to resolve if not assigned
        if (usable == null) usable = GetComponent<Usable>();
        if (usable == null) usable = GetComponentInChildren<Usable>(true);
    }

    private void OnEnable()
    {
        StartCoroutine(EnsureSubscribedAndRefresh());
    }

    private void OnDisable()
    {
        if (subscribed && PlayerCurrency.Instance != null)
        {
            PlayerCurrency.Instance.CurrencyChanged -= OnCurrencyChanged;
            subscribed = false;
        }
    }

    private IEnumerator EnsureSubscribedAndRefresh()
    {
        // Wait until PlayerCurrency singleton exists (handles scene load races)
        while (PlayerCurrency.Instance == null)
            yield return null;

        if (!subscribed)
        {
            PlayerCurrency.Instance.CurrencyChanged += OnCurrencyChanged;
            subscribed = true;
        }

        // Apply initial state based on current currency
        OnCurrencyChanged(PlayerCurrency.Instance.CurrentCurrency);
    }

    private void OnCurrencyChanged(int currentCurrency)
    {
        if (usable == null)
        {
            Debug.LogWarning($"{nameof(CurrencyUsableToggle)}: No Usable assigned/found on {name}.");
            return;
        }

        // Enable Usable when currency == 0, disable otherwise
        bool enable = (currentCurrency == 0);
        usable.enabled = enable;

        // Debug to verify it's running
        // (Remove later if noisy)
        Debug.Log($"{nameof(CurrencyUsableToggle)} on {usable.name}: currency={currentCurrency} -> usable.enabled={enable}");
    }
}
