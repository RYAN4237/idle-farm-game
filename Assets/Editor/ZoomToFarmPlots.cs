using UnityEngine;
using UnityEditor;

public class ZoomToFarmPlots
{
    [MenuItem("Tools/Zoom Scene to FarmPlot_0")]
    public static void Execute()
    {
        var container = GameObject.Find("FarmPlots");
        if (container == null) { Debug.LogError("FarmPlots not found"); return; }

        Transform first = null;
        foreach (Transform t in container.transform) { first = t; break; }
        if (first == null) { Debug.LogError("No child"); return; }

        // Move scene view camera to look at first plot close up
        var sv = UnityEditor.SceneView.lastActiveSceneView;
        if (sv != null)
        {
            sv.LookAt(first.position, Quaternion.identity, 4f);
            sv.Repaint();
        }

        Debug.Log($"Zoomed to FarmPlot at {first.position}, scale={first.localScale}");
    }
}
