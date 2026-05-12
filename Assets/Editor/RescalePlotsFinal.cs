using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class RescalePlotsFinal
{
    [MenuItem("Tools/Rescale FarmPlots Final")]
    public static void Execute()
    {
        var container = GameObject.Find("FarmPlots");
        if (container == null) { Debug.LogError("FarmPlots not found"); return; }

        var plots = new System.Collections.Generic.List<Transform>();
        foreach (Transform t in container.transform)
            plots.Add(t);

        // Tilled_Dirt_0: 16px @ PPU=100 → 0.16 units
        // Target: each tile = 2.0 units → scale = 2.0 / 0.16 = 12.5
        // With 2.0u cell spacing, 3x2 grid = 4u x 2u
        float scale  = 12.5f;
        float startX = 7.8f;
        float startY = 4.5f;
        float cellW  = 2.2f;
        float cellH  = 2.2f;
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

        // Move farmer between the two rows
        var farmer = GameObject.Find("Farmer");
        if (farmer != null)
            farmer.transform.position = new Vector3(startX - 1.0f, startY - cellH * 0.5f, -0.3f);

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        Debug.Log($"Rescaled {plots.Count} plots to scale={scale}, each tile ~{0.16f*scale:F2}u, grid x:{startX}~{startX+(cols-1)*cellW}");
    }
}
