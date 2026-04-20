using UnityEngine;
using UnityEditor;
using TMPro;

/// Final visual polish pass
public class VisualPolish
{
    public static void Execute()
    {
        // ── 1. Farm plots: warmer brown, bigger, slight shadow effect ──
        var emptyColor   = new Color(0.42f, 0.30f, 0.18f, 1f);   // warm brown
        var growingColor = new Color(0.22f, 0.62f, 0.22f, 1f);   // fresh green
        var readyColor   = new Color(0.35f, 0.95f, 0.35f, 1f);   // bright green

        for (int i = 1; i <= 3; i++)
        {
            var go = GameObject.Find("FarmPlot_" + i);
            if (go == null) continue;

            var plot = go.GetComponent<FarmPlot>();
            if (plot != null)
            {
                plot.emptyColor   = emptyColor;
                plot.growingColor = growingColor;
                plot.readyColor   = readyColor;
            }

            var sr = go.GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = emptyColor;

            // Label polish
            var label = go.transform.Find("Label");
            if (label != null)
            {
                var tmp = label.GetComponent<TextMeshPro>();
                if (tmp != null)
                {
                    tmp.color    = new Color(1f, 0.92f, 0.65f, 1f);  // warm cream
                    tmp.fontSize = 10f;
                }
            }

            EditorUtility.SetDirty(go);
        }

        // ── 2. RightPanel: slightly warmer dark tone ──
        var canvas     = GameObject.Find("UICanvas");
        var rightPanel = canvas?.transform.Find("RightPanel");
        if (rightPanel != null)
        {
            var img = rightPanel.GetComponent<UnityEngine.UI.Image>();
            if (img != null) img.color = new Color(0.11f, 0.13f, 0.16f, 0.96f);
            EditorUtility.SetDirty(rightPanel.gameObject);
        }

        // ── 3. GroundStrip: deeper grass green ──
        var ground = canvas?.transform.Find("GroundStrip");
        if (ground != null)
        {
            var img = ground.GetComponent<UnityEngine.UI.Image>();
            if (img != null) img.color = new Color(0.24f, 0.46f, 0.13f, 1f);
            EditorUtility.SetDirty(ground.gameObject);
        }

        // ── 4. Camera sky: slightly warmer blue ──
        var cam = Camera.main;
        if (cam != null)
        {
            cam.backgroundColor = new Color(0.50f, 0.78f, 0.95f, 1f);
            EditorUtility.SetDirty(cam.gameObject);
        }

        // ── 5. Save ──
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("VisualPolish complete + saved!");
    }
}
