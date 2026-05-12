using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class RescaleFarmPlots2
{
    public static void Execute()
    {
        var container = GameObject.Find("FarmPlots");
        if (container == null) { Debug.LogError("FarmPlots not found"); return; }

        var plots = new List<Transform>();
        foreach (Transform t in container.transform)
            plots.Add(t);

        // Place in 3x2 grid on the open grass area center-left of BG
        // BG visible: x 1.61~19.39, y -2.3~7.7. Camera center (10.5, 2.7)
        // Open grass center area: ~x 6~9, y 4~5.5
        int   cols   = 3;
        float cell   = 1.5f;
        float startX = 6.2f;
        float startY = 5.0f;

        for (int i = 0; i < plots.Count; i++)
        {
            int col = i % cols;
            int row = i / cols;
            plots[i].position   = new Vector3(startX + col * cell, startY - row * cell, -0.1f);
            plots[i].localScale = Vector3.one * 1.4f;
            EditorUtility.SetDirty(plots[i].gameObject);
        }

        // Farmer to center of plot grid
        var farmer = GameObject.Find("Farmer");
        if (farmer != null)
        {
            farmer.transform.position   = new Vector3(7.7f, 4.5f, -0.2f);
            farmer.transform.localScale = Vector3.one * 0.6f;
            EditorUtility.SetDirty(farmer);
        }

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log($"[Rescale2] Placed {plots.Count} plots. startX={startX} startY={startY} cell={cell}");
    }
}
