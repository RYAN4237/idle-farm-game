using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class TrimAndLayoutFarmPlots2
{
    public static void Execute()
    {
        var container = GameObject.Find("FarmPlots");
        if (container == null) { Debug.LogError("FarmPlots not found"); return; }

        var allChildren = new List<Transform>();
        foreach (Transform t in container.transform)
            allChildren.Add(t);

        Debug.Log($"[Trim] Found {allChildren.Count} FarmPlot children");

        int keep = 6;
        // Destroy extras immediately
        for (int i = keep; i < allChildren.Count; i++)
        {
            Object.DestroyImmediate(allChildren[i].gameObject);
        }

        // Re-collect after deletion
        var kept = new List<Transform>();
        foreach (Transform t in container.transform)
            kept.Add(t);

        Debug.Log($"[Trim] After deletion: {kept.Count} plots remain");

        // Place in 3x2 grid on crop field area of BG
        // BG center (10.5, 2.7), visible area x:1.61~19.39, y:-2.3~7.7
        // Crop field area ~(9.5~12.0, 3.0~4.0)
        int cols = 3;
        float cellSize = 0.78f;
        float startX = 9.5f;
        float startY = 3.9f;

        for (int i = 0; i < kept.Count; i++)
        {
            int col = i % cols;
            int row = i / cols;
            kept[i].position = new Vector3(startX + col * cellSize, startY - row * cellSize, -0.1f);
            kept[i].localScale = Vector3.one * 0.7f;
            EditorUtility.SetDirty(kept[i].gameObject);
        }

        var farmer = GameObject.Find("Farmer");
        if (farmer != null)
        {
            farmer.transform.position = new Vector3(10.5f, 3.5f, -0.15f);
            farmer.transform.localScale = Vector3.one * 0.45f;
            EditorUtility.SetDirty(farmer);
        }

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log($"[Trim] Done — {kept.Count} plots placed in 3x2 at crop field.");
    }
}
