using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class FixTimerLayout
{
    public static void Execute()
    {
        var canvas = GameObject.Find("UICanvas");
        if (canvas == null) { Debug.LogError("UICanvas not found"); return; }

        float BAR  = 0.26f;
        float LEFT = 0.20f;

        // ── Helper: set stretch rect inside the left-bar zone ──
        // We convert sub-anchors (0-1 within left*bar zone) to canvas anchors

        // OuterRing: fill entire timer section (let AspectRatioFitter make it square)
        FixRing(canvas, "OuterRing",
            new Color(0.14f, 0.16f, 0.20f, 1f),
            0.04f, 0.03f, 0.96f, 0.97f, LEFT, BAR);

        // ProgressRing: same size, on top
        FixRing(canvas, "ProgressRing",
            new Color(0.20f, 0.85f, 0.70f, 0.95f),
            0.04f, 0.03f, 0.96f, 0.97f, LEFT, BAR);
        var progImg = canvas.transform.Find("ProgressRing")?.GetComponent<Image>();
        if (progImg != null)
        {
            progImg.type       = Image.Type.Filled;
            progImg.fillMethod = Image.FillMethod.Radial360;
            progImg.fillAmount = 1f;
            progImg.fillOrigin = 2;
            progImg.raycastTarget = false;
            EditorUtility.SetDirty(progImg.gameObject);
        }

        // StatusText: top third of ring
        FixText(canvas, "StatusText", 0.10f, 0.64f, 0.90f, 0.80f, LEFT, BAR, 11f, new Color(0.6f,0.65f,0.7f,1f));
        // TimerText: middle
        FixText(canvas, "TimerText",  0.05f, 0.36f, 0.95f, 0.64f, LEFT, BAR, 30f, Color.white);
        // DurationLabel: lower portion
        FixText(canvas, "DurationLabel", 0.08f, 0.13f, 0.92f, 0.30f, LEFT, BAR, 9f, new Color(0.45f,0.45f,0.50f,1f));

        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("FixTimerLayout complete + saved!");
    }

    static void FixRing(GameObject canvas, string name, Color color,
        float ax, float ay, float bx, float by, float LEFT, float BAR)
    {
        var t = canvas.transform.Find(name);
        if (t == null) { Debug.LogWarning(name + " not found"); return; }

        var r = t.GetComponent<RectTransform>();
        // Anchor inside [0..LEFT] x [0..BAR] zone with sub-fraction padding
        r.anchorMin        = new Vector2(ax * LEFT,  ay * BAR);
        r.anchorMax        = new Vector2(bx * LEFT,  by * BAR);
        r.offsetMin        = Vector2.zero;
        r.offsetMax        = Vector2.zero;
        r.anchoredPosition = Vector2.zero;
        r.sizeDelta        = Vector2.zero;
        r.pivot            = new Vector2(0.5f, 0.5f);

        // Add AspectRatioFitter to keep it square
        var arf = t.GetComponent<AspectRatioFitter>() ?? t.gameObject.AddComponent<AspectRatioFitter>();
        arf.aspectMode  = AspectRatioFitter.AspectMode.HeightControlsWidth;
        arf.aspectRatio = 1f;

        var img = t.GetComponent<Image>();
        if (img != null) { img.color = color; img.raycastTarget = false; }

        EditorUtility.SetDirty(t.gameObject);
    }

    static void FixText(GameObject canvas, string name,
        float ax, float ay, float bx, float by,
        float LEFT, float BAR, float fontSize, Color color)
    {
        var t = canvas.transform.Find(name);
        if (t == null) { Debug.LogWarning(name + " not found"); return; }

        var r = t.GetComponent<RectTransform>();
        r.anchorMin        = new Vector2(ax * LEFT, ay * BAR);
        r.anchorMax        = new Vector2(bx * LEFT, by * BAR);
        r.offsetMin        = Vector2.zero;
        r.offsetMax        = Vector2.zero;
        r.anchoredPosition = Vector2.zero;
        r.sizeDelta        = Vector2.zero;
        r.pivot            = new Vector2(0.5f, 0.5f);

        var tmp = t.GetComponent<TextMeshProUGUI>();
        if (tmp != null)
        {
            tmp.fontSize        = fontSize;
            tmp.color           = color;
            tmp.alignment       = TextAlignmentOptions.Center;
            tmp.raycastTarget   = false;
            tmp.enableAutoSizing = false;
        }

        EditorUtility.SetDirty(t.gameObject);
    }
}
