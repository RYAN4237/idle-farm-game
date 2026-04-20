using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class FinalSceneWire
{
    public static void Execute()
    {
        var canvas = GameObject.Find("UICanvas");
        if (canvas == null) { Debug.LogError("UICanvas null"); return; }

        // ── 1. Wire TopBarFP script ───────────────────────────────────
        var topBar = canvas.transform.Find("TopBar");
        if (topBar != null)
        {
            var fpScript = topBar.GetComponent<TopBarFP>() ?? topBar.gameObject.AddComponent<TopBarFP>();
            // Find Value text
            var valGO = topBar.Find("Value");
            if (valGO != null)
            {
                var tmp = valGO.GetComponentInChildren<TextMeshProUGUI>();
                if (tmp == null)
                {
                    // Create it
                    var t = new GameObject("Text");
                    t.transform.SetParent(valGO, false);
                    var rt = t.AddComponent<RectTransform>();
                    rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
                    rt.offsetMin = rt.offsetMax = Vector2.zero;
                    tmp = t.AddComponent<TextMeshProUGUI>();
                    tmp.text = "0"; tmp.fontSize = 14; tmp.color = Color.white;
                    tmp.alignment = TextAlignmentOptions.Left;
                    tmp.fontStyle = FontStyles.Bold;
                }
                fpScript.valueText = tmp;
                EditorUtility.SetDirty(topBar.gameObject);
                Debug.Log("TopBarFP wired");
            }
        }

        // ── 2. Ensure ExpandablePanel panel hidden pos = 210 ─────────
        var panel = canvas.transform.Find("ExpandablePanel");
        if (panel != null)
        {
            var rt = panel.GetComponent<RectTransform>();
            // offsetMin/offsetMax defines the panel width relative to anchor
            // anchorMin=(1,0), anchorMax=(1,1), offsetMin=(-200,28), offsetMax=(0,0)
            // => panel is 200px wide, from right edge to -200
            // When hidden: anchoredPosition.x = 210 pushes it off-screen
            rt.anchoredPosition = new Vector2(210, 0);
            EditorUtility.SetDirty(panel.gameObject);
        }

        // ── 3. Verify UIManager wiring ────────────────────────────────
        var uiMgr = canvas.GetComponent<UIManager>();
        if (uiMgr != null)
        {
            if (uiMgr.expandablePanel == null && panel != null)
                uiMgr.expandablePanel = panel.GetComponent<RectTransform>();

            var bar = canvas.transform.Find("RightIconBar");
            if (bar != null)
            {
                var sb = bar.Find("SeedButton");
                var bb = bar.Find("BuildButton");
                var ub = bar.Find("UpgradeButton");
                if (sb && uiMgr.seedButton == null)    uiMgr.seedButton    = sb.GetComponent<Button>();
                if (bb && uiMgr.buildButton == null)   uiMgr.buildButton   = bb.GetComponent<Button>();
                if (ub && uiMgr.upgradeButton == null) uiMgr.upgradeButton = ub.GetComponent<Button>();
            }

            var cls = panel?.Find("BottomBar/CloseBtn");
            if (cls && uiMgr.closeButton == null)
                uiMgr.closeButton = cls.GetComponent<Button>();

            EditorUtility.SetDirty(canvas);
            Debug.Log($"UIManager: seed={uiMgr.seedButton?.name}, panel={uiMgr.expandablePanel?.name}, close={uiMgr.closeButton?.name}");
        }

        // ── 4. PomoCanvas reference resolution check ──────────────────
        var pomoCanvas = GameObject.Find("PomoCanvas");
        if (pomoCanvas != null)
        {
            var sc = pomoCanvas.GetComponent<CanvasScaler>();
            if (sc != null)
            {
                sc.referenceResolution = new Vector2(724, 407);
                sc.matchWidthOrHeight = 0.5f;
                EditorUtility.SetDirty(pomoCanvas);
                Debug.Log("PomoCanvas scaler fixed");
            }
        }

        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("[FinalSceneWire] Done!");
    }
}
