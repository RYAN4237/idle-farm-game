using UnityEngine;
using UnityEditor;
using TMPro;

public class FixLabelPositions
{
    public static void Execute()
    {
        // FarmPlot scale = 12, sprite native = 0.16 units
        // World size of plot = 12 × 0.16 = 1.92 units
        // Local space of plot: 1 unit = 1/12 world unit = 0.083 world units
        // To position label at y = +0.5 world units above plot center:
        //   localPosition.y = 0.5 / 12 = 0.042
        // To position label inside plot center: localPosition.y = 0

        for (int i = 1; i <= 3; i++)
        {
            var go = GameObject.Find("FarmPlot_" + i);
            if (go == null) continue;

            // ── Label: centered inside the plot ──
            var label = go.transform.Find("Label");
            if (label != null)
            {
                // Place label at vertical center of plot
                label.localPosition = new Vector3(0f, 0f, -0.1f);
                // Scale so text renders at ~0.5 world units tall
                // TMP fontSize=12, at scale 0.038: 12 × 0.038 = 0.46 world units ✓
                label.localScale = new Vector3(0.038f, 0.038f, 1f);

                var tmp = label.GetComponent<TextMeshPro>();
                if (tmp != null)
                {
                    tmp.fontSize               = 12f;
                    tmp.rectTransform.sizeDelta = new Vector2(3f, 2.5f);
                    tmp.color                   = new Color(1f, 1f, 0.85f, 1f);
                    tmp.enableWordWrapping       = false;
                    tmp.textWrappingMode         = TMPro.TextWrappingModes.Normal;
                    tmp.overflowMode             = TMPro.TextOverflowModes.Overflow;
                }
                EditorUtility.SetDirty(label.gameObject);
            }

            // ── Progress bar: at bottom edge of plot ──
            // Bottom of plot in local space = -0.08 (half of 0.16 native sprite)
            var barBg = go.transform.Find("ProgressBarBG");
            if (barBg != null)
            {
                barBg.localPosition = new Vector3(0f, -0.06f, -0.05f);
                barBg.localScale    = new Vector3(0.013f, 0.006f, 1f);
                EditorUtility.SetDirty(barBg.gameObject);
            }

            // ── Plot positions: spread out more, lower on screen ──
            EditorUtility.SetDirty(go);
        }

        // Move all 3 plots to final positions
        var positions = new Vector3[]
        {
            new Vector3(-3.0f, -3.5f, 0f),
            new Vector3( 0.0f, -3.5f, 0f),
            new Vector3( 3.0f, -3.5f, 0f),
        };

        for (int i = 0; i < 3; i++)
        {
            var go = GameObject.Find("FarmPlot_" + (i + 1));
            if (go == null) continue;
            go.transform.position = positions[i];
            EditorUtility.SetDirty(go);
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("FixLabelPositions complete! Labels inside plots, plots at y=-3.5");
    }
}
