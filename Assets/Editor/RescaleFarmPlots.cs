using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class RescaleFarmPlots
{
    public static void Execute()
    {
        var container = GameObject.Find("FarmPlots");
        if (container == null) { Debug.LogError("FarmPlots not found"); return; }

        var plots = new List<Transform>();
        foreach (Transform t in container.transform)
            plots.Add(t);

        // Place in 3x2 grid on the open grassy area right of the corn
        // Background image: corn is ~x10~11, y4~5
        // Place plots slightly right of farmer at x11.5~14, y4.5~3.5
        int   cols   = 3;
        float cell   = 1.4f;   // 1.4 units spacing
        float startX = 11.4f;
        float startY = 4.5f;

        for (int i = 0; i < plots.Count; i++)
        {
            int col = i % cols;
            int row = i / cols;
            plots[i].position   = new Vector3(startX + col * cell, startY - row * cell, -0.1f);
            plots[i].localScale = Vector3.one * 1.3f;   // visible size
            EditorUtility.SetDirty(plots[i].gameObject);
        }

        // Move Farmer to center of plots
        var farmer = GameObject.Find("Farmer");
        if (farmer != null)
        {
            farmer.transform.position   = new Vector3(12.5f, 4.0f, -0.2f);
            farmer.transform.localScale = Vector3.one * 0.9f;
            EditorUtility.SetDirty(farmer);
        }

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log($"[Rescale] {plots.Count} FarmPlots repositioned. Scale=1.3, cell=1.4u");
    }
}
