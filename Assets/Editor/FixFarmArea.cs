using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class FixFarmArea
{
    public static void Execute()
    {
        var canvas     = GameObject.Find("UICanvas");
        var rightPanel = canvas?.transform.Find("RightPanel");

        // ── 1. Remove the black BackgroundPanel that covers the farm area ──
        var bp = canvas.transform.Find("BackgroundPanel");
        if (bp != null)
        {
            Object.DestroyImmediate(bp.gameObject);
            Debug.Log("Removed BackgroundPanel.");
        }

        // Also remove TopBackground if present
        var tp = canvas.transform.Find("TopBackground");
        if (tp != null) Object.DestroyImmediate(tp.gameObject);

        // ── 2. GroundStrip: only a thin strip at the very bottom (10%) ──
        // Farm plots sit on grass visually but grass shouldn't cover them
        var ground = canvas.transform.Find("GroundStrip");
        if (ground != null)
        {
            var r = ground.GetComponent<RectTransform>();
            r.anchorMin = new Vector2(0f, 0f);
            r.anchorMax = new Vector2(0.70f, 0.14f); // thin strip
            r.offsetMin = Vector2.zero;
            r.offsetMax = Vector2.zero;
            r.anchoredPosition = Vector2.zero;
            r.sizeDelta        = Vector2.zero;

            var img = ground.GetComponent<Image>();
            if (img != null) img.color = new Color(0.28f, 0.50f, 0.16f, 1f);
            ground.SetSiblingIndex(0); // behind everything
            EditorUtility.SetDirty(ground.gameObject);
        }

        // ── 3. Fix Start Focus button (it disappeared from view) ──
        var buttonBar = rightPanel?.Find("ButtonBar");
        if (buttonBar != null)
        {
            // Find StartPauseButton and ensure it has correct anchors
            var startBtn = buttonBar.Find("StartPauseButton");
            if (startBtn != null)
            {
                var r = startBtn.GetComponent<RectTransform>();
                r.anchorMin = new Vector2(0f, 0f);
                r.anchorMax = new Vector2(0.56f, 1f);
                r.offsetMin = Vector2.zero;
                r.offsetMax = Vector2.zero;
                r.anchoredPosition = Vector2.zero;
                r.sizeDelta        = Vector2.zero;

                // Ensure button text is visible
                var btnText = startBtn.GetComponentInChildren<TextMeshProUGUI>();
                if (btnText != null)
                {
                    btnText.fontSize = 14f;
                    btnText.color    = Color.white;
                }
                EditorUtility.SetDirty(startBtn.gameObject);
            }

            var resetBtn = buttonBar.Find("ResetButton");
            if (resetBtn != null)
            {
                var r = resetBtn.GetComponent<RectTransform>();
                r.anchorMin = new Vector2(0.60f, 0f);
                r.anchorMax = new Vector2(1f, 1f);
                r.offsetMin = Vector2.zero;
                r.offsetMax = Vector2.zero;
                r.anchoredPosition = Vector2.zero;
                r.sizeDelta        = Vector2.zero;
                EditorUtility.SetDirty(resetBtn.gameObject);
            }
        }

        // ── 4. Reposition Farm Plots ──
        // Camera ortho=5, 16:9 → world y range -5 to +5
        // Left 70% world x: -8.89 to +3.34, center = -2.78
        // Place plots at y=-1.5 so they float above the thin grass strip
        float cx = -2.78f;
        float sp = 4.2f;
        var plotPos = new Vector3[]
        {
            new Vector3(cx - sp, -1.5f, 0f),
            new Vector3(cx,      -1.5f, 0f),
            new Vector3(cx + sp, -1.5f, 0f),
        };

        for (int i = 0; i < 3; i++)
        {
            var go = GameObject.Find("FarmPlot_" + (i + 1));
            if (go == null) continue;

            go.transform.position   = plotPos[i];
            go.transform.localScale = new Vector3(20f, 20f, 1f);

            // Fix label
            var label = go.transform.Find("Label");
            if (label != null)
            {
                label.localPosition = new Vector3(0f, 0.006f, -0.1f);
                label.localScale    = new Vector3(0.022f, 0.022f, 1f);
                var tmp = label.GetComponent<TextMeshPro>();
                if (tmp != null)
                {
                    tmp.text      = "Plant\n(10 FP)";
                    tmp.fontSize  = 10f;
                    tmp.color     = new Color(1f, 0.95f, 0.75f, 1f);
                    tmp.alignment = TextAlignmentOptions.Center;
                    tmp.enableWordWrapping  = false;
                    tmp.textWrappingMode    = TMPro.TextWrappingModes.NoWrap;
                    tmp.overflowMode        = TMPro.TextOverflowModes.Overflow;
                    tmp.rectTransform.sizeDelta = new Vector2(8f, 5f);
                }
                EditorUtility.SetDirty(label.gameObject);
            }
            EditorUtility.SetDirty(go);
        }

        // ── 5. Save ──
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("FixFarmArea complete + saved!");
    }
}
