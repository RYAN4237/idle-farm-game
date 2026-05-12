using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

/// Keeps only 6 FarmPlots, deletes the rest, positions them on the crop field in the BG image.
public class TrimAndLayoutFarmPlots
{
    [MenuItem("Farm/Trim And Layout Farm Plots")]
    public static void Execute()
    {
        var container = GameObject.Find("FarmPlots");
        if (container == null) { Debug.LogError("FarmPlots not found"); return; }

        var allPlots = new System.Collections.Generic.List<Transform>();
        foreach (Transform t in container.transform)
            allPlots.Add(t);

        int keep = 6;
        // Delete extras
        for (int i = keep; i < allPlots.Count; i++)
            Undo.DestroyObjectImmediate(allPlots[i].gameObject);

        // Reposition the 6 kept plots in a 3x2 grid on the crop field
        // BG world bounds: x 1.61~19.39, y -2.3~7.7
        // Crop field center visible in BG ~(10.5, 3.5) - the open grassy area near the corn
        int cols = 3;
        float cellSize = 0.85f;
        float startX = 9.6f;   // left edge of 3-col grid
        float startY = 4.0f;   // top row y

        int idx = 0;
        foreach (Transform t in container.transform)
        {
            int col = idx % cols;
            int row = idx / cols;
            t.position = new Vector3(startX + col * cellSize, startY - row * cellSize, -0.1f);
            t.localScale = Vector3.one * cellSize * 0.88f;
            EditorUtility.SetDirty(t.gameObject);
            idx++;
        }

        // Move Farmer to center of plots
        var farmer = GameObject.Find("Farmer");
        if (farmer != null)
        {
            farmer.transform.position = new Vector3(10.5f, 3.5f, -0.15f);
            farmer.transform.localScale = Vector3.one * 0.5f;
            EditorUtility.SetDirty(farmer);
        }

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log($"[TrimAndLayout] Kept {Mathf.Min(keep, allPlots.Count)} plots in 3x2 grid.");
    }
}
