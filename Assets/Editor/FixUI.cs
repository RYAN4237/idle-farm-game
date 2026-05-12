using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// Fixes the UICanvas: correct CanvasScaler, proper BottomBar layout,
/// and adds the right-side expandable panel with Seeds/Auto/Build tabs.
public class FixUI
{
    const string SL_UI = "Assets/Sprout Lands - UI Pack - Basic pack/Sprite sheets/Sprite sheet for Basic Pack.png";

    [MenuItem("Farm/Fix UI")]
    public static void Execute()
    {
        var canvasGO = GameObject.Find("UICanvas");
        if (canvasGO == null)
        {
            Debug.LogError("[FixUI] UICanvas not found in scene!");
            return;
        }

        // ── 1. Fix CanvasScaler ──────────────────────────────────────
        var scaler = canvasGO.GetComponent<CanvasScaler>();
        if (scaler != null)
        {
            scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1631, 909);
            scaler.screenMatchMode     = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight  = 0.5f;
            EditorUtility.SetDirty(scaler);
            Debug.Log("[FixUI] CanvasScaler fixed: 1631x909");
        }

        // ── 2. Fix Canvas sort order ─────────────────────────────────
        var canvas = canvasGO.GetComponent<Canvas>();
        if (canvas != null)
        {
            canvas.sortingOrder = 100;
            EditorUtility.SetDirty(canvas);
        }

        // ── 3. Rebuild BottomBar children ────────────────────────────
        var bottomBarGO = canvasGO.transform.Find("BottomBar")?.gameObject;
        if (bottomBarGO == null)
        {
            Debug.LogError("[FixUI] BottomBar not found!");
            return;
        }

        // Destroy all existing children of BottomBar
        for (int i = bottomBarGO.transform.childCount - 1; i >= 0; i--)
            Object.DestroyImmediate(bottomBarGO.transform.GetChild(i).gameObject);

        // Fix BottomBar RectTransform — anchor to bottom strip
        var barRT = bottomBarGO.GetComponent<RectTransform>();
        barRT.anchorMin = new Vector2(0, 0);
        barRT.anchorMax = new Vector2(1, 0);
        barRT.pivot     = new Vector2(0.5f, 0f);
        barRT.offsetMin = Vector2.zero;
        barRT.offsetMax = Vector2.zero;
        barRT.sizeDelta = new Vector2(0, 60);
        EditorUtility.SetDirty(barRT);

        // Fix BottomBar image
        var barImg = bottomBarGO.GetComponent<Image>();
        if (barImg != null)
        {
            barImg.color = new Color(0.85f, 0.73f, 0.52f, 0.97f);
            var panelSpr = LoadSprite(SL_UI, "Sprite sheet for Basic Pack_0");
            if (panelSpr != null) { barImg.sprite = panelSpr; barImg.type = Image.Type.Sliced; }
            EditorUtility.SetDirty(barImg);
        }

        // ── 4. FP Display (left side of BottomBar) ───────────────────
        var fpGO = new GameObject("FPDisplay");
        fpGO.transform.SetParent(bottomBarGO.transform, false);
        var fpRT = fpGO.AddComponent<RectTransform>();
        fpRT.anchorMin        = new Vector2(0f, 0f);
        fpRT.anchorMax        = new Vector2(0f, 1f);
        fpRT.pivot            = new Vector2(0f, 0.5f);
        fpRT.anchoredPosition = new Vector2(8f, 0f);
        fpRT.sizeDelta        = new Vector2(140f, 0f);

        // Coin icon
        var coinGO = new GameObject("CoinIcon");
        coinGO.transform.SetParent(fpGO.transform, false);
        var coinRT = coinGO.AddComponent<RectTransform>();
        coinRT.anchorMin        = new Vector2(0f, 0.1f);
        coinRT.anchorMax        = new Vector2(0f, 0.9f);
        coinRT.pivot            = new Vector2(0f, 0.5f);
        coinRT.anchoredPosition = new Vector2(0f, 0f);
        coinRT.sizeDelta        = new Vector2(40f, 0f);
        var coinImg = coinGO.AddComponent<Image>();
        coinImg.preserveAspect = true;
        var coinSpr = LoadSprite(SL_UI, "Sprite sheet for Basic Pack_290");
        if (coinSpr != null) coinImg.sprite = coinSpr;
        else coinImg.color = new Color(1f, 0.85f, 0.1f);

        // FP Text
        var fpTxtGO = new GameObject("FPText");
        fpTxtGO.transform.SetParent(fpGO.transform, false);
        var fpTxtRT = fpTxtGO.AddComponent<RectTransform>();
        fpTxtRT.anchorMin        = new Vector2(0f, 0f);
        fpTxtRT.anchorMax        = new Vector2(1f, 1f);
        fpTxtRT.pivot            = new Vector2(0f, 0.5f);
        fpTxtRT.anchoredPosition = new Vector2(44f, 0f);
        fpTxtRT.sizeDelta        = new Vector2(-44f, 0f);
        var fpTM = fpTxtGO.AddComponent<TextMeshProUGUI>();
        fpTM.text      = "0";
        fpTM.fontSize  = 22;
        fpTM.fontStyle = FontStyles.Bold;
        fpTM.color     = new Color(0.22f, 0.12f, 0.02f);
        fpTM.alignment = TextAlignmentOptions.MidlineLeft;

        var topBarFP = fpGO.AddComponent<TopBarFP>();
        topBarFP.valueText = fpTM;
        EditorUtility.SetDirty(topBarFP);

        // ── 5. Shop / Seeds button (right side of BottomBar) ─────────
        var shopGO = new GameObject("ShopButton");
        shopGO.transform.SetParent(bottomBarGO.transform, false);
        var shopRT = shopGO.AddComponent<RectTransform>();
        shopRT.anchorMin        = new Vector2(1f, 0f);
        shopRT.anchorMax        = new Vector2(1f, 1f);
        shopRT.pivot            = new Vector2(1f, 0.5f);
        shopRT.anchoredPosition = new Vector2(-8f, 0f);
        shopRT.sizeDelta        = new Vector2(90f, -8f);

        var shopImg = shopGO.AddComponent<Image>();
        var btnSpr  = LoadSprite(SL_UI, "Sprite sheet for Basic Pack_672");
        if (btnSpr != null) { shopImg.sprite = btnSpr; shopImg.type = Image.Type.Sliced; }
        else shopImg.color = new Color(0.35f, 0.65f, 0.25f);

        var shopBtn = shopGO.AddComponent<Button>();
        var shopColors = shopBtn.colors;
        shopColors.highlightedColor = new Color(1f, 1f, 0.8f);
        shopColors.pressedColor     = new Color(0.7f, 0.7f, 0.5f);
        shopBtn.colors = shopColors;

        var shopLblGO = new GameObject("Label");
        shopLblGO.transform.SetParent(shopGO.transform, false);
        var shopLblRT = shopLblGO.AddComponent<RectTransform>();
        shopLblRT.anchorMin = Vector2.zero;
        shopLblRT.anchorMax = Vector2.one;
        shopLblRT.offsetMin = shopLblRT.offsetMax = Vector2.zero;
        var shopTM = shopLblGO.AddComponent<TextMeshProUGUI>();
        shopTM.text      = "SEEDS";
        shopTM.fontSize  = 18;
        shopTM.fontStyle = FontStyles.Bold;
        shopTM.color     = new Color(0.22f, 0.12f, 0.02f);
        shopTM.alignment = TextAlignmentOptions.Center;

        // ── 6. Build the right-side expandable panel ─────────────────
        // Remove old expandable panel if it exists
        var oldPanel = canvasGO.transform.Find("ExpandablePanel");
        if (oldPanel != null) Object.DestroyImmediate(oldPanel.gameObject);

        var panelGO = new GameObject("ExpandablePanel");
        panelGO.transform.SetParent(canvasGO.transform, false);
        var panelRT = panelGO.AddComponent<RectTransform>();
        // Anchored to right edge, above bottom bar
        panelRT.anchorMin        = new Vector2(1f, 0f);
        panelRT.anchorMax        = new Vector2(1f, 1f);
        panelRT.pivot            = new Vector2(1f, 0f);
        panelRT.anchoredPosition = new Vector2(210f, 60f); // starts hidden (210px off right)
        panelRT.sizeDelta        = new Vector2(210f, -60f);

        var panelImg = panelGO.AddComponent<Image>();
        panelImg.color = new Color(0.18f, 0.22f, 0.28f, 0.97f);
        var panelBgSpr = LoadSprite(SL_UI, "Sprite sheet for Basic Pack_0");
        if (panelBgSpr != null) { panelImg.sprite = panelBgSpr; panelImg.type = Image.Type.Sliced; }

        // ── 6a. Panel title bar ──────────────────────────────────────
        var titleBarGO = new GameObject("TitleBar");
        titleBarGO.transform.SetParent(panelGO.transform, false);
        var titleBarRT = titleBarGO.AddComponent<RectTransform>();
        titleBarRT.anchorMin = new Vector2(0f, 1f);
        titleBarRT.anchorMax = new Vector2(1f, 1f);
        titleBarRT.pivot     = new Vector2(0.5f, 1f);
        titleBarRT.anchoredPosition = Vector2.zero;
        titleBarRT.sizeDelta = new Vector2(0f, 36f);
        var titleBarImg = titleBarGO.AddComponent<Image>();
        titleBarImg.color = new Color(0.12f, 0.15f, 0.20f, 1f);

        var titleTxtGO = new GameObject("TitleText");
        titleTxtGO.transform.SetParent(titleBarGO.transform, false);
        var titleTxtRT = titleTxtGO.AddComponent<RectTransform>();
        titleTxtRT.anchorMin = new Vector2(0f, 0f);
        titleTxtRT.anchorMax = new Vector2(0.8f, 1f);
        titleTxtRT.offsetMin = new Vector2(8f, 0f);
        titleTxtRT.offsetMax = Vector2.zero;
        var titleTM = titleTxtGO.AddComponent<TextMeshProUGUI>();
        titleTM.text      = "SEEDS";
        titleTM.fontSize  = 16;
        titleTM.fontStyle = FontStyles.Bold;
        titleTM.color     = Color.white;
        titleTM.alignment = TextAlignmentOptions.MidlineLeft;

        // Close button
        var closeBtnGO = new GameObject("CloseButton");
        closeBtnGO.transform.SetParent(titleBarGO.transform, false);
        var closeBtnRT = closeBtnGO.AddComponent<RectTransform>();
        closeBtnRT.anchorMin        = new Vector2(1f, 0f);
        closeBtnRT.anchorMax        = new Vector2(1f, 1f);
        closeBtnRT.pivot            = new Vector2(1f, 0.5f);
        closeBtnRT.anchoredPosition = new Vector2(-4f, 0f);
        closeBtnRT.sizeDelta        = new Vector2(28f, -4f);
        var closeBtnImg = closeBtnGO.AddComponent<Image>();
        closeBtnImg.color = new Color(0.7f, 0.2f, 0.2f, 1f);
        var closeBtn = closeBtnGO.AddComponent<Button>();
        var closeTxtGO = new GameObject("X");
        closeTxtGO.transform.SetParent(closeBtnGO.transform, false);
        var closeTxtRT = closeTxtGO.AddComponent<RectTransform>();
        closeTxtRT.anchorMin = Vector2.zero; closeTxtRT.anchorMax = Vector2.one;
        closeTxtRT.offsetMin = closeTxtRT.offsetMax = Vector2.zero;
        var closeTM = closeTxtGO.AddComponent<TextMeshProUGUI>();
        closeTM.text = "✕"; closeTM.fontSize = 14; closeTM.color = Color.white;
        closeTM.alignment = TextAlignmentOptions.Center;

        // ── 6b. Tab buttons row ──────────────────────────────────────
        var tabRowGO = new GameObject("TabRow");
        tabRowGO.transform.SetParent(panelGO.transform, false);
        var tabRowRT = tabRowGO.AddComponent<RectTransform>();
        tabRowRT.anchorMin        = new Vector2(0f, 1f);
        tabRowRT.anchorMax        = new Vector2(1f, 1f);
        tabRowRT.pivot            = new Vector2(0.5f, 1f);
        tabRowRT.anchoredPosition = new Vector2(0f, -36f);
        tabRowRT.sizeDelta        = new Vector2(0f, 30f);

        Button seedsTabBtn = MakeTabButton(tabRowGO.transform, "SeedsTab", "Seeds", 0f, 0.333f);
        Button autoTabBtn  = MakeTabButton(tabRowGO.transform, "AutoTab",  "Auto",  0.333f, 0.667f);
        Button buildTabBtn = MakeTabButton(tabRowGO.transform, "BuildTab", "Build", 0.667f, 1f);

        // ── 6c. Content area ─────────────────────────────────────────
        var contentGO = new GameObject("ContentArea");
        contentGO.transform.SetParent(panelGO.transform, false);
        var contentRT = contentGO.AddComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0f, 0f);
        contentRT.anchorMax = new Vector2(1f, 1f);
        contentRT.offsetMin = new Vector2(0f, 0f);
        contentRT.offsetMax = new Vector2(0f, -66f); // below title+tabs
        contentGO.AddComponent<Image>().color = new Color(0.14f, 0.17f, 0.22f, 0.95f);

        // Seeds panel
        var seedsPanelGO = new GameObject("SeedsPanel");
        seedsPanelGO.transform.SetParent(contentGO.transform, false);
        seedsPanelGO.AddComponent<RectTransform>();
        StretchFull(seedsPanelGO);
        seedsPanelGO.AddComponent<Image>().color = new Color(0, 0, 0, 0);
        seedsPanelGO.AddComponent<CropShopUIController>();

        // Auto panel
        var autoPanelGO = new GameObject("AutoPanel");
        autoPanelGO.transform.SetParent(contentGO.transform, false);
        autoPanelGO.AddComponent<RectTransform>();
        StretchFull(autoPanelGO);
        autoPanelGO.AddComponent<Image>().color = new Color(0, 0, 0, 0);
        var autoPanel = autoPanelGO.AddComponent<AutoPanel>();

        // Auto panel content
        var autoLevelTxt = MakePanelText(autoPanelGO.transform, "LevelText",
            new Vector2(0f, 0.75f), Vector2.one, "Auto-Farmer: OFF", 13f, Color.white);
        var autoDescTxt = MakePanelText(autoPanelGO.transform, "DescText",
            new Vector2(0f, 0.45f), new Vector2(1f, 0.75f),
            "Automatically harvests\nready crops.", 10f, new Color(0.75f, 0.75f, 0.75f));
        var autoIntervalTxt = MakePanelText(autoPanelGO.transform, "IntervalText",
            new Vector2(0f, 0.30f), new Vector2(1f, 0.45f),
            "Buy to activate", 10f, new Color(0.6f, 0.8f, 0.6f));

        var autoUpgradeBtnGO = new GameObject("UpgradeButton");
        autoUpgradeBtnGO.transform.SetParent(autoPanelGO.transform, false);
        var autoUpgradeRT = autoUpgradeBtnGO.AddComponent<RectTransform>();
        autoUpgradeRT.anchorMin        = new Vector2(0.1f, 0.05f);
        autoUpgradeRT.anchorMax        = new Vector2(0.9f, 0.28f);
        autoUpgradeRT.offsetMin        = autoUpgradeRT.offsetMax = Vector2.zero;
        autoUpgradeBtnGO.AddComponent<Image>().color = new Color(0.15f, 0.45f, 0.25f);
        var autoUpgradeBtn = autoUpgradeBtnGO.AddComponent<Button>();
        var autoUpgradeTxtGO = new GameObject("Text");
        autoUpgradeTxtGO.transform.SetParent(autoUpgradeBtnGO.transform, false);
        StretchFull(autoUpgradeTxtGO);
        var autoUpgradeTM = autoUpgradeTxtGO.AddComponent<TextMeshProUGUI>();
        autoUpgradeTM.text = "Upgrade\n50 FP"; autoUpgradeTM.fontSize = 11;
        autoUpgradeTM.color = Color.white; autoUpgradeTM.alignment = TextAlignmentOptions.Center;

        autoPanel.levelText         = autoLevelTxt;
        autoPanel.descText          = autoDescTxt;
        autoPanel.intervalText      = autoIntervalTxt;
        autoPanel.upgradeButton     = autoUpgradeBtn;
        autoPanel.upgradeButtonText = autoUpgradeTM;
        EditorUtility.SetDirty(autoPanel);

        // Build panel
        var buildPanelGO = new GameObject("BuildPanel");
        buildPanelGO.transform.SetParent(contentGO.transform, false);
        buildPanelGO.AddComponent<RectTransform>();
        StretchFull(buildPanelGO);
        buildPanelGO.AddComponent<Image>().color = new Color(0, 0, 0, 0);
        var buildPanel = buildPanelGO.AddComponent<BuildPanel>();
        var buildTitleTxt = MakePanelText(buildPanelGO.transform, "TitleText",
            new Vector2(0f, 0.7f), Vector2.one, "Buildings", 14f, Color.white);
        var buildDescTxt = MakePanelText(buildPanelGO.transform, "DescText",
            Vector2.zero, new Vector2(1f, 0.7f),
            "Place decorations and\nupgrades on your farm.\n\nComing soon!", 10f,
            new Color(0.75f, 0.75f, 0.75f));
        buildPanel.titleText = buildTitleTxt;
        buildPanel.descText  = buildDescTxt;
        EditorUtility.SetDirty(buildPanel);

        // Start with seeds panel active, others hidden
        autoPanelGO.SetActive(false);
        buildPanelGO.SetActive(false);

        // ── 6d. TabMenuController ────────────────────────────────────
        var tabCtrl = panelGO.AddComponent<TabMenuController>();
        tabCtrl.seedsTab   = seedsTabBtn;
        tabCtrl.autoTab    = autoTabBtn;
        tabCtrl.buildTab   = buildTabBtn;
        tabCtrl.seedsPanel = seedsPanelGO;
        tabCtrl.autoPanel  = autoPanelGO;
        tabCtrl.buildPanel = buildPanelGO;
        EditorUtility.SetDirty(tabCtrl);

        // ── 7. Wire UIManager ─────────────────────────────────────────
        var uiMgr = canvasGO.GetComponent<UIManager>();
        if (uiMgr != null)
        {
            uiMgr.seedButton      = shopBtn;
            uiMgr.expandablePanel = panelRT;
            uiMgr.panelTitle      = titleTM;
            uiMgr.closeButton     = closeBtn;
            EditorUtility.SetDirty(uiMgr);
            Debug.Log("[FixUI] UIManager wired.");
        }

        // ── 8. Save scene ─────────────────────────────────────────────
        EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();

        Debug.Log("[FixUI] UI fixed and saved!");
    }

    // ── Helpers ──────────────────────────────────────────────────────

    static Button MakeTabButton(Transform parent, string name, string label,
                                float xMin, float xMax)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(xMin, 0f);
        rt.anchorMax = new Vector2(xMax, 1f);
        rt.offsetMin = new Vector2(1f, 1f);
        rt.offsetMax = new Vector2(-1f, -1f);
        var img = go.AddComponent<Image>();
        img.color = new Color(0.12f, 0.15f, 0.20f, 1f);
        var btn = go.AddComponent<Button>();
        var txtGO = new GameObject("Text");
        txtGO.transform.SetParent(go.transform, false);
        var txtRT = txtGO.AddComponent<RectTransform>();
        txtRT.anchorMin = Vector2.zero; txtRT.anchorMax = Vector2.one;
        txtRT.offsetMin = txtRT.offsetMax = Vector2.zero;
        var tm = txtGO.AddComponent<TextMeshProUGUI>();
        tm.text = label; tm.fontSize = 11; tm.fontStyle = FontStyles.Bold;
        tm.color = new Color(0.55f, 0.55f, 0.55f); tm.alignment = TextAlignmentOptions.Center;
        return btn;
    }

    static TextMeshProUGUI MakePanelText(Transform parent, string name,
        Vector2 ancMin, Vector2 ancMax, string text, float size, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = ancMin; rt.anchorMax = ancMax;
        rt.offsetMin = new Vector2(6f, 4f); rt.offsetMax = new Vector2(-6f, -4f);
        var tm = go.AddComponent<TextMeshProUGUI>();
        tm.text = text; tm.fontSize = size; tm.color = color;
        tm.alignment = TextAlignmentOptions.MidlineLeft;
        tm.raycastTarget = false;
        return tm;
    }

    static void StretchFull(GameObject go)
    {
        var rt = go.GetComponent<RectTransform>();
        if (rt == null) rt = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    static Sprite LoadSprite(string path, string name)
    {
        foreach (var obj in AssetDatabase.LoadAllAssetsAtPath(path))
            if (obj is Sprite s && s.name == name) return s;
        return null;
    }
}
