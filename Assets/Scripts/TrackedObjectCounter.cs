using System.Collections.Generic;
using UnityEngine;
using PixelCrushers.DialogueSystem;

public class TrackedObjectCounter : MonoBehaviour
{
    public static TrackedObjectCounter Instance { get; private set; }
    private readonly Dictionary<string, int> counts = new();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Register Lua functions once:
        Lua.RegisterFunction("CountObjects", this, GetType().GetMethod(nameof(Lua_CountObjects)));
        Lua.RegisterFunction("IsCountAtLeast", this, GetType().GetMethod(nameof(Lua_IsCountAtLeast)));
    }

    public void Adjust(string key, int delta)
    {
        if (!counts.ContainsKey(key)) counts[key] = 0;
        counts[key] += delta;
        // Optional: mirror into a Lua variable if you prefer variables over functions:
        DialogueLua.SetVariable($"Counts.{key}", counts[key]);
    }

    public int GetCount(string key) => counts.TryGetValue(key, out var n) ? n : 0;

    // Lua bindings:
    public double Lua_CountObjects(string key) => GetCount(key);
    public double Lua_IsCountAtLeast(string key, double threshold) => GetCount(key) >= (int)threshold ? 1 : 0;
}
