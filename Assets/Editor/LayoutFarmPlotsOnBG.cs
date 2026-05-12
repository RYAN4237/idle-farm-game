using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

/// Repositions all FarmPlot children onto the crop field area of the background image.
/// BG image world bounds: x 1.61~19.39, y -2.3~7.7 (camera center 10.5,2.7 orthoSize 5 aspect ~16:9)
/// Crop field is roughly centered at x~10.0, y~3.2 in the BG
public class LayoutFarmPlotsOnBG
{
    [MenuItem("Farm/Layout Farm Plots On BG")]
    public static void Execute()
    {
        var container = GameObject.Find("FarmPlots");
        if (container == null) { Debug.LogError("FarmPlots not found"); return; }

        var plots = new System.Collections.Generic.List<FarmPlot>();
        foreach (Transform t in container.transform)
        {
            var fp = t.GetComponent<FarmPlot>();
            if (fp != null) plots.Add(fp);
        }

        if (plots.Count == 0) { Debug.LogWarning("No FarmPlot children found"); return; }

        // Place plots in a grid on the crop field area of the BG
        // Crop area center: ~(10.0, 3.1), cell size 0.9, padding 0.05
        int cols = Mathf.CeilToInt(Mathf.Sqrt(plots.Count));
        int rows = Mathf.CeilToInt((float)plots.Count / cols);

        float cellSize  = 0.85f;
        float startX    = 10.0f - (cols - 1) * cellSize * 0.5f;
        float startY    = 3.1f  + (rows - 1) * cellSize * 0.5f;

        for (int i = 0; i < plots.Count; i++)
        {
            int col = i % cols;
            int row = i / cols;
            float x = startX + col * cellSize;
            float y = startY - row * cellSize;
            plots[i].transform.position = new Vector3(x, y, -0.1f);
            plots[i].transform.localScale = Vector3.one * cellSize * 0.9f;
            EditorUtility.SetDirty(plots[i].gameObject);
        }

        // Move Farmer to center of crop field
        var farmer = GameObject.Find("Farmer");
        if (farmer != null)
        {
            farmer.transform.position = new Vector3(10.0f, 3.1f, -0.15f);
            farmer.transform.localScale = Vector3.one * 0.55f;
            EditorUtility.SetDirty(farmer);
        }

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log($"[LayoutFarmPlotsOnBG] Placed {plots.Count} plots in {cols}x{rows} grid at crop field. Farmer repositioned.");
    }
}
