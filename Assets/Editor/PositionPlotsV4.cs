using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class PositionPlotsV4
{
    [MenuItem("Tools/Position Plots V4 - Above River")]
    public static void Execute()
    {
        var container = GameObject.Find("FarmPlots");
        if (container == null) { Debug.LogError("FarmPlots not found"); return; }

        var plots = new System.Collections.Generic.List<Transform>();
        foreach (Transform t in container.transform)
            plots.Add(t);

        // Camera: x=1.61~19.39, y=-2.3~7.7
        // River top edge ~y=3.0, safe grass above ~y=3.5~6.0
        // BG wheat field center ~x=9~12, y=3.5~5.8
        // Place 3x2 grid at wheat area, top row y=5.5, bottom y=3.6 (just above river)
        float scale  = 12.5f;
        float cellW  = 2.1f;
        float cellH  = 1.9f;   // tighter vertical to fit above river
        float startX = 9.3f;
        float startY = 5.5f;
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

        var farmer = GameObject.Find("Farmer");
        if (farmer != null)
            farmer.transform.position = new Vector3(startX - 1.5f, startY - cellH * 0.5f, -0.3f);

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        int rows = Mathf.CeilToInt((float)plots.Count / cols);
        Debug.Log($"Plots: y top={startY}, y bottom={startY-(rows-1)*cellH:F1} (river ~y=3.0)");
    }
}
