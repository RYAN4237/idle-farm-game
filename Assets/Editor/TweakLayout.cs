using UnityEngine;
using UnityEditor;
using TMPro;
using UnityEngine.UI;

public class TweakLayout
{
    public static void Execute()
    {
        var canvas     = GameObject.Find("UICanvas");
        var rightPanel = canvas?.transform.Find("RightPanel")?.gameObject;
        if (rightPanel == null) { Debug.LogError("RightPanel not found"); return; }

        // ── 1. Widen right panel to 30% ──
        SetStretch(rightPanel, 0.70f, 0f, 1f, 1f);

        // Update ground strip to match
        var ground = canvas.transform.Find("GroundStrip");
        if (ground != null) SetStretch(ground.gameObject, 0f, 0f, 0.70f, 0.30f);

        // ── 2. Timer: anchor top, fixed 250×260 square ──
        var center = rightPanel.transform.Find("CenterContainer");
        if (center != null)
        {
            var r = center.GetComponent<RectTransform>();
            r.anchorMin        = new Vector2(0.5f, 1f);
            r.anchorMax        = new Vector2(0.5f, 1f);
            r.pivot            = new Vector2(0.5f, 1f);
            r.sizeDelta        = new Vector2(250f, 265f);
            r.anchoredPosition = new Vector2(0f, -10f);
            EditorUtility.SetDirty(center.gameObject);
        }

        // ── 3. Buttons: below timer ──
        var buttonBar = rightPanel.transform.Find("ButtonBar");
        if (buttonBar != null)
        {
            var r = buttonBar.GetComponent<RectTransform>();
            r.anchorMin        = new Vector2(0.5f, 1f);
            r.anchorMax        = new Vector2(0.5f, 1f);
            r.pivot            = new Vector2(0.5f, 1f);
            r.sizeDelta        = new Vector2(240f, 54f);
            r.anchoredPosition = new Vector2(0f, -285f);
            EditorUtility.SetDirty(buttonBar.gameObject);
        }

        // ── 4. CycleDots: just above buttons ──
        var dots = rightPanel.transform.Find("CycleDots");
        if (dots != null)
        {
            var r = dots.GetComponent<RectTransform>();
            r.anchorMin        = new Vector2(0.5f, 1f);
            r.anchorMax        = new Vector2(0.5f, 1f);
            r.pivot            = new Vector2(0.5f, 1f);
            r.sizeDelta        = new Vector2(160f, 22f);
            r.anchoredPosition = new Vector2(0f, -262f);
            EditorUtility.SetDirty(dots.gameObject);
        }

        // ── 5. Fix FarmPlot labels to show full text ──
        for (int i = 1; i <= 3; i++)
        {
            var go = GameObject.Find("FarmPlot_" + i);
            if (go == null) continue;

            var label = go.transform.Find("Label");
            if (label != null)
            {
                var tmp = label.GetComponent<TextMeshPro>();
                if (tmp != null)
                {
                    tmp.text                = "Plant\n(10 FP)";
                    tmp.fontSize            = 9f;
                    tmp.enableWordWrapping  = false;
                    tmp.textWrappingMode    = TMPro.TextWrappingModes.NoWrap;
                    tmp.overflowMode        = TMPro.TextOverflowModes.Overflow;
                    tmp.rectTransform.sizeDelta = new Vector2(8f, 5f);
                    label.localScale        = new Vector3(0.020f, 0.020f, 1f);
                    label.localPosition     = new Vector3(0f, 0.005f, -0.1f);
                }
                EditorUtility.SetDirty(label.gameObject);
            }
        }

        // ── 6. Reposition plots slightly left (panel is now 30%) ──
        // Left 70% world center x = (-8.89 + 3.34)/2 = -2.78
        float cx = -2.78f;
        float sp = 4.3f;
        var pos = new Vector3[]
        {
            new Vector3(cx - sp, -2.4f, 0f),
            new Vector3(cx,      -2.4f, 0f),
            new Vector3(cx + sp, -2.4f, 0f),
        };
        for (int i = 0; i < 3; i++)
        {
            var go = GameObject.Find("FarmPlot_" + (i + 1));
            if (go == null) continue;
            go.transform.position = pos[i];
            EditorUtility.SetDirty(go);
        }

        // ── 7. Save ──
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("TweakLayout complete + saved!");
    }

    static void SetStretch(GameObject go, float ax, float ay, float bx, float by)
    {
        var r = go.GetComponent<RectTransform>();
        r.anchorMin = new Vector2(ax, ay); r.anchorMax = new Vector2(bx, by);
        r.offsetMin = Vector2.zero;        r.offsetMax = Vector2.zero;
        r.anchoredPosition = Vector2.zero; r.sizeDelta = Vector2.zero;
        EditorUtility.SetDirty(go);
    }
}
