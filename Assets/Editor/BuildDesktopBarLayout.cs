using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

/// Rebuilds layout as a bottom desktop bar like Rusty's Retirement:
/// - Full screen height game window
/// - Bottom strip (~22% height): Timer left | Farm center | Shop right
/// - Top 78%: Sky + background (visible farm world)
public class BuildDesktopBarLayout
{
    const float BAR_HEIGHT = 0.28f;  // bottom 28% is the bar

    public static void Execute()
    {
        var canvas = GameObject.Find("UICanvas");
        if (canvas == null) { Debug.LogError("UICanvas not found"); return; }

        // ── 1. Camera: sky blue, fills entire screen ──
        var cam = Camera.main;
        if (cam != null)
        {
            cam.clearFlags      = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.50f, 0.78f, 0.95f, 1f);
            EditorUtility.SetDirty(cam.gameObject);
        }

        // ── 2. Remove old layout panels ──
        foreach (var name in new[]{"RightPanel","GroundStrip","GrassMid","PanelDivider",
                                    "Clouds","TopBackground","BackgroundPanel"})
        {
            var t = canvas.transform.Find(name);
            if (t != null) Object.DestroyImmediate(t.gameObject);
        }

        // ── 3. Bottom bar background (full width) ──
        var barGO  = MakePanel(canvas, "BottomBar", new Color(0.08f, 0.09f, 0.11f, 0.97f), 0);
        SetStretch(barGO, 0f, 0f, 1f, BAR_HEIGHT);

        // Thin top border on bar
        var borderGO = MakePanel(canvas, "BarBorder", new Color(0.25f, 0.30f, 0.38f, 1f), 1);
        SetStretch(borderGO, 0f, BAR_HEIGHT - 0.003f, 1f, BAR_HEIGHT);

        // ── 4. Grass strip just above bar ──
        var grassGO = MakePanel(canvas, "GrassStrip", new Color(0.24f, 0.46f, 0.13f, 1f), 0);
        SetStretch(grassGO, 0f, BAR_HEIGHT, 1f, BAR_HEIGHT + 0.04f);

        // ── 5. LEFT SECTION — Timer (leftmost 20%) ──
        var timerSection = MakePanel(canvas, "TimerSection", new Color(0.06f, 0.08f, 0.10f, 0f), 2);
        SetStretch(timerSection, 0f, 0f, 0.20f, BAR_HEIGHT);

        // Move Timer elements into TimerSection
        MoveTimerElements(canvas, timerSection.transform);

        // ── 6. RIGHT SECTION — Shop (rightmost 22%) ──
        var shopSection = MakePanel(canvas, "ShopSection", new Color(0.06f, 0.08f, 0.10f, 0f), 2);
        SetStretch(shopSection, 0.78f, 0f, 1f, BAR_HEIGHT);

        // Move shop elements
        MoveShopElements(canvas, shopSection.transform);

        // ── 7. CENTER — Farm area label ──
        // Farm plots are World Space, already visible
        // Add a "FARM" label at center-bottom of bar
        var farmLabel = new GameObject("FarmAreaLabel");
        farmLabel.transform.SetParent(canvas.transform, false);
        var flRect = farmLabel.AddComponent<RectTransform>();
        flRect.anchorMin = new Vector2(0.20f, BAR_HEIGHT * 0.85f);
        flRect.anchorMax = new Vector2(0.78f, BAR_HEIGHT);
        flRect.offsetMin = Vector2.zero;
        flRect.offsetMax = Vector2.zero;
        var flTmp = farmLabel.AddComponent<TextMeshProUGUI>();
        flTmp.text      = "— Farm —";
        flTmp.fontSize  = 11f;
        flTmp.color     = new Color(0.40f, 0.42f, 0.45f, 0.8f);
        flTmp.alignment = TextAlignmentOptions.Center;
        flTmp.raycastTarget = false;

        // ── 8. Reposition Farm Plots into the visible area above the bar ──
        // Bar = bottom 28%, so farm plots should be in world y = -2 to +5
        // Camera ortho=5 → world bottom = -5, bar top ≈ y = -5 + 10*0.28 = -2.2
        // Front row at y=-1.0, back row at y=+1.2 (more vertical space now)
        RepositionPlots();

        // ── 9. PopupAnchor: center of screen ──
        var popup = canvas.transform.Find("PopupAnchor");
        if (popup != null)
        {
            var r = popup.GetComponent<RectTransform>();
            r.anchorMin = new Vector2(0.5f, 0.5f);
            r.anchorMax = new Vector2(0.5f, 0.5f);
            r.anchoredPosition = Vector2.zero;
            r.sizeDelta = new Vector2(300f, 200f);
            EditorUtility.SetDirty(popup.gameObject);
        }

        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("BuildDesktopBarLayout complete + saved!");
    }

    static void MoveTimerElements(GameObject canvas, Transform timerSection)
    {
        // CenterContainer (timer ring)
        var center = canvas.transform.Find("RightPanel/CenterContainer")
                  ?? canvas.transform.Find("CenterContainer");
        if (center != null)
        {
            center.SetParent(timerSection, false);
            var r = center.GetComponent<RectTransform>();
            // Square timer in top part of timer section
            r.anchorMin        = new Vector2(0.5f, 1f);
            r.anchorMax        = new Vector2(0.5f, 1f);
            r.pivot            = new Vector2(0.5f, 1f);
            r.sizeDelta        = new Vector2(200f, 210f);
            r.anchoredPosition = new Vector2(0f, 0f);
            EditorUtility.SetDirty(center.gameObject);
        }

        // ButtonBar
        var buttonBar = canvas.transform.Find("RightPanel/ButtonBar")
                     ?? canvas.transform.Find("ButtonBar");
        if (buttonBar != null)
        {
            buttonBar.SetParent(timerSection, false);
            var r = buttonBar.GetComponent<RectTransform>();
            r.anchorMin        = new Vector2(0.04f, 0f);
            r.anchorMax        = new Vector2(0.96f, 0f);
            r.pivot            = new Vector2(0.5f, 0f);
            r.sizeDelta        = new Vector2(0f, 44f);
            r.anchoredPosition = new Vector2(0f, 6f);
            EditorUtility.SetDirty(buttonBar.gameObject);
        }

        // CycleDots
        var dots = canvas.transform.Find("RightPanel/CycleDots")
                ?? canvas.transform.Find("CycleDots");
        if (dots != null)
        {
            dots.SetParent(timerSection, false);
            var r = dots.GetComponent<RectTransform>();
            r.anchorMin        = new Vector2(0.5f, 0f);
            r.anchorMax        = new Vector2(0.5f, 0f);
            r.pivot            = new Vector2(0.5f, 0f);
            r.sizeDelta        = new Vector2(120f, 16f);
            r.anchoredPosition = new Vector2(0f, 54f);
            EditorUtility.SetDirty(dots.gameObject);
        }
    }

    static void MoveShopElements(GameObject canvas, Transform shopSection)
    {
        // TopRightPanel (FP stats)
        var stats = canvas.transform.Find("RightPanel/TopRightPanel")
                 ?? canvas.transform.Find("TopRightPanel");
        if (stats != null)
        {
            stats.SetParent(shopSection, false);
            var r = stats.GetComponent<RectTransform>();
            r.anchorMin        = new Vector2(0f, 0.72f);
            r.anchorMax        = new Vector2(1f, 1f);
            r.offsetMin        = new Vector2(4f, 0f);
            r.offsetMax        = new Vector2(-4f, -4f);
            r.anchoredPosition = Vector2.zero;
            r.sizeDelta        = Vector2.zero;
            EditorUtility.SetDirty(stats.gameObject);

            // Rearrange stats
            LayoutStat(stats, "FPLabel",         0f,0.75f,1f,1.00f, 9f,  new Color(0.5f,0.5f,0.5f,1f));
            LayoutStat(stats, "FocusPointsText", 0f,0.35f,1f,0.77f, 22f, Color.white);
            LayoutStat(stats, "IncomeRateText",  0f,0.00f,1f,0.38f, 11f, new Color(0.2f,0.85f,0.7f,1f));
            var sess = stats.Find("SessionCountText");
            if (sess != null) sess.gameObject.SetActive(false);
        }

        // CropShopPanel
        var shopPanel = canvas.transform.Find("RightPanel/CropShopPanel")
                     ?? canvas.transform.Find("CropShopPanel");
        if (shopPanel != null)
        {
            shopPanel.SetParent(shopSection, false);
            var r = shopPanel.GetComponent<RectTransform>();
            r.anchorMin        = new Vector2(0f, 0f);
            r.anchorMax        = new Vector2(1f, 0.70f);
            r.offsetMin        = new Vector2(3f, 3f);
            r.offsetMax        = new Vector2(-3f, -3f);
            r.anchoredPosition = Vector2.zero;
            r.sizeDelta        = Vector2.zero;
            EditorUtility.SetDirty(shopPanel.gameObject);
        }
    }

    static void RepositionPlots()
    {
        // Bar height = 28% of screen. Camera ortho=5 → world height 10
        // Bar top world y = -5 + 10*0.28 = -2.2
        // Place 2 rows clearly above bar
        float cx   = 0f;   // centered in world
        float sp   = 3.5f;
        float row0 = 1.2f;   // back row
        float row1 = -1.2f;  // front row (just above grass)

        var positions = new Vector3[]
        {
            new Vector3(cx - sp,  row0, 0f),
            new Vector3(cx,       row0, 0f),
            new Vector3(cx + sp,  row0, 0f),
            new Vector3(cx - sp,  row1, 0f),
            new Vector3(cx,       row1, 0f),
            new Vector3(cx + sp,  row1, 0f),
        };

        for (int i = 0; i < 6; i++)
        {
            var go = GameObject.Find("FarmPlot_" + (i + 1));
            if (go == null) continue;
            go.transform.position   = positions[i];
            go.transform.localScale = new Vector3(22f, 22f, 1f);
            EditorUtility.SetDirty(go);
        }
    }

    // ── Helpers ───────────────────────────────────────────
    static GameObject MakePanel(GameObject canvas, string name, Color color, int siblingIndex)
    {
        var go  = new GameObject(name);
        go.transform.SetParent(canvas.transform, false);
        go.AddComponent<RectTransform>();
        var img = go.AddComponent<Image>();
        img.color = color;
        img.raycastTarget = false;
        go.transform.SetSiblingIndex(siblingIndex);
        return go;
    }

    static void SetStretch(GameObject go, float ax, float ay, float bx, float by)
    {
        var r = go.GetComponent<RectTransform>();
        r.anchorMin = new Vector2(ax, ay); r.anchorMax = new Vector2(bx, by);
        r.offsetMin = Vector2.zero;        r.offsetMax = Vector2.zero;
        r.anchoredPosition = Vector2.zero; r.sizeDelta = Vector2.zero;
        EditorUtility.SetDirty(go);
    }

    static void LayoutStat(Transform parent, string name,
        float ax, float ay, float bx, float by, float fontSize, Color color)
    {
        var t = parent.Find(name); if (t == null) return;
        var r = t.GetComponent<RectTransform>();
        r.anchorMin = new Vector2(ax,ay); r.anchorMax = new Vector2(bx,by);
        r.offsetMin = Vector2.zero;       r.offsetMax = Vector2.zero;
        r.anchoredPosition = Vector2.zero; r.sizeDelta = Vector2.zero;
        var tmp = t.GetComponent<TextMeshProUGUI>();
        if (tmp != null) { tmp.fontSize = fontSize; tmp.color = color; tmp.alignment = TMPro.TextAlignmentOptions.Center; }
        EditorUtility.SetDirty(t.gameObject);
    }
}
