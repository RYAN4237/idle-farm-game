using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class FinalPolish
{
    static Color C(string h) { ColorUtility.TryParseHtmlString(h, out Color c); return c; }

    public static void Execute()
    {
        var canvas = GameObject.Find("UICanvas");
        if (canvas == null) return;

        // ── 1. Fix IconBar buttons: transition=None, correct colors ───
        var bar = canvas.transform.Find("RightIconBar");
        if (bar != null)
        {
            var btnDefs = new (string name, string color)[]
            {
                ("SeedButton",    "#2a7010"),
                ("BuildButton",   "#7a4010"),
                ("UpgradeButton", "#103870"),
            };
            foreach (var (n, col) in btnDefs)
            {
                var btnGO = bar.Find(n)?.gameObject;
                if (btnGO == null) continue;
                var img = btnGO.GetComponent<Image>();
                if (img != null) { img.color = C(col); EditorUtility.SetDirty(btnGO); }
                var btn = btnGO.GetComponent<Button>();
                if (btn != null)
                {
                    btn.transition = Selectable.Transition.None;
                    EditorUtility.SetDirty(btnGO);
                }
            }
            // Bar background
            var barImg = bar.GetComponent<Image>();
            if (barImg != null) { barImg.color = C("#1a1006"); EditorUtility.SetDirty(bar.gameObject); }
            Debug.Log("IconBar buttons fixed: transition=None");
        }

        // ── 2. Fix TopBar FP display ──────────────────────────────────
        var topBar = canvas.transform.Find("TopBar");
        if (topBar != null)
        {
            // Make sure Value text is visible and properly sized
            var val = topBar.Find("Value");
            if (val != null)
            {
                var rt = val.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.18f,0); rt.anchorMax = new Vector2(0.4f,1);
                rt.offsetMin = rt.offsetMax = Vector2.zero;
                var tmp = val.GetComponentInChildren<TextMeshProUGUI>();
                if (tmp != null) { tmp.fontSize = 14; tmp.color = Color.white; EditorUtility.SetDirty(tmp.gameObject); }
                EditorUtility.SetDirty(val.gameObject);
            }
            var label = topBar.Find("FPDisplay");
            if (label != null)
            {
                var tmp = label.GetComponentInChildren<TextMeshProUGUI>();
                if (tmp != null) { tmp.text = "Focus Points:"; tmp.fontSize = 12; EditorUtility.SetDirty(tmp.gameObject); }
            }
            EditorUtility.SetDirty(topBar.gameObject);
        }

        // ── 3. Make sure ExpandablePanel uses correct slide animation ──
        var panel = canvas.transform.Find("ExpandablePanel");
        if (panel != null)
        {
            // Make sure it's hidden off-screen initially
            var rt = panel.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(205, 0); // hidden
            EditorUtility.SetDirty(panel.gameObject);
        }

        // ── 4. UIManager shown/hidden positions ───────────────────────
        // Already configured: shown=0, hidden=205

        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("[FinalPolish] Done!");
    }
}
