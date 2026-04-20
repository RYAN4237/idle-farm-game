using UnityEngine;
using UnityEditor;
using TMPro;

public class FixPlotScale
{
    public static void Execute()
    {
        // UISprite is 100px at 100 PPU = 1 unit. But the built-in UISprite
        // is actually stored at a different size. Let's measure and set scale
        // so each plot is exactly 1.6 world units square.
        // Target world size = 1.6 units
        // We set scale = 10 so that sprite (0.16 native) × 10 = 1.6 ✓

        for (int i = 1; i <= 3; i++)
        {
            var plotGO = GameObject.Find("FarmPlot_" + i);
            if (plotGO == null) continue;

            // Plot: scale 10 → 0.16 × 10 = 1.6 world units
            plotGO.transform.localScale = new Vector3(10f, 10f, 1f);

            // Collider: world-space 1×1 but in local space = 1/10 = 0.1
            // Actually BoxCollider2D uses localScale automatically for size
            // Set size to 1 in local space → 10 × 1 = 10... no.
            // BoxCollider2D size IS in local space. So size=0.16 × scale=10 → 1.6 ✓
            var col = plotGO.GetComponent<BoxCollider2D>();
            if (col != null) col.size = new Vector2(0.16f, 0.16f); // matches sprite

            // Label: compensate for parent scale, so it appears normal size
            var label = plotGO.transform.Find("Label");
            if (label != null)
            {
                // 0.1/10 = 0.01 per unit at parent scale 10
                label.localScale    = new Vector3(0.05f, 0.05f, 1f);
                label.localPosition = new Vector3(0f, 0.13f, -0.1f);
                var tmp = label.GetComponent<TextMeshPro>();
                if (tmp != null)
                {
                    tmp.fontSize = 14f;
                    tmp.rectTransform.sizeDelta = new Vector2(4f, 2f);
                }
            }

            // ProgressBarBG: thin strip at bottom
            var barBg = plotGO.transform.Find("ProgressBarBG");
            if (barBg != null)
            {
                barBg.localScale    = new Vector3(0.013f, 0.008f, 1f);
                barBg.localPosition = new Vector3(0f, -0.07f, -0.05f);

                var barFill = barBg.Find("ProgressBarFill");
                if (barFill != null)
                {
                    barFill.localScale    = new Vector3(0.001f, 1f, 1f);
                    barFill.localPosition = new Vector3(-0.5f, 0f, -0.05f);
                }
            }

            EditorUtility.SetDirty(plotGO);
            Debug.Log($"FarmPlot_{i}: scale=10, collider=0.16 → world size 1.6");
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("FixPlotScale complete!");
    }
}
