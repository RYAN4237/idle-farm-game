using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

/// Rebuilds the right shop section as a tab menu:
/// Tab bar: [Seeds] [Auto] [Build]
/// Panel area: switches content per tab
public class BuildTabMenu
{
    // These match FinalBarBuild proportions
    const float poX0 = 0.8188f;  // pomo section start
    const float poX1 = 1.0f;
    const float shopX0 = 0.5375f;
    const float shopX1 = 0.8177f;
    const float afX0   = 0.4594f;
    const float afX1   = 0.5365f;

    public static void Execute()
    {
        var canvas = GameObject.Find("UICanvas");
        if (canvas == null) { Debug.LogError("UICanvas not found"); return; }

        // Remove old shop + autofarmer elements
        Destroy(canvas, "CropShopPanel");
        Destroy(canvas, "AutoFarmerBtn");

        // ── Create RightMenu container (shop area + autofarmer area combined) ──
        // Spans from afX0 to shopX1
        float menuX0 = afX0;
        float menuX1 = shopX1;

        var menuGO = new GameObject("RightMenu");
        menuGO.transform.SetParent(canvas.transform, false);
        AR(menuGO, menuX0, 0f, menuX1, 1f);
        menuGO.AddComponent<Image>().color = new Color(0.07f, 0.09f, 0.11f, 1f);

        // ── Tab Bar (top 22%) ──
        var tabBarGO = new GameObject("TabBar");
        tabBarGO.transform.SetParent(menuGO.transform, false);
        AR(tabBarGO, 0f, 0.78f, 1f, 1f);
        tabBarGO.AddComponent<Image>().color = new Color(0.05f, 0.07f, 0.09f, 1f);

        float tw = 1f / 3f;
        var seedsTabGO = MakeTabBtn(tabBarGO.transform, "SeedsTab",  "Seeds",  "🌾", 0f,   tw);
        var autoTabGO  = MakeTabBtn(tabBarGO.transform, "AutoTab",   "Auto",   "🤖", tw,   tw*2);
        var buildTabGO = MakeTabBtn(tabBarGO.transform, "BuildTab",  "Build",  "🏗", tw*2, 1f);

        // Tab dividers
        MakeDiv(tabBarGO.transform, tw);
        MakeDiv(tabBarGO.transform, tw*2);

        // ── Panel Area (bottom 78%) ──
        var panelAreaGO = new GameObject("PanelArea");
        panelAreaGO.transform.SetParent(menuGO.transform, false);
        AR(panelAreaGO, 0f, 0f, 1f, 0.78f);
        panelAreaGO.AddComponent<Image>().color = Color.clear;

        // Seeds Panel
        var seedsPanel = BuildSeedsPanel(panelAreaGO.transform);

        // Auto Panel
        var autoPanel = BuildAutoPanel(panelAreaGO.transform);

        // Build Panel
        var buildPanel = BuildBuildPanel(panelAreaGO.transform);

        // ── TabMenuController ──
        var tabCtrl = menuGO.AddComponent<TabMenuController>();
        tabCtrl.seedsTab   = seedsTabGO.GetComponent<Button>();
        tabCtrl.autoTab    = autoTabGO.GetComponent<Button>();
        tabCtrl.buildTab   = buildTabGO.GetComponent<Button>();
        tabCtrl.seedsPanel = seedsPanel;
        tabCtrl.autoPanel  = autoPanel;
        tabCtrl.buildPanel = buildPanel;
        EditorUtility.SetDirty(menuGO);

        // ── Wire CropShopUIController into seeds panel ──
        var cropUI = seedsPanel.AddComponent<CropShopUIController>();
        EditorUtility.SetDirty(seedsPanel);

        // ── Wire AutoPanel ──
        var autoScript = autoPanel.AddComponent<AutoPanel>();
        // Find sub-elements
        autoScript.levelText        = FindDeep<TextMeshProUGUI>(autoPanel.transform, "LevelText");
        autoScript.descText         = FindDeep<TextMeshProUGUI>(autoPanel.transform, "DescText");
        autoScript.intervalText     = FindDeep<TextMeshProUGUI>(autoPanel.transform, "IntervalText");
        autoScript.upgradeButton    = FindDeep<Button>(autoPanel.transform, "UpgradeBtn");
        autoScript.upgradeButtonText= FindDeep<Button>(autoPanel.transform, "UpgradeBtn")
                                        ?.GetComponentInChildren<TextMeshProUGUI>();
        EditorUtility.SetDirty(autoPanel);

        // ── Wire BuildPanel ──
        var buildScript = buildPanel.AddComponent<BuildPanel>();
        buildScript.titleText = FindDeep<TextMeshProUGUI>(buildPanel.transform, "TitleText");
        buildScript.descText  = FindDeep<TextMeshProUGUI>(buildPanel.transform, "DescText");
        EditorUtility.SetDirty(buildPanel);

        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("BuildTabMenu complete + saved!");
    }

    // ── Seeds Panel: crop shop grid ──
    static GameObject BuildSeedsPanel(Transform parent)
    {
        var go = new GameObject("SeedsPanel");
        go.transform.SetParent(parent, false);
        AR(go, 0f, 0f, 1f, 1f);
        go.AddComponent<Image>().color = Color.clear;
        // Title
        SubTxt(go.transform, "ShopTitle", 0f, 0.90f, 1f, 1f, "SEEDS", 8f, new Color(0.5f,0.5f,0.5f));
        return go;
    }

    // ── Auto Panel: autofarmer info + upgrade ──
    static GameObject BuildAutoPanel(Transform parent)
    {
        var go = new GameObject("AutoPanel");
        go.transform.SetParent(parent, false);
        AR(go, 0f, 0f, 1f, 1f);
        go.AddComponent<Image>().color = Color.clear;

        // Level text
        SubTxt(go.transform, "LevelText",    0f,0.80f,1f,0.96f, "Auto-Farmer: OFF", 10f, Color.white);
        // Description
        SubTxt(go.transform, "DescText",     0f,0.54f,1f,0.80f,
            "Auto harvests ready crops\nand replants empty plots.", 8f, new Color(0.65f,0.65f,0.65f));
        // Interval
        SubTxt(go.transform, "IntervalText", 0f,0.40f,1f,0.54f, "Not active", 8f, new Color(0.2f,0.85f,0.7f));

        // FP display
        SubTxt(go.transform, "FPDisplay",    0f,0.28f,1f,0.40f, "FP: 0", 8f, new Color(1f,0.9f,0.4f));

        // Upgrade button
        var btnGO = new GameObject("UpgradeBtn");
        btnGO.transform.SetParent(go.transform, false);
        var br = btnGO.AddComponent<RectTransform>();
        br.anchorMin=new Vector2(0.05f,0.04f); br.anchorMax=new Vector2(0.95f,0.26f);
        br.offsetMin=Vector2.zero; br.offsetMax=Vector2.zero;
        br.anchoredPosition=Vector2.zero; br.sizeDelta=Vector2.zero;
        var bImg=btnGO.AddComponent<Image>(); bImg.color=new Color(0.15f,0.28f,0.45f,1f);
        var btn=btnGO.AddComponent<Button>(); btn.targetGraphic=bImg;
        var bc=btn.colors;
        bc.highlightedColor=new Color(0.22f,0.38f,0.58f,1f);
        bc.pressedColor    =new Color(0.10f,0.20f,0.32f,1f);
        btn.colors=bc;
        var bTxt=new GameObject("Text"); bTxt.transform.SetParent(btnGO.transform,false);
        var bTr=bTxt.AddComponent<RectTransform>();
        bTr.anchorMin=Vector2.zero; bTr.anchorMax=Vector2.one;
        bTr.offsetMin=Vector2.zero; bTr.offsetMax=Vector2.zero;
        var bTMP=bTxt.AddComponent<TextMeshProUGUI>();
        bTMP.text="Buy Auto-Farmer\n200 FP"; bTMP.fontSize=9f;
        bTMP.color=Color.white; bTMP.alignment=TextAlignmentOptions.Center;
        bTMP.raycastTarget=false;

        return go;
    }

    // ── Build Panel: placeholder ──
    static GameObject BuildBuildPanel(Transform parent)
    {
        var go = new GameObject("BuildPanel");
        go.transform.SetParent(parent, false);
        AR(go, 0f, 0f, 1f, 1f);
        go.AddComponent<Image>().color = Color.clear;
        SubTxt(go.transform, "TitleText", 0f,0.72f,1f,0.92f, "Buildings", 11f, Color.white);
        SubTxt(go.transform, "DescText",  0f,0.30f,1f,0.72f,
            "Place decorations &\nupgrades on your farm.\n\nComing soon!", 8f, new Color(0.6f,0.6f,0.6f));
        // Lock icon
        SubTxt(go.transform, "LockIcon",  0f,0.52f,1f,0.72f, "🔒", 20f, new Color(0.5f,0.5f,0.5f,0.5f));
        return go;
    }

    // ── Helpers ──────────────────────────────────────────
    static GameObject MakeTabBtn(Transform parent, string name, string label, string icon,
        float x0, float x1)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var r = go.AddComponent<RectTransform>();
        r.anchorMin=new Vector2(x0,0f); r.anchorMax=new Vector2(x1,1f);
        r.offsetMin=Vector2.zero; r.offsetMax=Vector2.zero;
        r.anchoredPosition=Vector2.zero; r.sizeDelta=Vector2.zero;

        var img = go.AddComponent<Image>();
        img.color = new Color(0.12f,0.15f,0.20f,1f);
        var btn = go.AddComponent<Button>(); btn.targetGraphic=img;
        var cs = btn.colors;
        cs.highlightedColor=new Color(0.18f,0.22f,0.28f,1f);
        cs.pressedColor    =new Color(0.10f,0.13f,0.16f,1f);
        btn.colors=cs;

        // Icon + label stacked
        var txtGO = new GameObject("Text"); txtGO.transform.SetParent(go.transform,false);
        var tr = txtGO.AddComponent<RectTransform>();
        tr.anchorMin=Vector2.zero; tr.anchorMax=Vector2.one;
        tr.offsetMin=Vector2.zero; tr.offsetMax=Vector2.zero;
        var tmp = txtGO.AddComponent<TextMeshProUGUI>();
        tmp.text = label; // No emoji (font doesn't support)
        tmp.fontSize=9f; tmp.color=new Color(0.55f,0.55f,0.55f,1f);
        tmp.alignment=TextAlignmentOptions.Center; tmp.raycastTarget=false;

        return go;
    }

    static void MakeDiv(Transform parent, float x)
    {
        var go = new GameObject("Div"); go.transform.SetParent(parent,false);
        var r = go.AddComponent<RectTransform>();
        r.anchorMin=new Vector2(x,0f); r.anchorMax=new Vector2(x+0.002f,1f);
        r.offsetMin=Vector2.zero; r.offsetMax=Vector2.zero;
        go.AddComponent<Image>().color=new Color(0.20f,0.25f,0.30f,1f);
    }

    static void AR(GameObject go, float ax, float ay, float bx, float by)
    {
        var r = go.GetComponent<RectTransform>();
        if (r==null) r=go.AddComponent<RectTransform>();
        r.anchorMin=new Vector2(ax,ay); r.anchorMax=new Vector2(bx,by);
        r.offsetMin=Vector2.zero; r.offsetMax=Vector2.zero;
        r.anchoredPosition=Vector2.zero; r.sizeDelta=Vector2.zero;
        EditorUtility.SetDirty(go);
    }

    static void SubTxt(Transform parent, string name, float ax, float ay, float bx, float by,
        string text, float size, Color col)
    {
        var go=new GameObject(name); go.transform.SetParent(parent,false);
        AR(go, ax, ay, bx, by);
        var tmp=go.AddComponent<TextMeshProUGUI>();
        tmp.text=text; tmp.fontSize=size; tmp.color=col;
        tmp.alignment=TextAlignmentOptions.Center; tmp.raycastTarget=false;
    }

    static T FindDeep<T>(Transform root, string name) where T:Component
    {
        if(root.name==name){var c=root.GetComponent<T>();if(c!=null)return c;}
        foreach(Transform ch in root){var f=FindDeep<T>(ch,name);if(f!=null)return f;}
        return null;
    }

    static void Destroy(GameObject parent, string name)
    {
        var t=parent.transform.Find(name);
        if(t!=null) Object.DestroyImmediate(t.gameObject);
    }
}
