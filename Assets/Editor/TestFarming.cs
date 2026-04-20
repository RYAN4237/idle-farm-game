using UnityEngine;
using UnityEditor;

public class TestFarming
{
    // Give player 30 FP so they can plant immediately
    public static void GiveFocusPoints()
    {
        if (ResourceSystem.Instance == null) { Debug.LogError("ResourceSystem not found — is game running?"); return; }
        ResourceSystem.Instance.AddFocusPoints(30f);
        Debug.Log("Added 30 FP. Total: " + ResourceSystem.Instance.FocusPoints);
    }

    // Force-plant plot 1
    public static void PlantPlot1()
    {
        var go = GameObject.Find("FarmPlot_1");
        if (go == null) { Debug.LogError("FarmPlot_1 not found"); return; }
        var plot = go.GetComponent<FarmPlot>();
        if (plot == null) { Debug.LogError("FarmPlot component missing"); return; }
        plot.Plant();
        Debug.Log("Planted FarmPlot_1. State: " + plot.State);
    }

    // Force-ready plot 1 (skip growth timer)
    public static void ForceReadyPlot1()
    {
        var go = GameObject.Find("FarmPlot_1");
        if (go == null) { Debug.LogError("FarmPlot_1 not found"); return; }
        var plot = go.GetComponent<FarmPlot>();
        if (plot == null) { Debug.LogError("FarmPlot component missing"); return; }
        plot.SetState(FarmPlot.PlotState.Ready);
        Debug.Log("FarmPlot_1 forced to Ready state.");
    }

    // Harvest plot 1
    public static void HarvestPlot1()
    {
        var go = GameObject.Find("FarmPlot_1");
        if (go == null) { Debug.LogError("FarmPlot_1 not found"); return; }
        var plot = go.GetComponent<FarmPlot>();
        if (plot == null) { Debug.LogError("FarmPlot component missing"); return; }
        plot.Harvest();
        Debug.Log("Harvested FarmPlot_1. FP: " + ResourceSystem.Instance?.FocusPoints);
    }
}
