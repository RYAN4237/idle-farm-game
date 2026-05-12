using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class PositionPlotsOnGrass
{
    [MenuItem("Tools/Position Plots on Grass Area")]
    public static void Execute()
    {
        var container = GameObject.Find("FarmPlots");
        if (container == null) { Debug.LogError("FarmPlots not found"); return; }

        var plots = new System.Collections.Generic.List<Transform>();
        foreach (Transform t in container.transform)
            plots.Add(t);

        // Camera area: x=1.61~19.39, y=-2.3~7.7
        // Open grass between big oak (center-left) and apple tree (center-right)
        // Wheat BG area ~x=8~11, y=3~5.5 — place our tiles right on top of it
        // Scale=12.5 → each tile = 2.0u → use 2.0u cell spacing
        float scale  = 12.5f;
        float cellW  = 2.1f;
        float cellH  = 2.1f;
        float startX = 9.0f;   // Start center of wheat area
        float startY = 5.0f;   // Top row at y=5.0
        int   cols   = 3;

        for (int i = 0; i < plots.Count; i++)
        {
            int col = i % cols;
            int row = i / cols;
            float x = startX + col * cellW;
            float y = startY - row * cellH;
            plots[i].localPosition = new Vector3(x, y, -0.2f);
            plots[i].localScale    = Vector3.one * scale;
        }

        // Move farmer to left of plots
        var farmer = GameObject.Find("Farmer");
        if (farmer != null)
            farmer.transform.position = new Vector3(startX - 1.2f, startY - cellH * 0.5f, -0.3f);

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        Debug.Log($"Placed {plots.Count} plots at x:{startX}~{startX+(cols-1)*cellW:F1}, y:{startY}~{startY-(plots.Count/cols-1)*cellH:F1}");
    }
}
