using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class FinalUIFix
{
    static Color C(string h) { ColorUtility.TryParseHtmlString(h, out Color c); return c; }
    static readonly string OBJ = "Assets/Sprout Lands - Sprites - Basic pack/Objects/";

    static Sprite GetSpr(string sheet, int r, int c)
    {
        string k = System.IO.Path.GetFileNameWithoutExtension(sheet) + "_" + r + "_" + c;
        foreach (var o in AssetDatabase.LoadAllAssetsAtPath(OBJ + sheet))
            if (o is Sprite s && s.name == k) return s;
        return null;
    }

    public static void Execute()
    {
        var canvas = GameObject.Find("UICanvas");
        if (canvas == null) { Debug.LogError("UICanvas not found"); return; }

        // ── 1. CanvasScaler: match WIDTH so 1280 = game width ─────────
        // Game window is ~724px wide. With matchWidth=0:
        //   scaleFactor = 724/1280 = 0.565  (our units map to fewer pixels)
        // With matchHeight=1:
        //   scaleFactor = 407/720 = 0.565  (same result here)
        // The issue: 52 UI units × 0.565 = 29 pixels — too thin!
        // Solution: use a smaller reference resolution so UI units = more pixels
        var scaler = canvas.GetComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(724, 407); // match actual window size
        scaler.screenMatchMode     = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight  = 0.5f;
        EditorUtility.SetDirty(canvas);
        Debug.Log("CanvasScaler: reference 724x407 (matches game window)");

        // ── 2. Fix TopBar ─────────────────────────────────────────────
        var topBar = canvas.transform.Find("TopBar")?.gameObject;
        if (topBar != null)
        {
            var rt = topBar.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0,1); rt.anchorMax = new Vector2(1,1);
            rt.pivot     = new Vector2(0.5f,1);
            rt.offsetMin = new Vector2(0,-28); rt.offsetMax = new Vector2(0,0);
            EditorUtility.SetDirty(topBar);

            // Fix text sizes for smaller canvas
            foreach (var tmp in topBar.GetComponentsInChildren<TextMeshProUGUI>())
            {
                tmp.fontSize = Mathf.Min(tmp.fontSize, 13);
                EditorUtility.SetDirty(tmp.gameObject);
            }
        }

        // ── 3. Rebuild RightIconBar at correct size ────────────────────
        var oldBar = canvas.transform.Find("RightIconBar");
        if (oldBar != null) Object.DestroyImmediate(oldBar.gameObject);

        var bar = new GameObject("RightIconBar");
        bar.transform.SetParent(canvas.transform, false);
        var barRT = bar.AddComponent<RectTransform>();
        // 44px wide, full height below topbar
        barRT.anchorMin = new Vector2(1,0);
        barRT.anchorMax = new Vector2(1,1);
        barRT.pivot     = new Vector2(1,0.5f);
        barRT.offsetMin = new Vector2(-44, 28);
        barRT.offsetMax = new Vector2(0, 0);
        var barImg = bar.AddComponent<Image>();
        barImg.color = C("#1a1006");

        // 3 buttons
        var defs = new (string n, string sh, int r, int c, Color col)[]
        {
            ("SeedButton",    "Basic Plants.png",              0, 0, C("#2a7010")),
            ("BuildButton",   "Basic tools and meterials.png", 0, 2, C("#7a4010")),
            ("UpgradeButton", "Basic Grass Biom things 1.png", 0, 3, C("#103870")),
        };

        float btnH = 42f;
        for (int i = 0; i < defs.Length; i++)
        {
            var (n, sh, r, c, col) = defs[i];
            var btn = new GameObject(n);
            btn.transform.SetParent(bar.transform, false);
            var bRT = btn.AddComponent<RectTransform>();
            bRT.anchorMin = new Vector2(0,1); bRT.anchorMax = new Vector2(1,1);
            bRT.pivot     = new Vector2(0.5f,1);
            bRT.sizeDelta = new Vector2(0, btnH);
            bRT.anchoredPosition = new Vector2(0, -(4 + i*(btnH+3)));

            var img = btn.AddComponent<Image>();
            img.color = col;

            var button = btn.AddComponent<Button>();
            button.targetGraphic = img;
            var cs = ColorBlock.defaultColorBlock;
            cs.normalColor = Color.white;
            cs.highlightedColor = new Color(1.2f,1.2f,0.8f);
            cs.pressedColor = new Color(0.75f,0.75f,0.75f);
            button.colors = cs;

            var bOL = btn.AddComponent<Outline>();
            bOL.effectColor = new Color(0,0,0,0.5f);
            bOL.effectDistance = new Vector2(1,-1);

            // Sprite
            var spr = GetSpr(sh, r, c);
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

        // ── 4. Fix ExpandablePanel size ───────────────────────────────
        var panel = canvas.transform.Find("ExpandablePanel")?.gameObject;
        if (panel != null)
        {
            var rt = panel.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(1,0); rt.anchorMax = new Vector2(1,1);
            rt.pivot     = new Vector2(1,0.5f);
            rt.offsetMin = new Vector2(-200,28); rt.offsetMax = new Vector2(0,0);
            rt.anchoredPosition = new Vector2(205,0); // hidden
            EditorUtility.SetDirty(panel);
        }

        // ── 5. Fix PomoWidget size (it's on PomoCanvas, smaller now) ──
        var pomoCanvas = GameObject.Find("PomoCanvas");
        var pomoScaler = pomoCanvas?.GetComponent<CanvasScaler>();
        if (pomoScaler != null)
        {
            pomoScaler.referenceResolution = new Vector2(724,407);
            pomoScaler.matchWidthOrHeight  = 0.5f;
            EditorUtility.SetDirty(pomoCanvas);
        }
        var pomoWidget = pomoCanvas?.transform.Find("PomoWidget");
        if (pomoWidget != null)
        {
            var wRT = pomoWidget.GetComponent<RectTransform>();
            wRT.sizeDelta = new Vector2(150, 200);
            wRT.anchoredPosition = new Vector2(5,-5);
            EditorUtility.SetDirty(pomoWidget.gameObject);
        }

        // ── 6. Wire UIManager ─────────────────────────────────────────
        var uiMgr = canvas.GetComponent<UIManager>() ?? canvas.AddComponent<UIManager>();
        if (panel != null) uiMgr.expandablePanel = panel.GetComponent<RectTransform>();
        var closePath = "ExpandablePanel/BottomBar/CloseBtn";
        var closeGO = canvas.transform.Find(closePath);
        if (closeGO) uiMgr.closeButton = closeGO.GetComponent<Button>();
        uiMgr.seedButton    = bar.transform.Find("SeedButton")?.GetComponent<Button>();
        uiMgr.buildButton   = bar.transform.Find("BuildButton")?.GetComponent<Button>();
        uiMgr.upgradeButton = bar.transform.Find("UpgradeButton")?.GetComponent<Button>();
        EditorUtility.SetDirty(canvas);

        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("[FinalUIFix] Done! Canvas=724x407, IconBar=44px, sprites loaded.");
    }
}
