using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

/// Makes the timer look clean without needing a circular sprite.
/// OuterRing = dark rounded rect (no sprite needed, just color + corner radius via UI)
/// ProgressRing = hidden (no circular sprite available)
/// Timer numbers are the hero element
public class FixTimerVisuals
{
    public static void Execute()
    {
        var canvas = GameObject.Find("UICanvas");

        float BAR  = 0.26f;
        float LEFT = 0.20f;

        // ── OuterRing: dark bg that fills the timer section neatly ──
        var outerT = canvas.transform.Find("OuterRing");
        if (outerT != null)
        {
            // Remove AspectRatioFitter (causes issues with stretch)
            var arf = outerT.GetComponent<AspectRatioFitter>();
            if (arf != null) Object.DestroyImmediate(arf);

            var r = outerT.GetComponent<RectTransform>();
            // Fill the left bar area with small inset
            r.anchorMin        = new Vector2(0.005f, 0.01f);
            r.anchorMax        = new Vector2(LEFT - 0.005f, BAR - 0.01f);
            r.offsetMin        = Vector2.zero;
            r.offsetMax        = Vector2.zero;
            r.anchoredPosition = Vector2.zero;
            r.sizeDelta        = Vector2.zero;
            r.pivot            = new Vector2(0.5f, 0.5f);

            var img = outerT.GetComponent<Image>();
            if (img != null)
            {
                img.color         = new Color(0.12f, 0.14f, 0.18f, 1f);
                img.raycastTarget = false;
                img.type          = Image.Type.Simple;
            }
            EditorUtility.SetDirty(outerT.gameObject);
        }

        // ── ProgressRing: thin horizontal progress bar at bottom of timer section ──
        var progT = canvas.transform.Find("ProgressRing");
        if (progT != null)
        {
            var arf = progT.GetComponent<AspectRatioFitter>();
            if (arf != null) Object.DestroyImmediate(arf);

            var r = progT.GetComponent<RectTransform>();
            // Thin bar at the very bottom of the timer section
            r.anchorMin        = new Vector2(0.01f,  0.01f);
            r.anchorMax        = new Vector2(LEFT - 0.01f, 0.035f);
            r.offsetMin        = Vector2.zero;
            r.offsetMax        = Vector2.zero;
            r.anchoredPosition = Vector2.zero;
            r.sizeDelta        = Vector2.zero;
            r.pivot            = new Vector2(0.5f, 0.5f);

            var img = progT.GetComponent<Image>();
            if (img != null)
            {
                img.color         = new Color(0.20f, 0.85f, 0.70f, 1f);
                img.raycastTarget = false;
                img.type          = Image.Type.Filled;
                img.fillMethod    = Image.FillMethod.Horizontal;
                img.fillAmount    = 1f;
                img.fillOrigin    = 0;
            }
            EditorUtility.SetDirty(progT.gameObject);
        }

        // ── StatusText: "Focus" / "Break" — top of timer box ──
        FixText(canvas, "StatusText",
            0.01f, 0.74f, LEFT-0.01f, BAR-0.01f,
            11f, new Color(0.45f,0.48f,0.52f,1f));

        // ── TimerText: big bold "25:00" — center ──
        FixText(canvas, "TimerText",
            0.005f, BAR*0.36f, LEFT-0.005f, BAR*0.74f,
            32f, Color.white);

        // ── DurationLabel: "← 25 min →" ──
        FixText(canvas, "DurationLabel",
            0.01f, BAR*0.10f, LEFT-0.01f, BAR*0.32f,
            9f, new Color(0.40f,0.42f,0.46f,1f));

        // ── Wire UIManager.progressRing ──
        var uiMgr = canvas.GetComponent<UIManager>();
        if (uiMgr != null && progT != null)
        {
            uiMgr.progressRing = progT.GetComponent<Image>();
            EditorUtility.SetDirty(canvas);
        }

        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("FixTimerVisuals complete + saved!");
    }

    static void FixText(GameObject canvas, string name,
        float ax, float ay, float bx, float by,
        float fontSize, Color color)
    {
        var t = canvas.transform.Find(name);
        if (t == null) { Debug.LogWarning(name + " not found"); return; }

        var arf = t.GetComponent<AspectRatioFitter>();
        if (arf != null) Object.DestroyImmediate(arf);

        var r = t.GetComponent<RectTransform>();
        r.anchorMin        = new Vector2(ax, ay);
        r.anchorMax        = new Vector2(bx, by);
        r.offsetMin        = Vector2.zero;
        r.offsetMax        = Vector2.zero;
        r.anchoredPosition = Vector2.zero;
        r.sizeDelta        = Vector2.zero;
        r.pivot            = new Vector2(0.5f, 0.5f);

        var tmp = t.GetComponent<TextMeshProUGUI>();
        if (tmp != null)
        {
            tmp.fontSize         = fontSize;
            tmp.color            = color;
            tmp.alignment        = TextAlignmentOptions.Center;
            tmp.raycastTarget    = false;
            tmp.enableAutoSizing = false;
        }
        EditorUtility.SetDirty(t.gameObject);
    }
}
