using UnityEngine;
using UnityEditor;

public class FocusSceneView
{
    [MenuItem("Farm/Focus Scene View on Scene")]
    public static void Execute()
    {
        var sv = SceneView.lastActiveSceneView;
        if (sv == null) { Debug.LogWarning("No active SceneView"); return; }

        sv.in2DMode = true;
        sv.pivot = new Vector3(0, 0, 0);
        sv.size = 6f; // ortho size ~= camera size 5, slight margin
        sv.Repaint();

        Debug.Log("Scene View focused on origin, size=6");
    }
}
