using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// Auto-harvests ready plots and re-plants them at a set interval.
/// Player buys this with FP. Multiple levels = faster interval.
public class AutoFarmer : MonoBehaviour
{
    public static AutoFarmer Instance { get; private set; }

    [Header("Purchase Costs per Level")]
    public float[] levelCosts     = { 200f, 500f, 1000f };
    public float[] intervals      = { 8f,   5f,   2f    }; // seconds between actions
    public int     CurrentLevel   { get; private set; } = 0; // 0 = not purchased

    private Coroutine farmRoutine;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // ── Purchase ──────────────────────────────────────────
    public bool CanUpgrade() => CurrentLevel < levelCosts.Length;

    public float UpgradeCost() =>
        CanUpgrade() ? levelCosts[CurrentLevel] : -1f;

    public bool TryUpgrade()
    {
        if (!CanUpgrade()) return false;
        float cost = UpgradeCost();
        if (!ResourceSystem.Instance.SpendFocusPoints(cost)) return false;

        CurrentLevel++;
        Debug.Log($"AutoFarmer upgraded to Level {CurrentLevel}!");

        // Restart routine with new interval
        if (farmRoutine != null) StopCoroutine(farmRoutine);
        farmRoutine = StartCoroutine(FarmRoutine());
        return true;
    }

    // ── Auto-farm loop ────────────────────────────────────
    IEnumerator FarmRoutine()
    {
        while (true)
        {
            float interval = intervals[Mathf.Clamp(CurrentLevel - 1, 0, intervals.Length - 1)];
            yield return new WaitForSeconds(interval);

            var plots = FindObjectsByType<FarmPlot>(FindObjectsSortMode.None);
            foreach (var plot in plots)
            {
                if (plot.State == FarmPlot.PlotState.Ready)
                {
                    plot.Harvest();
                    yield return new WaitForSeconds(0.3f); // small delay between harvests
                }
            }

            // Re-plant empty plots
            yield return new WaitForSeconds(0.5f);
            foreach (var plot in plots)
            {
                if (plot.State == FarmPlot.PlotState.Empty && !plot.isLocked)
                {
                    plot.Plant();
                    yield return new WaitForSeconds(0.2f);
                }
            }
        }
    }

    // ── Save/Load ──────────────────────────────────────────
    public void RestoreLevel(int level)
    {
        CurrentLevel = level;
        if (CurrentLevel > 0)
        {
            if (farmRoutine != null) StopCoroutine(farmRoutine);
            farmRoutine = StartCoroutine(FarmRoutine());
        }
    }
}
