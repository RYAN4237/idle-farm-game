using UnityEngine;
using UnityEditor;

public class DiagnoseRuntime
{
    public static void Execute()
    {
        Debug.Log("=== Runtime Diagnosis ===");

        // FocusSystem
        var fs = Object.FindObjectOfType<FocusSystem>();
        Debug.Log($"FocusSystem: {(fs != null ? $"found, running={fs.IsRunning}, time={fs.TimeRemaining:F0}s" : "NOT FOUND")}");

        // FocusSystem.Instance
        Debug.Log($"FocusSystem.Instance: {(FocusSystem.Instance != null ? "OK" : "null")}");

        // ResourceSystem
        var rs = Object.FindObjectOfType<ResourceSystem>();
        Debug.Log($"ResourceSystem: {(rs != null ? $"found, FP={rs.FocusPoints:F0}" : "NOT FOUND")}");

        // FarmBoostReceiver
        var fbr = Object.FindObjectOfType<FarmBoostReceiver>();
        Debug.Log($"FarmBoostReceiver: {(fbr != null ? $"found, mult={fbr.GrowthMultiplier:F1}" : "NOT FOUND")}");

        // FocusEventBridge
        var feb = Object.FindObjectOfType<FocusEventBridge>();
        Debug.Log($"FocusEventBridge: {(feb != null ? "found" : "NOT FOUND")}");

        // AmbientPerformance
        var ap = Object.FindObjectOfType<AmbientPerformance>();
        Debug.Log($"AmbientPerformance: {(ap != null ? "found" : "NOT FOUND")}");

        // PomoWidget
        var pw = Object.FindObjectOfType<PomoWidget>();
        Debug.Log($"PomoWidget: {(pw != null ? "found" : "NOT FOUND")}");

        // FarmGrid
        Debug.Log($"FarmGrid.Instance: {(FarmGrid.Instance != null ? $"cellSize={FarmGrid.Instance.cellSize}" : "null")}");

        Debug.Log("=== End Diagnosis ===");
    }
}
