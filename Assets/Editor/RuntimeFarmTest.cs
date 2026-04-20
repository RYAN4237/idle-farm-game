using UnityEngine;
using UnityEditor;

/// Run this while game is PLAYING to test the full farm loop
public class RuntimeFarmTest
{
    public static void PlantAll()
    {
        for (int i = 1; i <= 3; i++)
        {
            var go   = GameObject.Find("FarmPlot_" + i);
            var plot = go?.GetComponent<FarmPlot>();
            if (plot != null && plot.State == FarmPlot.PlotState.Empty)
            {
                plot.Plant();
                Debug.Log($"FarmPlot_{i} planted. FP left: {ResourceSystem.Instance?.FocusPoints}");
            }
        }
    }

    public static void ForceReadyAll()
    {
        for (int i = 1; i <= 3; i++)
        {
            var go   = GameObject.Find("FarmPlot_" + i);
            var plot = go?.GetComponent<FarmPlot>();
            if (plot != null && plot.State == FarmPlot.PlotState.Growing)
            {
                plot.SetState(FarmPlot.PlotState.Ready);
                Debug.Log($"FarmPlot_{i} forced Ready.");
            }
        }
    }

    public static void HarvestAll()
    {
        for (int i = 1; i <= 3; i++)
        {
            var go   = GameObject.Find("FarmPlot_" + i);
            var plot = go?.GetComponent<FarmPlot>();
            if (plot != null && plot.State == FarmPlot.PlotState.Ready)
            {
                plot.Harvest();
                Debug.Log($"FarmPlot_{i} harvested. FP: {ResourceSystem.Instance?.FocusPoints}");
            }
        }
    }

    public static void PrintState()
    {
        Debug.Log($"FP: {ResourceSystem.Instance?.FocusPoints}");
        for (int i = 1; i <= 3; i++)
        {
            var go   = GameObject.Find("FarmPlot_" + i);
            var plot = go?.GetComponent<FarmPlot>();
            var sr   = go?.GetComponent<SpriteRenderer>();
            Debug.Log($"FarmPlot_{i} → State: {plot?.State}  Color: {sr?.color}");
        }
    }
}
