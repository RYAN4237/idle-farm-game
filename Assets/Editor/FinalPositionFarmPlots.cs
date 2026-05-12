using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class FinalPositionFarmPlots
{
    [MenuItem("Tools/Final Position Farm Plots")]
    public static void Execute()
    {
        var container = GameObject.Find("FarmPlots");
        if (container == null) { Debug.LogError("FarmPlots not found"); return; }

        var plots = new System.Collections.Generic.List<Transform>();
        foreach (Transform t in container.transform)
            plots.Add(t);

        // BG image wheat field is roughly center-left of the visible camera area
        // Camera: x=1.61~19.39, y=-2.3~7.7
        // Wheat is at ~35-50% x, ~35-55% y of the BG
        // World: x ~ 7.5~10.5, y ~ 3.0~5.5
        // Scale up to 2.5 so each tile is 2.5 units wide
        float startX = 7.5f;
        float startY = 4.5f;
        float cellW  = 2.0f;
        float cellH  = 2.0f;
        int   cols   = 3;
        float scale  = 2.5f;

        for (int i = 0; i < plots.Count; i++)
        {
            int col = i % cols;
            int row = i / cols;
            float x = startX + col * cellW;
            float y = startY - row * cellH;
            plots[i].localPosition = new Vector3(x, y, -0.2f);
            plots[i].localScale    = Vector3.one * scale;
        }

        // Also move the Farmer to be near the plots
        var farmer = GameObject.Find("Farmer");
        if (farmer != null)
        {
            farmer.transform.position = new Vector3(startX + cellW, startY - cellH * 0.5f, -0.3f);
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        Debug.Log($"Plots placed at x:{startX}~{startX+(cols-1)*cellW}, y:{startY}~{startY-(plots.Count/cols-1)*cellH}, scale={scale}");
    }
}
