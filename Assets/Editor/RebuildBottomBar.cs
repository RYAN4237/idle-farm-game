using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

/// Rebuilds the bottom toolbar to match Rusty Retirement's pixel-farm aesthetic:
/// - Dark earth-tone panel (deep slate-green border, warm interior)
/// - Left: coin icon + FP counter
/// - Center-left: crop count placeholder
/// - Right: SEEDS button (forest green)
/// - ExpandablePanel: dark panel with green tab accents
public class RebuildBottomBar
{
    // Rusty Retirement palette
    static readonly Color BAR_BG        = new Color(0.13f, 0.18f, 0.13f, 0.96f); // deep forest
    static readonly Color BAR_BORDER    = new Color(0.08f, 0.12f, 0.08f, 1.00f); // darker border line
    static readonly Color COUNTER_BG    = new Color(0.08f, 0.12f, 0.09f, 0.90f); // counter pill bg
    static readonly Color SEEDS_BTN     = new Color(0.22f, 0.48f, 0.22f, 1.00f); // grass green
    static readonly Color SEEDS_PRESS   = new Color(0.16f, 0.36f, 0.16f, 1.00f);
    static readonly Color TEXT_GOLD     = new Color(0.96f, 0.84f, 0.45f, 1.00f); // gold resource text
    static readonly Color TEXT_WHITE    = new Color(0.95f, 0.95f, 0.90f, 1.00f);
    static readonly Color PANEL_BG      = new Color(0.11f, 0.15f, 0.11f, 0.97f);
    static readonly Color TAB_ACTIVE    = new Color(0.22f, 0.48f, 0.22f, 1.00f);
    static readonly Color TAB_INACTIVE  = new Color(0.15f, 0.20f, 0.15f, 1.00f);

    [MenuItem("Tools/Rebuild Bottom Bar (Rusty Retirement Style)")]
    public static void Execute()
    {
        var canvas = GameObject.Find("UICanvas");
        if (canvas == null) { Debug.LogError("[RebuildBottomBar] UICanvas not found"); return; }

        RebuildBar(canvas);
        RebuildPanel(canvas);

        EditorUtility.SetDirty(canvas);
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log("[RebuildBottomBar] Done — Rusty Retirement style applied");
    }

    // ─────────────────────────────────────────────────────────────────
    // BOTTOM BAR
    // ─────────────────────────────────────────────────────────────────
    static void RebuildBar(GameObject canvas)
    {
        var barTf = canvas.transform.Find("BottomBar");
        if (barTf == null) { Debug.LogError("BottomBar not found"); return; }

        // --- Main bar background ---
        var barImg = barTf.GetComponent<Image>();
        barImg.sprite = null;
        barImg.type   = Image.Type.Simple;
        barImg.color  = BAR_BG;
        var barRt = barTf.GetComponent<RectTransform>();
        barRt.sizeDelta = new Vector2(0, 56);

        // Top border line
        EnsureBorderLine(barTf.gameObject, "TopBorder", BAR_BORDER, 0, 56, 2);

        // --- FPDisplay (left counter) ---
        var fpTf = barTf.Find("FPDisplay");
        if (fpTf != null)
        {
            var fpRt = fpTf.GetComponent<RectTransform>();
            fpRt.anchorMin        = new Vector2(0, 0);
            fpRt.anchorMax        = new Vector2(0, 1);
            fpRt.pivot            = new Vector2(0, 0.5f);
            fpRt.anchoredPosition = new Vector2(10, 0);
            fpRt.sizeDelta        = new Vector2(130, -10);

            var fpImg = fpTf.GetComponent<Image>();
            if (fpImg == null) fpImg = fpTf.gameObject.AddComponent<Image>();
            fpImg.sprite        = null;
            fpImg.type          = Image.Type.Simple;
            fpImg.color         = COUNTER_BG;
            fpImg.raycastTarget = false;

            // Coin icon
            var coinTf = fpTf.Find("CoinIcon");
            if (coinTf != null)
            {
                var cRt = coinTf.GetComponent<RectTransform>();
                cRt.anchorMin        = new Vector2(0, 0.5f);
                cRt.anchorMax        = new Vector2(0, 0.5f);
                cRt.pivot            = new Vector2(0, 0.5f);
                cRt.anchoredPosition = new Vector2(8, 0);
                cRt.sizeDelta        = new Vector2(28, 28);
                var cImg = coinTf.GetComponent<Image>();
                if (cImg != null) cImg.color = new Color(0.98f, 0.82f, 0.22f, 1f);
            }

            // FP text
            var fpTextTf = fpTf.Find("FPText");
            if (fpTextTf != null)
            {
                var tRt = fpTextTf.GetComponent<RectTransform>();
                tRt.anchorMin        = new Vector2(0, 0);
                tRt.anchorMax        = new Vector2(1, 1);
                tRt.anchoredPosition = new Vector2(18, 0);
                tRt.sizeDelta        = new Vector2(-14, 0);
                var tmp = fpTextTf.GetComponent<TextMeshProUGUI>();
                if (tmp != null)
                {
                    tmp.color     = TEXT_GOLD;
                    tmp.fontSize  = 20;
                    tmp.fontStyle = FontStyles.Bold;
                    tmp.alignment = TextAlignmentOptions.MidlineLeft;
                }
            }
        }

        // --- ShopButton (right) ---
        var shopTf = barTf.Find("ShopButton");
        if (shopTf != null)
        {
            var sRt = shopTf.GetComponent<RectTransform>();
            sRt.anchorMin        = new Vector2(1, 0);
            sRt.anchorMax        = new Vector2(1, 1);
            sRt.pivot            = new Vector2(1, 0.5f);
            sRt.anchoredPosition = new Vector2(-10, 0);
            sRt.sizeDelta        = new Vector2(110, -10);

            var sImg = shopTf.GetComponent<Image>();
            sImg.sprite = null;
            sImg.type   = Image.Type.Simple;
            sImg.color  = SEEDS_BTN;

            var btn = shopTf.GetComponent<Button>();
            if (btn != null)
            {
                var colors = btn.colors;
                colors.normalColor      = SEEDS_BTN;
                colors.highlightedColor = new Color(0.28f, 0.58f, 0.28f, 1f);
                colors.pressedColor     = SEEDS_PRESS;
                colors.colorMultiplier  = 1f;
                btn.colors = colors;
            }

            var labelTf = shopTf.Find("Label");
            if (labelTf != null)
            {
                var tmp = labelTf.GetComponent<TextMeshProUGUI>();
                if (tmp != null)
                {
                    tmp.text      = "🌱 SEEDS";
                    tmp.color     = TEXT_WHITE;
                    tmp.fontSize  = 16;
                    tmp.fontStyle = FontStyles.Bold;
                    tmp.alignment = TextAlignmentOptions.Center;
                }
                var lRt = labelTf.GetComponent<RectTransform>();
                lRt.anchorMin = Vector2.zero;
                lRt.anchorMax = Vector2.one;
                lRt.sizeDelta = Vector2.zero;
                lRt.anchoredPosition = Vector2.zero;
            }
        }

        Debug.Log("[RebuildBottomBar] BottomBar rebuilt");
    }

    // ─────────────────────────────────────────────────────────────────
    // EXPANDABLE PANEL
    // ─────────────────────────────────────────────────────────────────
    static void RebuildPanel(GameObject canvas)
    {
        var panelTf = canvas.transform.Find("ExpandablePanel");
        if (panelTf == null) return;

        // Main panel bg
        var pImg = panelTf.GetComponent<Image>();
        pImg.sprite = null;
        pImg.type   = Image.Type.Simple;
        pImg.color  = PANEL_BG;

        // Panel size: 240 wide, full height minus bar
        var pRt = panelTf.GetComponent<RectTransform>();
        pRt.anchorMin        = new Vector2(1, 0);
        pRt.anchorMax        = new Vector2(1, 1);
        pRt.pivot            = new Vector2(1, 0);
        pRt.anchoredPosition = new Vector2(240, 56);
        pRt.sizeDelta        = new Vector2(240, -56);

        // Title bar
        var titleTf = panelTf.Find("TitleBar");
        if (titleTf != null)
        {
            var tImg = titleTf.GetComponent<Image>();
            if (tImg != null) { tImg.sprite = null; tImg.color = new Color(0.08f, 0.12f, 0.08f, 1f); }
            var tRt = titleTf.GetComponent<RectTransform>();
            tRt.anchorMin        = new Vector2(0, 1);
            tRt.anchorMax        = new Vector2(1, 1);
            tRt.pivot            = new Vector2(0.5f, 1);
            tRt.anchoredPosition = Vector2.zero;
            tRt.sizeDelta        = new Vector2(0, 40);

            var titleText = titleTf.Find("TitleText")?.GetComponent<TextMeshProUGUI>();
            if (titleText != null) { titleText.color = TEXT_GOLD; titleText.fontSize = 17; titleText.fontStyle = FontStyles.Bold; }

            var closeBtn = titleTf.Find("CloseButton");
            if (closeBtn != null)
            {
                var cImg = closeBtn.GetComponent<Image>();
                if (cImg != null) { cImg.sprite = null; cImg.color = new Color(0.55f, 0.18f, 0.12f, 1f); }
                var xTxt = closeBtn.Find("X")?.GetComponent<TextMeshProUGUI>();
                if (xTxt != null) xTxt.color = TEXT_WHITE;
            }
        }

        // Tab row
        var tabRowTf = panelTf.Find("TabRow");
        if (tabRowTf != null)
        {
            var tabRt = tabRowTf.GetComponent<RectTransform>();
            tabRt.anchorMin        = new Vector2(0, 1);
            tabRt.anchorMax        = new Vector2(1, 1);
            tabRt.pivot            = new Vector2(0.5f, 1);
            tabRt.anchoredPosition = new Vector2(0, -40);
            tabRt.sizeDelta        = new Vector2(0, 32);

            StyleTabBtn(tabRowTf.Find("SeedsTab"), "SEEDS");
            StyleTabBtn(tabRowTf.Find("AutoTab"),  "AUTO");
            StyleTabBtn(tabRowTf.Find("BuildTab"), "BUILD");
        }

        // Content area
        var contentTf = panelTf.Find("ContentArea");
        if (contentTf != null)
        {
            var cImg = contentTf.GetComponent<Image>();
            if (cImg != null) { cImg.sprite = null; cImg.color = new Color(0.10f, 0.14f, 0.10f, 0.95f); }
            var cRt = contentTf.GetComponent<RectTransform>();
            cRt.anchorMin        = new Vector2(0, 0);
            cRt.anchorMax        = new Vector2(1, 1);
            cRt.pivot            = new Vector2(0.5f, 1);
            cRt.anchoredPosition = new Vector2(0, -72);
            cRt.sizeDelta        = new Vector2(0, -72);
        }

        // Update TabMenuController colors to match
        var tmc = panelTf.GetComponent<TabMenuController>();
        if (tmc != null)
        {
            tmc.activeTabColor   = TAB_ACTIVE;
            tmc.inactiveTabColor = TAB_INACTIVE;
            tmc.activeTextColor  = TEXT_WHITE;
            tmc.inactiveTextColor = new Color(0.55f, 0.65f, 0.55f, 1f);
            EditorUtility.SetDirty(tmc);
        }

        Debug.Log("[RebuildBottomBar] ExpandablePanel rebuilt");
    }

    static void StyleTabBtn(Transform tf, string label)
    {
        if (tf == null) return;
        var img = tf.GetComponent<Image>();
        if (img != null) { img.sprite = null; img.color = TAB_INACTIVE; }
        var txt = tf.Find("Text")?.GetComponent<TextMeshProUGUI>();
        if (txt != null)
        {
            txt.text      = label;
            txt.color     = new Color(0.55f, 0.65f, 0.55f, 1f);
            txt.fontSize  = 13;
            txt.fontStyle = FontStyles.Bold;
            txt.alignment = TextAlignmentOptions.Center;
        }
    }

    static void EnsureBorderLine(GameObject parent, string name, Color color, float anchorY, float posY, float height)
    {
        var tf = parent.transform.Find(name);
        GameObject go;
        if (tf == null)
        {
            go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            go.AddComponent<Image>();
        }
        else go = tf.gameObject;

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin        = new Vector2(0, 1);
        rt.anchorMax        = new Vector2(1, 1);
        rt.pivot            = new Vector2(0.5f, 1);
        rt.anchoredPosition = new Vector2(0, 0);
        rt.sizeDelta        = new Vector2(0, height);

        var img = go.GetComponent<Image>();
        img.sprite        = null;
        img.color         = color;
        img.raycastTarget = false;
    }
}
