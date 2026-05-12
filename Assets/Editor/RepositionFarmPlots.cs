using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class RepositionFarmPlots
{
    [MenuItem("Tools/Reposition Farm Plots to Wheat Field")]
    public static void Execute()
    {
        var container = GameObject.Find("FarmPlots");
        if (container == null) { Debug.LogError("FarmPlots not found"); return; }

        var plots = new System.Collections.Generic.List<Transform>();
        foreach (Transform t in container.transform)
            plots.Add(t);

        if (plots.Count == 0) { Debug.LogError("No child plots found"); return; }

        // Wheat field area in the background image ~ center-left area
        // BG world: x:1.61~19.39, y:-2.3~7.7 (camera at 10.5, 2.7, size=5)
        // Wheat patch visible at roughly x:9~12, y:4~6
        float startX = 9.5f;
        float startY = 5.5f;
        float cellW  = 1.5f;
        float cellH  = 1.5f;
        int cols = 3;

        for (int i = 0; i < plots.Count; i++)
        {
            int col = i % cols;
            int row = i / cols;
            float x = startX + col * cellW;
            float y = startY - row * cellH;
            plots[i].localPosition = new Vector3(x, y, -0.1f);
            plots[i].localScale = Vector3.one * 1.4f;
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        Debug.Log($"Repositioned {plots.Count} plots. Grid at x:{startX}~{startX+(cols-1)*cellW}, y:{startY}~{startY-(plots.Count/cols-1)*cellH}");
    }
}
