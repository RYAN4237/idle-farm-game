using UnityEngine;
using UnityEditor;
using TMPro;

public class LayoutFix3
{
    public static void Execute()
    {
        var canvas = GameObject.Find("UICanvas");

        // ── 1. TopBackground: cover top 55% leaving more space for Farm ──
        var topBG = canvas?.transform.Find("TopBackground");
        if (topBG != null)
        {
            var r = topBG.GetComponent<RectTransform>();
            r.anchorMin = new Vector2(0f, 0.45f);
            r.anchorMax = new Vector2(1f, 1.0f);
            r.offsetMin = Vector2.zero;
            r.offsetMax = Vector2.zero;
            EditorUtility.SetDirty(topBG.gameObject);
        }

        // ── 2. ButtonBar: anchor at 0.45 of screen, move up a bit ──
        var buttonBar = canvas?.transform.Find("ButtonBar");
        if (buttonBar != null)
        {
            var r = buttonBar.GetComponent<RectTransform>();
            r.anchorMin = new Vector2(0.5f, 0.45f);
            r.anchorMax = new Vector2(0.5f, 0.45f);
            r.anchoredPosition = new Vector2(0f, 30f);
            EditorUtility.SetDirty(buttonBar.gameObject);
        }

        // ── 3. CycleDots: just above ButtonBar ──
        var cycleDots = canvas?.transform.Find("CycleDots");
        if (cycleDots != null)
        {
            var r = cycleDots.GetComponent<RectTransform>();
            r.anchorMin = new Vector2(0.5f, 0.45f);
            r.anchorMax = new Vector2(0.5f, 0.45f);
            r.anchoredPosition = new Vector2(0f, 90f);
            EditorUtility.SetDirty(cycleDots.gameObject);
        }

        // ── 4. FarmPlots: lower to y=-3.2, bigger scale ──
        // Screen bottom = world y=-5, top of farm zone = world y=-1 (approx)
        // Place 3 plots spread across bottom, scale 12 = 1.92 world units each
        Vector3[] positions = new Vector3[]
        {
            new Vector3(-3.0f, -3.3f, 0f),
            new Vector3( 0.0f, -3.3f, 0f),
            new Vector3( 3.0f, -3.3f, 0f),
        };

        for (int i = 0; i < 3; i++)
        {
            var go = GameObject.Find("FarmPlot_" + (i + 1));
            if (go == null) continue;

            go.transform.position   = positions[i];
            go.transform.localScale = new Vector3(12f, 12f, 1f);

            // Fix collider size to match sprite (0.16 native)
            var col = go.GetComponent<BoxCollider2D>();
            if (col != null) col.size = new Vector2(0.16f, 0.16f);

            // Fix label position and scale for new parent scale
            var label = go.transform.Find("Label");
            if (label != null)
            {
                label.localPosition = new Vector3(0f, 0.11f, -0.1f);
                label.localScale    = new Vector3(0.042f, 0.042f, 1f);
                var tmp = label.GetComponent<TextMeshPro>();
                if (tmp != null)
                {
                    tmp.fontSize = 14f;
                    tmp.rectTransform.sizeDelta = new Vector2(4f, 2f);
                }
                EditorUtility.SetDirty(label.gameObject);
            }

            // Fix progress bar position
            var barBg = go.transform.Find("ProgressBarBG");
            if (barBg != null)
            {
                barBg.localPosition = new Vector3(0f, -0.065f, -0.05f);
                barBg.localScale    = new Vector3(0.012f, 0.007f, 1f);
                EditorUtility.SetDirty(barBg.gameObject);
            }

            EditorUtility.SetDirty(go);
        }

        // ── 5. FarmLabel at very bottom ──
        var farmLabel = canvas?.transform.Find("FarmLabel");
        if (farmLabel != null)
        {
            var r = farmLabel.GetComponent<RectTransform>();
            r.anchorMin = new Vector2(0.5f, 0f);
            r.anchorMax = new Vector2(0.5f, 0f);
            r.anchoredPosition = new Vector2(0f, 12f);
            EditorUtility.SetDirty(farmLabel.gameObject);
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("LayoutFix3 complete!");
    }
}
