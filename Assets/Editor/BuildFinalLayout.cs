using UnityEngine;
using UnityEditor;
using TMPro;
using UnityEngine.UI;

/// Clean rebuild: Rusty's Retirement style
/// Left 73%: Sky + Grass + Farm plots
/// Right 27%: Dark panel with Timer + Buttons + FP stats
public class BuildFinalLayout
{
    public static void Execute()
    {
        var canvas = GameObject.Find("UICanvas");
        if (canvas == null) { Debug.LogError("UICanvas not found"); return; }

        // ── Step 1: Camera background = sky blue ──
        var cam = Camera.main;
        if (cam != null)
        {
            cam.clearFlags      = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.52f, 0.80f, 0.92f, 1f);
            EditorUtility.SetDirty(cam.gameObject);
        }

        // ── Step 2: Clean up old layout objects ──
        foreach (var name in new[]{"TopBackground","BackgroundPanel","FarmLabel","RightPanel","GroundStrip"})
            DestroyChild(canvas, name);

        // ── Step 3: Create GroundStrip (left 73%, bottom 30%) ──
        var groundGO  = MakeImage(canvas, "GroundStrip", new Color(0.28f, 0.50f, 0.16f, 1f), 1);
        SetStretch(groundGO, 0f, 0f, 0.73f, 0.30f);

        // ── Step 4: Create RightPanel (right 27%, full height) ──
        var rightGO  = MakeImage(canvas, "RightPanel", new Color(0.10f, 0.12f, 0.15f, 0.95f), 2);
        SetStretch(rightGO, 0.73f, 0f, 1f, 1f);

        // ── Step 5: Move Timer elements into RightPanel ──

        // CenterContainer → top of right panel, fixed square
        var center = canvas.transform.Find("CenterContainer");
        if (center != null)
        {
            center.SetParent(rightGO.transform, false);
            var r = center.GetComponent<RectTransform>();
            r.anchorMin        = new Vector2(0.5f, 1f);
            r.anchorMax        = new Vector2(0.5f, 1f);
            r.pivot            = new Vector2(0.5f, 1f);
            r.sizeDelta        = new Vector2(230f, 255f);
            r.anchoredPosition = new Vector2(0f, -8f);
            EditorUtility.SetDirty(center.gameObject);
        }

        // ButtonBar → just below timer
        var buttonBar = canvas.transform.Find("ButtonBar");
        if (buttonBar != null)
        {
            buttonBar.SetParent(rightGO.transform, false);
            var r = buttonBar.GetComponent<RectTransform>();
            r.anchorMin        = new Vector2(0.5f, 1f);
            r.anchorMax        = new Vector2(0.5f, 1f);
            r.pivot            = new Vector2(0.5f, 1f);
            r.sizeDelta        = new Vector2(220f, 52f);
            r.anchoredPosition = new Vector2(0f, -272f);
            EditorUtility.SetDirty(buttonBar.gameObject);

            // Fix button anchors inside ButtonBar
            FixButtonInBar(buttonBar.Find("StartPauseButton"), 0f, 0f, 0.56f, 1f);
            FixButtonInBar(buttonBar.Find("ResetButton"),      0.60f, 0f, 1f,   1f);
        }

        // TopRightPanel → bottom stats area
        var topRight = canvas.transform.Find("TopRightPanel");
        if (topRight != null)
        {
            topRight.SetParent(rightGO.transform, false);
            var r = topRight.GetComponent<RectTransform>();
            r.anchorMin        = new Vector2(0f, 0f);
            r.anchorMax        = new Vector2(1f, 0f);
            r.pivot            = new Vector2(0.5f, 0f);
            r.sizeDelta        = new Vector2(-12f, 155f);
            r.anchoredPosition = new Vector2(0f, 8f);
            EditorUtility.SetDirty(topRight.gameObject);

            // Reposition stats labels
            LayoutStat(topRight, "FPLabel",         0f,0.82f,1f,1.00f, 10f, new Color(0.55f,0.55f,0.55f,1f));
            LayoutStat(topRight, "FocusPointsText", 0f,0.52f,1f,0.84f, 30f, Color.white);
            LayoutStat(topRight, "IncomeRateText",  0f,0.28f,1f,0.54f, 13f, new Color(0.20f,0.85f,0.70f,1f));
            LayoutStat(topRight, "SessionCountText",0f,0.04f,1f,0.30f, 11f, new Color(0.45f,0.45f,0.45f,1f));
        }

        // CycleDots → just above buttons
        var cycleDots = canvas.transform.Find("CycleDots");
        if (cycleDots != null)
        {
            cycleDots.SetParent(rightGO.transform, false);
            var r = cycleDots.GetComponent<RectTransform>();
            r.anchorMin        = new Vector2(0.5f, 1f);
            r.anchorMax        = new Vector2(0.5f, 1f);
            r.pivot            = new Vector2(0.5f, 1f);
            r.sizeDelta        = new Vector2(150f, 22f);
            r.anchoredPosition = new Vector2(0f, -248f);
            EditorUtility.SetDirty(cycleDots.gameObject);
        }

        // PopupAnchor stays on main canvas, center-left
        var popup = canvas.transform.Find("PopupAnchor");
        if (popup != null)
        {
            var r = popup.GetComponent<RectTransform>();
            r.anchorMin        = new Vector2(0.365f, 0.5f);
            r.anchorMax        = new Vector2(0.365f, 0.5f);
            r.anchoredPosition = Vector2.zero;
            r.sizeDelta        = new Vector2(300f, 200f);
            EditorUtility.SetDirty(popup.gameObject);
        }

        // ── Step 6: Reposition Farm Plots ──
        // Camera ortho=5 → world ±5 height, ±8.89 width (16:9)
        // Left 73%: world x = -8.89 to +3.61, center x = -2.64
        // Grass top = 30% screen = world y = -5 + 10*0.30 = -2.0
        // Plot scale=20 → size=3.2 units. Bottom of plot at y=-4.0, top at -0.8 ✓
        float cx      = -2.64f;
        float spacing = 4.3f;
        var positions = new Vector3[]
        {
            new Vector3(cx - spacing, -2.4f, 0f),
            new Vector3(cx,           -2.4f, 0f),
            new Vector3(cx + spacing, -2.4f, 0f),
        };

        for (int i = 0; i < 3; i++)
        {
            var go = GameObject.Find("FarmPlot_" + (i + 1));
            if (go == null) continue;

            go.transform.position   = positions[i];
            go.transform.localScale = new Vector3(20f, 20f, 1f);

            var col = go.GetComponent<BoxCollider2D>();
            if (col != null) col.size = new Vector2(0.16f, 0.16f);

            // Label centered inside plot
            var label = go.transform.Find("Label");
            if (label != null)
            {
                label.localPosition = new Vector3(0f, 0.008f, -0.1f);
                label.localScale    = new Vector3(0.022f, 0.022f, 1f);
                var tmp = label.GetComponent<TextMeshPro>();
                if (tmp != null)
                {
                    tmp.fontSize            = 11f;
                    tmp.rectTransform.sizeDelta = new Vector2(6f, 4f);
                    tmp.color               = new Color(1f, 0.95f, 0.75f, 1f);
                    tmp.alignment           = TextAlignmentOptions.Center;
                }
                EditorUtility.SetDirty(label.gameObject);
            }

            // Progress bar at bottom of plot
            var barBg = go.transform.Find("ProgressBarBG");
            if (barBg != null)
            {
                barBg.localPosition = new Vector3(0f, -0.050f, -0.05f);
                barBg.localScale    = new Vector3(0.012f, 0.006f, 1f);
                EditorUtility.SetDirty(barBg.gameObject);
            }

            EditorUtility.SetDirty(go);
        }

        // ── Step 7: Save ──
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log("BuildFinalLayout complete + saved!");
    }

    // ── Helpers ──────────────────────────────────────────────────

    static GameObject MakeImage(GameObject canvasGO, string name, Color color, int siblingIndex)
    {
        var go  = new GameObject(name);
        go.transform.SetParent(canvasGO.transform, false);
        go.transform.SetSiblingIndex(siblingIndex);
        go.AddComponent<RectTransform>();
        var img        = go.AddComponent<Image>();
        img.color      = color;
        img.raycastTarget = false;
        return go;
    }

    static void SetStretch(GameObject go, float ax, float ay, float bx, float by)
    {
        var r = go.GetComponent<RectTransform>();
        r.anchorMin        = new Vector2(ax, ay);
        r.anchorMax        = new Vector2(bx, by);
        r.offsetMin        = Vector2.zero;
        r.offsetMax        = Vector2.zero;
        r.anchoredPosition = Vector2.zero;
        r.sizeDelta        = Vector2.zero;
        EditorUtility.SetDirty(go);
    }

    static void FixButtonInBar(Transform t, float ax, float ay, float bx, float by)
    {
        if (t == null) return;
        var r = t.GetComponent<RectTransform>();
        r.anchorMin        = new Vector2(ax, ay);
        r.anchorMax        = new Vector2(bx, by);
        r.offsetMin        = Vector2.zero;
        r.offsetMax        = Vector2.zero;
        r.anchoredPosition = Vector2.zero;
        r.sizeDelta        = Vector2.zero;
        EditorUtility.SetDirty(t.gameObject);
    }

    static void LayoutStat(Transform parent, string childName,
        float ax, float ay, float bx, float by, float fontSize, Color color)
    {
        var t = parent.Find(childName);
        if (t == null) return;
        var r = t.GetComponent<RectTransform>();
        r.anchorMin        = new Vector2(ax, ay);
        r.anchorMax        = new Vector2(bx, by);
        r.offsetMin        = Vector2.zero;
        r.offsetMax        = Vector2.zero;
        r.anchoredPosition = Vector2.zero;
        r.sizeDelta        = Vector2.zero;
        var tmp = t.GetComponent<TextMeshProUGUI>();
        if (tmp != null)
        {
            tmp.fontSize  = fontSize;
            tmp.color     = color;
            tmp.alignment = TextAlignmentOptions.Center;
        }
        EditorUtility.SetDirty(t.gameObject);
    }

    static void DestroyChild(GameObject parent, string name)
    {
        var t = parent.transform.Find(name);
        if (t != null) Object.DestroyImmediate(t.gameObject);
    }
}
