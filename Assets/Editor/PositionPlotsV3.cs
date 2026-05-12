using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class PositionPlotsV3
{
    [MenuItem("Tools/Position Plots V3")]
    public static void Execute()
    {
        var container = GameObject.Find("FarmPlots");
        if (container == null) { Debug.LogError("FarmPlots not found"); return; }

        var plots = new System.Collections.Generic.List<Transform>();
        foreach (Transform t in container.transform)
            plots.Add(t);

        // Camera y: -2.3~7.7. River is at y~0.5~2.5
        // Keep all plots above y=3.0 to avoid river
        // Tight 3x2 grid in the center grass area
        float scale  = 12.5f;
        float cellW  = 2.1f;
        float cellH  = 2.1f;
        float startX = 9.2f;   // center, between oak & apple tree
        float startY = 5.3f;   // top row
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

        // Bottom row will be at y=5.3-2.1=3.2 → clear of river
        var farmer = GameObject.Find("Farmer");
        if (farmer != null)
            farmer.transform.position = new Vector3(startX - 1.5f, startY - cellH * 0.5f, -0.3f);

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        Debug.Log($"Plots: top row y={startY}, bottom row y={startY-(plots.Count/cols-1)*cellH:F1}");
    }
}
