using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class FixIconBarPosition
{
    static Color Hex(string h) { ColorUtility.TryParseHtmlString(h, out Color c); return c; }
    static readonly string OBJ = "Assets/Sprout Lands - Sprites - Basic pack/Objects/";

    static Sprite GetSprite(string sheet, int row, int col)
    {
        string key = System.IO.Path.GetFileNameWithoutExtension(sheet) + "_" + row + "_" + col;
        foreach (var obj in AssetDatabase.LoadAllAssetsAtPath(OBJ + sheet))
            if (obj is Sprite s && s.name == key) return s;
        return null;
    }

    public static void Execute()
    {
        var canvas = GameObject.Find("UICanvas");
        if (canvas == null) { Debug.LogError("UICanvas not found"); return; }

        // ── Rebuild RightIconBar cleanly ───────────────────────────────
        var oldBar = canvas.transform.Find("RightIconBar");
        if (oldBar != null) Object.DestroyImmediate(oldBar.gameObject);

        var bar = new GameObject("RightIconBar");
        bar.transform.SetParent(canvas.transform, false);
        var barRT = bar.AddComponent<RectTransform>();

        // Full right strip, below TopBar (32px)
        barRT.anchorMin = new Vector2(1, 0);
        barRT.anchorMax = new Vector2(1, 1);
        barRT.pivot     = new Vector2(1, 0.5f);
        barRT.offsetMin = new Vector2(-52, 32);   // 52px wide
        barRT.offsetMax = new Vector2(0, 0);

        var barImg = bar.AddComponent<Image>();
        barImg.color = Hex("#2a1a0a");

        // 3 icon buttons with fixed px size
        var defs = new (string name, string sheet, int r, int c, Color bg)[]
        {
            ("SeedButton",    "Basic Plants.png",              0, 0, Hex("#2a6a0c")),
            ("BuildButton",   "Basic tools and meterials.png", 0, 2, Hex("#6a4008")),
            ("UpgradeButton", "Basic Grass Biom things 1.png", 0, 3, Hex("#082848")),
        };

        for (int i = 0; i < defs.Length; i++)
        {
            var (name, sheet, r, c, bg) = defs[i];
            var btn = new GameObject(name);
            btn.transform.SetParent(bar.transform, false);
            var bRT = btn.AddComponent<RectTransform>();

            // Anchor from top, fixed 44px square
            bRT.anchorMin = new Vector2(0.1f, 1f);
            bRT.anchorMax = new Vector2(0.9f, 1f);
            bRT.pivot     = new Vector2(0.5f, 1f);
            bRT.sizeDelta = new Vector2(0, 44);
            bRT.anchoredPosition = new Vector2(0, -(8 + i * 50f));

            var bImg = btn.AddComponent<Image>();
            bImg.color = bg;
            var button = btn.AddComponent<Button>();
            button.targetGraphic = bImg;
            var cs = button.colors;
            cs.highlightedColor = new Color(1f,1f,0.6f); button.colors = cs;

            var bOL = btn.AddComponent<Outline>();
            bOL.effectColor = Hex("#0a0804"); bOL.effectDistance = new Vector2(1,-1);

            var spr = GetSprite(sheet, r, c);
            if (spr != null)
            {
                var ico = new GameObject("Icon");
                ico.transform.SetParent(btn.transform, false);
                var iRT = ico.AddComponent<RectTransform>();
                iRT.anchorMin = new Vector2(0.1f,0.1f);
                iRT.anchorMax = new Vector2(0.9f,0.9f);
                iRT.offsetMin = iRT.offsetMax = Vector2.zero;
                var iImg = ico.AddComponent<Image>();
                iImg.sprite = spr; iImg.preserveAspect = true; iImg.raycastTarget = false;
            }
        }

        // Fix TopBar
        var topBar = canvas.transform.Find("TopBar")?.gameObject;
        if (topBar != null)
        {
            var rt = topBar.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0,1); rt.anchorMax = new Vector2(1,1);
            rt.pivot = new Vector2(0.5f,1f);
            rt.offsetMin = new Vector2(0,-32); rt.offsetMax = new Vector2(0,0);
            EditorUtility.SetDirty(topBar);
        }

        // Fix ExpandablePanel position (hidden: right+235, shown: right edge)
        var panel = canvas.transform.Find("ExpandablePanel")?.gameObject;
        if (panel != null)
        {
            var rt = panel.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(1,0); rt.anchorMax = new Vector2(1,1);
            rt.pivot = new Vector2(1,0.5f);
            rt.offsetMin = new Vector2(-230,32); rt.offsetMax = new Vector2(0,0);
            rt.anchoredPosition = new Vector2(235,0); // hidden
            EditorUtility.SetDirty(panel);
        }

        // Wire UIManager
        var uiMgr = canvas.GetComponent<UIManager>();
        if (uiMgr != null)
        {
            var sb = bar.transform.Find("SeedButton");
            var bb = bar.transform.Find("BuildButton");
            var ub = bar.transform.Find("UpgradeButton");
            if (sb) uiMgr.seedButton    = sb.GetComponent<Button>();
            if (bb) uiMgr.buildButton   = bb.GetComponent<Button>();
            if (ub) uiMgr.upgradeButton = ub.GetComponent<Button>();
            EditorUtility.SetDirty(canvas);
        }

        EditorUtility.SetDirty(bar);
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("[FixIconBarPosition] Done! IconBar full-height right strip.");
    }
}
