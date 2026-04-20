using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

/// Final visual polish pass
public class FinalPolishPass
{
    static Color C(string h) { ColorUtility.TryParseHtmlString(h, out Color c); return c; }

    public static void Execute()
    {
        var canvas = GameObject.Find("UICanvas");
        if (canvas == null) { Debug.LogError("UICanvas null"); return; }

        // ── 1. TopBar: make FP display bigger and clearer ─────────────
        var topBar = canvas.transform.Find("TopBar");
        if (topBar != null)
        {
            // Change label text
            var fpDisplay = topBar.Find("FPDisplay");
            if (fpDisplay != null)
            {
                var t = fpDisplay.GetComponentInChildren<TextMeshProUGUI>();
                if (t != null) { t.text = "🌿 Focus Points:"; t.fontSize = 11; }
            }
            // Make value more prominent
            var val = topBar.Find("Value");
            if (val != null)
            {
                var t = val.GetComponentInChildren<TextMeshProUGUI>();
                if (t != null)
                {
                    t.fontSize = 13;
                    t.fontStyle = FontStyles.Bold;
                    t.color = new Color(1f, 0.95f, 0.5f);
                }
            }
            EditorUtility.SetDirty(topBar.gameObject);
        }

        // ── 2. ExpandablePanel title font ─────────────────────────────
        var title = canvas.transform.Find("ExpandablePanel/TitleBar/TitleText");
        if (title != null)
        {
            var t = title.GetComponent<TextMeshProUGUI>();
            if (t != null) { t.fontSize = 12; t.fontStyle = FontStyles.Bold; }
            EditorUtility.SetDirty(title.gameObject);
        }

        // ── 3. PomoWidget: slightly smaller, more polished ─────────────
        var pomoWidget = GameObject.Find("PomoCanvas/PomoWidget");
        if (pomoWidget != null)
        {
            var rt = pomoWidget.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(148, 188);
            rt.anchoredPosition = new Vector2(4, -30);

            // Header color — more subdued green
            var header = pomoWidget.transform.Find("Header");
            if (header != null)
            {
                var img = header.GetComponent<Image>();
                if (img != null) img.color = C("#1e4a14");
            }

            // Phase label font
            var phase = pomoWidget.transform.Find("Header/Phase/T");
            if (phase != null)
            {
                var t = phase.GetComponent<TextMeshProUGUI>();
                if (t != null) { t.fontSize = 9; t.fontStyle = FontStyles.Bold; t.color = C("#90e060"); }
            }

            // Timer text — bigger
            var timer = pomoWidget.transform.Find("Body/TimerTxt/T");
            if (timer != null)
            {
                var t = timer.GetComponent<TextMeshProUGUI>();
                if (t != null) { t.fontSize = 22; t.fontStyle = FontStyles.Bold; }
            }

            // FP label
            var fp = pomoWidget.transform.Find("Body/FP/T");
            if (fp != null)
            {
                var t = fp.GetComponent<TextMeshProUGUI>();
                if (t != null) { t.fontSize = 8; t.color = C("#78c840"); }
            }

            EditorUtility.SetDirty(pomoWidget);
        }

        // ── 4. FarmMapScroller: wider scroll range ─────────────────────
        var cam = GameObject.Find("Main Camera");
        if (cam != null)
        {
            var scroller = cam.GetComponent<FarmMapScroller>();
            if (scroller == null) scroller = GameObject.Find("GameManager")?.GetComponent<FarmMapScroller>();
            if (scroller != null)
            {
                scroller.mapMinX = -13f;
                scroller.mapMaxX =  13f;
                scroller.scrollSpeed = 12f;
                EditorUtility.SetDirty(scroller.gameObject);
            }
        }

        // ── 5. Camera: slightly lower ortho for better farm view ───────
        if (cam != null)
        {
            var camera = cam.GetComponent<Camera>();
            if (camera != null)
            {
                camera.orthographicSize = 2.8f;
                EditorUtility.SetDirty(cam);
            }
        }

        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("[FinalPolishPass] Done!");
    }
}
