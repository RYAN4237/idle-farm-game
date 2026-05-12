using UnityEngine;
using System;

/// Calculates rewards earned while the game was closed and applies them on load.
/// Called by SaveSystem after all other data is restored.
public class OfflineProgressSystem : MonoBehaviour
{
    public static OfflineProgressSystem Instance { get; private set; }

    // Passive FP earned per second regardless of farming (idle income baseline)
    [SerializeField] float passiveIncomePerSecond = 0.5f;

    // Cap how much offline time we credit (24 hours max)
    const float MAX_OFFLINE_SECONDS = 86400f;

    const string KEY_QUIT_TIME = "save_quit_timestamp";

    // Result of last offline calculation — shown in UI
    public float LastOfflineSeconds  { get; private set; }
    public float LastOfflineFP       { get; private set; }
    public int   LastOfflineHarvests { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // Called by SaveSystem.SaveGame() — record quit time
    public void RecordQuitTime()
    {
        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        PlayerPrefs.SetString(KEY_QUIT_TIME, now.ToString());
    }

    // Called by SaveSystem.LoadGame() after plots/shop are restored.
    // Returns total FP earned offline (already added to ResourceSystem).
    public float ApplyOfflineProgress()
    {
        if (!PlayerPrefs.HasKey(KEY_QUIT_TIME)) return 0f;

        long quitTime = long.Parse(PlayerPrefs.GetString(KEY_QUIT_TIME));
        long nowTime  = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        float elapsed = Mathf.Clamp((float)(nowTime - quitTime), 0f, MAX_OFFLINE_SECONDS);

        if (elapsed < 5f) return 0f; // too short to matter

        LastOfflineSeconds  = elapsed;
        LastOfflineFP       = 0f;
        LastOfflineHarvests = 0;

        // ── 1. Passive idle income ────────────────────────────────────
        float passiveFP = passiveIncomePerSecond * elapsed;
        LastOfflineFP += passiveFP;

        // ── 2. Farm plot catch-up ─────────────────────────────────────
        // For each growing plot, figure out how many full harvest cycles
        // completed during offline time and credit the rewards.
        var plots = FindObjectsByType<FarmPlot>(FindObjectsSortMode.None);
        foreach (var plot in plots)
        {
            if (plot.isLocked) continue;
            if (plot.State == FarmPlot.PlotState.Empty) continue;

            float duration = plot.growthDuration;
            if (duration <= 0f) continue;

            float timeRemaining = plot.GetGrowTimerRemaining();

            // Time until first harvest
            float timeToFirstHarvest = (plot.State == FarmPlot.PlotState.Ready)
                ? 0f
                : timeRemaining;

            if (elapsed < timeToFirstHarvest) continue; // didn't even finish once

            float timeAfterFirst = elapsed - timeToFirstHarvest;
            int   extraCycles    = (int)(timeAfterFirst / duration);
            int   totalHarvests  = 1 + extraCycles;

            // Credit FP for each completed harvest
            float rewardPerHarvest = plot.harvestReward;
            float plotFP           = totalHarvests * rewardPerHarvest;
            LastOfflineFP       += plotFP;
            LastOfflineHarvests += totalHarvests;

            // Advance plot state: how far through the last cycle?
            float remainder = timeAfterFirst - extraCycles * duration;
            float progress  = 1f - (remainder / duration); // progress into new cycle
            plot.RestoreGrowingState(plot.GetActiveCrop(), remainder);
        }

        // ── 3. Apply to ResourceSystem ────────────────────────────────
        if (LastOfflineFP > 0f && ResourceSystem.Instance != null)
            ResourceSystem.Instance.AddFocusPoints(LastOfflineFP);

        Debug.Log($"[Offline] {elapsed:F0}s offline → +{LastOfflineFP:F0} FP " +
                  $"({LastOfflineHarvests} harvests + {passiveFP:F0} passive)");

        return LastOfflineFP;
    }
}
