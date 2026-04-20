using UnityEngine;
using UnityEditor;
using TMPro;
using UnityEngine.UI;

public class FixTimerPanel
{
    public static void Execute()
    {
        var canvas     = GameObject.Find("UICanvas");
        var rightPanel = canvas?.transform.Find("RightPanel");
        var center     = rightPanel?.Find("CenterContainer");

        if (center == null) { Debug.LogError("CenterContainer not found"); return; }

        // ── CenterContainer: fixed square anchored at top-center ──
        var centerR = center.GetComponent<RectTransform>();
        centerR.anchorMin        = new Vector2(0.5f, 1f);
        centerR.anchorMax        = new Vector2(0.5f, 1f);
        centerR.pivot            = new Vector2(0.5f, 1f);
        centerR.sizeDelta        = new Vector2(260f, 260f);
        centerR.anchoredPosition = new Vector2(0f, -8f);
        EditorUtility.SetDirty(center.gameObject);

        // ── OuterRing: stretch to fill CenterContainer ──
        FixChild(center, "OuterRing",
            anchorMin: Vector2.zero, anchorMax: Vector2.one,
            pos: Vector2.zero, size: Vector2.zero);

        // ── ProgressRing: same as OuterRing ──
        FixChild(center, "ProgressRing",
            anchorMin: Vector2.zero, anchorMax: Vector2.one,
            pos: Vector2.zero, size: Vector2.zero);

        // ── StatusText: top-center inside ring ──
        FixChild(center, "StatusText",
            anchorMin: new Vector2(0.1f, 0.62f), anchorMax: new Vector2(0.9f, 0.82f),
            pos: Vector2.zero, size: Vector2.zero);

        // ── TimerText: center of ring ──
        FixChild(center, "TimerText",
            anchorMin: new Vector2(0.05f, 0.35f), anchorMax: new Vector2(0.95f, 0.65f),
            pos: Vector2.zero, size: Vector2.zero);

        // ── DurationLabel: bottom of ring ──
        FixChild(center, "DurationLabel",
            anchorMin: new Vector2(0.1f, 0.12f), anchorMax: new Vector2(0.9f, 0.30f),
            pos: Vector2.zero, size: Vector2.zero);

        // ── ButtonBar: right below CenterContainer ──
        var buttonBar = rightPanel?.Find("ButtonBar");
        if (buttonBar != null)
        {
            var r = buttonBar.GetComponent<RectTransform>();
            r.anchorMin        = new Vector2(0.5f, 1f);
            r.anchorMax        = new Vector2(0.5f, 1f);
            r.pivot            = new Vector2(0.5f, 1f);
            r.sizeDelta        = new Vector2(244f, 52f);
            r.anchoredPosition = new Vector2(0f, -278f);   // 8px gap below CenterContainer
            EditorUtility.SetDirty(buttonBar.gameObject);

            FixButtonInBar(buttonBar, "StartPauseButton", 0f,    0f, 0.56f, 1f);
            FixButtonInBar(buttonBar, "ResetButton",      0.60f, 0f, 1f,    1f);
        }

        // ── CycleDots: between timer and buttons ──
        var dots = rightPanel?.Find("CycleDots");
        if (dots != null)
        {
            var r = dots.GetComponent<RectTransform>();
            r.anchorMin        = new Vector2(0.5f, 1f);
            r.anchorMax        = new Vector2(0.5f, 1f);
            r.pivot            = new Vector2(0.5f, 1f);
            r.sizeDelta        = new Vector2(150f, 20f);
            r.anchoredPosition = new Vector2(0f, -256f);
            EditorUtility.SetDirty(dots.gameObject);
        }

        // ── TopRightPanel (Stats): bottom section ──
        var stats = rightPanel?.Find("TopRightPanel");
        if (stats != null)
        {
            var r = stats.GetComponent<RectTransform>();
            r.anchorMin        = new Vector2(0f, 0f);
            r.anchorMax        = new Vector2(1f, 0f);
            r.pivot            = new Vector2(0.5f, 0f);
            r.sizeDelta        = new Vector2(-12f, 165f);
            r.anchoredPosition = new Vector2(0f, 8f);
            EditorUtility.SetDirty(stats.gameObject);

            LayoutStat(stats, "FPLabel",          0f, 0.82f, 1f, 1.00f, 10f, new Color(0.55f,0.55f,0.55f,1f), "FOCUS POINTS");
            LayoutStat(stats, "FocusPointsText",   0f, 0.52f, 1f, 0.84f, 32f, Color.white, null);
            LayoutStat(stats, "IncomeRateText",    0f, 0.28f, 1f, 0.54f, 13f, new Color(0.20f,0.85f,0.70f,1f), null);
            LayoutStat(stats, "SessionCountText",  0f, 0.04f, 1f, 0.30f, 11f, new Color(0.45f,0.45f,0.45f,1f), null);
        }

        // ── Save ──
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("FixTimerPanel complete + saved!");
    }

    static void FixChild(Transform parent, string name,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pos, Vector2 size)
    {
        var t = parent.Find(name);
        if (t == null) { Debug.LogWarning("Child not found: " + name); return; }
        var r = t.GetComponent<RectTransform>();
        r.anchorMin        = anchorMin;
        r.anchorMax        = anchorMax;
        r.anchoredPosition = pos;
        r.sizeDelta        = size;
        r.localPosition    = new Vector3(r.localPosition.x, r.localPosition.y, 0f);
        EditorUtility.SetDirty(t.gameObject);
    }

    static void FixButtonInBar(Transform bar, string name,
        float ax, float ay, float bx, float by)
    {
        var t = bar.Find(name);
        if (t == null) return;
        var r = t.GetComponent<RectTransform>();
        r.anchorMin = new Vector2(ax, ay); r.anchorMax = new Vector2(bx, by);
        r.offsetMin = Vector2.zero;        r.offsetMax = Vector2.zero;
        r.anchoredPosition = Vector2.zero; r.sizeDelta = Vector2.zero;
        EditorUtility.SetDirty(t.gameObject);
    }

    static void LayoutStat(Transform parent, string name,
        float ax, float ay, float bx, float by,
        float fontSize, Color color, string overrideText)
    {
        var t = parent.Find(name);
        if (t == null) return;
        var r = t.GetComponent<RectTransform>();
        r.anchorMin = new Vector2(ax, ay); r.anchorMax = new Vector2(bx, by);
        r.offsetMin = Vector2.zero;        r.offsetMax = Vector2.zero;
        r.anchoredPosition = Vector2.zero; r.sizeDelta = Vector2.zero;
        var tmp = t.GetComponent<TextMeshProUGUI>();
        if (tmp != null)
        {
            tmp.fontSize  = fontSize;
            tmp.color     = color;
            tmp.alignment = TextAlignmentOptions.Center;
            if (overrideText != null) tmp.text = overrideText;
        }
        EditorUtility.SetDirty(t.gameObject);
    }
}
