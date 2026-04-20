using System;
using System.IO;
using UnityEngine;

/// Shared state passed between Timer App and Farm App via a local JSON file.
/// Timer App writes it; Farm App reads it.
[Serializable]
public class FocusFarmState
{
    public bool  isRunning;
    public bool  isResting;
    public float timeRemaining;
    public float totalDuration;
    public int   completedCycles;
    public float focusPoints;
    public int   totalSessions;
    public bool  boostActive;
    public float boostMultiplier;
    public long  timestamp; // Unix ms — used to detect stale data
}

public static class SharedState
{
    static readonly string _folder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "FocusFarm");
    static readonly string _path = Path.Combine(_folder, "state.json");

    // ── Writer (Timer App calls this) ─────────────────────────────────
    public static void Write(FocusFarmState state)
    {
        try
        {
            if (!Directory.Exists(_folder)) Directory.CreateDirectory(_folder);
            state.timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            File.WriteAllText(_path, JsonUtility.ToJson(state, true));
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[SharedState] Write failed: {e.Message}");
        }
    }

    // ── Reader (Farm App calls this) ──────────────────────────────────
    public static FocusFarmState Read()
    {
        try
        {
            if (!File.Exists(_path)) return null;
            string json = File.ReadAllText(_path);
            var state = JsonUtility.FromJson<FocusFarmState>(json);
            // If data is older than 10 s, Timer App is probably closed
            long age = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - state.timestamp;
            if (age > 10000) return null;
            return state;
        }
        catch
        {
            return null;
        }
    }

    public static string FilePath => _path;
}
