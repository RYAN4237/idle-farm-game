using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

/// Simple fix: set Content to fixed height, no ContentSizeFitter issues
public class FixScrollSimple
{
    static readonly string OBJ = "Assets/Sprout Lands - Sprites - Basic pack/Objects/";
    static Color C(string h) { ColorUtility.TryParseHtmlString(h, out Color c); return c; }

    static Sprite Spr(string sheet, int r, int col)
    {
        string k = System.IO.Path.GetFileNameWithoutExtension(sheet) + "_" + r + "_" + col;
        foreach (var o in AssetDatabase.LoadAllAssetsAtPath(OBJ + sheet))
            if (o is Sprite s && s.name == k) return s;
        return null;
    }

    static GameObject G(string n, Transform p) { var g=new GameObject(n); g.transform.SetParent(p,false); g.AddComponent<RectTransform>(); return g; }
    static void Fill(GameObject g) { var rt=g.GetComponent<RectTransform>(); rt.anchorMin=Vector2.zero; rt.anchorMax=Vector2.one; rt.offsetMin=rt.offsetMax=Vector2.zero; }

    static TextMeshProUGUI MkTxt(string n, Transform p, string txt, float sz, Color c, bool bold=false)
    {
        var g=G(n,p); Fill(g);
        var t=g.AddComponent<TextMeshProUGUI>();
        t.text=txt; t.fontSize=sz; t.color=c;
        t.fontStyle=bold?FontStyles.Bold:FontStyles.Normal;
        t.alignment=TextAlignmentOptions.Center;
        t.enableWordWrapping=false; t.overflowMode=TextOverflowModes.Truncate;
        return t;
    }

    public static void Execute()
    {
        var canvas  = GameObject.Find("UICanvas");
        var scrollGO = canvas?.transform.Find("ExpandablePanel/ScrollView")?.gameObject;
        if (scrollGO == null) { Debug.LogError("ScrollView not found"); return; }

        // ── Completely nuke ScrollView children and rebuild ────────────
        for (int i = scrollGO.transform.childCount-1; i >= 0; i--)
            Object.DestroyImmediate(scrollGO.transform.GetChild(i).gameObject);

        // Remove old ScrollRect
        var oldSR = scrollGO.GetComponent<ScrollRect>();
        if (oldSR != null) Object.DestroyImmediate(oldSR);

        // ── Viewport (mask, fills scrollview minus scrollbar) ──────────
        var vp = G("Viewport", scrollGO.transform);
        var vpRT = vp.GetComponent<RectTransform>();
        vpRT.anchorMin=Vector2.zero; vpRT.anchorMax=Vector2.one;
        vpRT.offsetMin=Vector2.zero; vpRT.offsetMax=new Vector2(-8,0);
        vp.AddComponent<Image>().color = new Color(0,0,0,0);
        vp.AddComponent<Mask>().showMaskGraphic = false;

        // ── Content (fixed tall height - holds all cells) ──────────────
        // 4 rows × 61px (58+3) + 2 padding = 246px
        // Plus InfoBar 24px = 270px total
        float contentH = 270f;
        var ct = G("Content", vp.transform);
        var ctRT = ct.GetComponent<RectTransform>();
        ctRT.anchorMin = new Vector2(0,1); ctRT.anchorMax = new Vector2(1,1);
        ctRT.pivot     = new Vector2(0,1);
        ctRT.sizeDelta = new Vector2(0, contentH);
        ctRT.anchoredPosition = Vector2.zero;

        // ── SeedGrid (GridLayoutGroup, fixed size) ─────────────────────
        var grid = G("SeedGrid", ct.transform);
        var gRT  = grid.GetComponent<RectTransform>();
        gRT.anchorMin = new Vector2(0,1); gRT.anchorMax = new Vector2(1,1);
        gRT.pivot     = new Vector2(0,1);
        gRT.sizeDelta = new Vector2(0, 245f);
        gRT.anchoredPosition = Vector2.zero;

        var glg = grid.AddComponent<GridLayoutGroup>();
        glg.cellSize = new Vector2(50,58);
        glg.spacing  = new Vector2(3,3);
        glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        glg.constraintCount = 3;
        glg.childAlignment = TextAnchor.UpperLeft;
        glg.padding = new RectOffset(2,2,2,2);

        // ── InfoBar ────────────────────────────────────────────────────
        var infoGO = G("InfoBar", ct.transform);
        var iRT    = infoGO.GetComponent<RectTransform>();
        iRT.anchorMin = new Vector2(0,1); iRT.anchorMax = new Vector2(1,1);
        iRT.pivot     = new Vector2(0,1);
        iRT.sizeDelta = new Vector2(0, 22f);
        iRT.anchoredPosition = new Vector2(0, -246f);
        infoGO.AddComponent<Image>().color = C("#1e4808");
        MkTxt("T", infoGO.transform, "Select a seed  ·  click to plant", 8, C("#a0e860"));

        // ── Seeds ──────────────────────────────────────────────────────
        var plantIcons = new (string sh,int r,int c)[]
        {
            ("Basic Plants.png",0,0),("Basic Plants.png",0,1),("Basic Plants.png",0,2),
            ("Basic Plants.png",0,3),("Basic Plants.png",0,4),("Basic Plants.png",0,5),
            ("Basic Plants.png",1,0),("Basic Plants.png",1,1),("Basic Plants.png",1,2),
            ("Basic Plants.png",1,3),("Basic Plants.png",1,4),("Basic Plants.png",1,5),
        };
        var seeds = new (string n,int cost,int cnt,bool lk)[]
        {
            ("Wheat",10,99,false),("Carrot",15,8,false),("Beet",20,5,false),
            ("Turnip",18,3,false),("Pumpkin",40,2,false),("Corn",30,4,false),
            ("Wheat+",50,1,false),("Carrot+",60,0,false),("Beet+",80,99,false),
            ("Turnip+",120,4,false),("Shroom",200,0,true),("Dragon",500,0,true),
        };

        for (int i = 0; i < seeds.Length; i++)
        {
            var (n,cost,cnt,lk) = seeds[i];
            var cell = G("Cell_"+n, grid.transform);
            cell.AddComponent<Image>().color = lk ? C("#807060") : C("#ddc898");
            var ol = cell.AddComponent<Outline>();
            ol.effectColor=C("#8a6830"); ol.effectDistance=new Vector2(1,-1);
            var scb = cell.AddComponent<SeedCellButton>();
            scb.seedName=n; scb.seedCost=cost; scb.isLocked=lk;

            // Badge (count, top-right)
            var bdg = G("Badge",cell.transform);
            var bRT = bdg.GetComponent<RectTransform>();
            bRT.anchorMin=new Vector2(1,1); bRT.anchorMax=new Vector2(1,1);
            bRT.pivot=new Vector2(1,1); bRT.sizeDelta=new Vector2(16,11);
            bRT.anchoredPosition=new Vector2(-1,-1);
            bdg.AddComponent<Image>().color = lk?C("#605040"):C("#287010");
            MkTxt("T",bdg.transform,cnt.ToString(),7,Color.white,true);

            // Sprite icon
            if (i < plantIcons.Length && !lk)
            {
                var spr = Spr(plantIcons[i].sh, plantIcons[i].r, plantIcons[i].c);
                if (spr != null)
                {
                    var ico = G("Ico",cell.transform);
                    var irt = ico.GetComponent<RectTransform>();
                    irt.anchorMin=new Vector2(0.08f,0.36f);
                    irt.anchorMax=new Vector2(0.92f,0.82f);
                    irt.offsetMin=irt.offsetMax=Vector2.zero;
                    var ii=ico.AddComponent<Image>(); ii.sprite=spr; ii.preserveAspect=true; ii.raycastTarget=false;
                }
            }
            else if (lk)
            {
                var lkGO=G("Lock",cell.transform); Fill(lkGO);
                lkGO.AddComponent<Image>().color=new Color(0,0,0,0.3f);
                MkTxt("T",lkGO.transform,"LOCK",8,Color.white,true);
            }

            // Name
            var nm=G("Name",cell.transform);
            nm.GetComponent<RectTransform>().anchorMin=new Vector2(0,0.18f);
            nm.GetComponent<RectTransform>().anchorMax=new Vector2(1,0.38f);
            MkTxt("T",nm.transform,n,6,lk?C("#706050"):C("#3a2808"),true);

            // Cost
            var co=G("Cost",cell.transform);
            co.GetComponent<RectTransform>().anchorMin=new Vector2(0,0.02f);
            co.GetComponent<RectTransform>().anchorMax=new Vector2(1,0.20f);
            MkTxt("T",co.transform,"$"+cost,6,lk?C("#706050"):C("#7a4c10"));
        }

        // ── Scrollbar (right side) ─────────────────────────────────────
        var sb = G("Scrollbar", scrollGO.transform);
        var sbRT = sb.GetComponent<RectTransform>();
        sbRT.anchorMin=new Vector2(1,0); sbRT.anchorMax=new Vector2(1,1);
        sbRT.pivot=new Vector2(1,0.5f); sbRT.sizeDelta=new Vector2(8,0); sbRT.anchoredPosition=Vector2.zero;
        sb.AddComponent<Image>().color = C("#2a1a08");

        var handle = G("Handle", sb.transform);
        var hRT    = handle.GetComponent<RectTransform>();
        hRT.anchorMin=Vector2.zero; hRT.anchorMax=Vector2.one;
        hRT.offsetMin=new Vector2(1,0); hRT.offsetMax=new Vector2(-1,0);
        var hImg = handle.AddComponent<Image>(); hImg.color = C("#8a6820");

        var scrollbar = sb.AddComponent<Scrollbar>();
        scrollbar.direction = Scrollbar.Direction.BottomToTop;
        scrollbar.handleRect = hRT;
        scrollbar.targetGraphic = hImg;

        // ── ScrollRect on ScrollView ───────────────────────────────────
        var sr = scrollGO.AddComponent<ScrollRect>();
        sr.horizontal = false; sr.vertical = true;
        sr.viewport  = vpRT;
        sr.content   = ctRT;
        sr.verticalScrollbar = scrollbar;
        sr.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
        sr.scrollSensitivity = 30f;
        sr.movementType = ScrollRect.MovementType.Clamped;
        sr.inertia = false;

        EditorUtility.SetDirty(scrollGO);
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("[FixScrollSimple] Done! Fixed height content, no CSF issues.");
    }
}
