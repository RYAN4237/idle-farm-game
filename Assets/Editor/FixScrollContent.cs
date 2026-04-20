using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class FixScrollContent
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

    static GameObject MakeGO(string n, Transform p)
    { var g=new GameObject(n); g.transform.SetParent(p,false); g.AddComponent<RectTransform>(); return g; }

    static RectTransform Fill(GameObject g)
    { var rt=g.GetComponent<RectTransform>(); rt.anchorMin=Vector2.zero; rt.anchorMax=Vector2.one; rt.offsetMin=rt.offsetMax=Vector2.zero; return rt; }

    static TextMeshProUGUI TMP(string n, Transform p, string text, float sz, Color col, bool bold=false, TextAlignmentOptions align=TextAlignmentOptions.Center)
    {
        var g=MakeGO(n,p); Fill(g);
        var t=g.AddComponent<TextMeshProUGUI>();
        t.text=text; t.fontSize=sz; t.color=col;
        t.fontStyle=bold?FontStyles.Bold:FontStyles.Normal;
        t.alignment=align; t.enableWordWrapping=false; t.overflowMode=TextOverflowModes.Truncate;
        return t;
    }

    public static void Execute()
    {
        var canvas = GameObject.Find("UICanvas");
        var scrollView = canvas?.transform.Find("ExpandablePanel/ScrollView");
        if (scrollView == null) { Debug.LogError("ScrollView not found"); return; }

        // Fix Viewport: stretch to fill scrollview minus scrollbar
        var viewport = scrollView.Find("Viewport");
        if (viewport != null)
        {
            var vRT = viewport.GetComponent<RectTransform>();
            vRT.anchorMin = Vector2.zero; vRT.anchorMax = Vector2.one;
            vRT.offsetMin = Vector2.zero; vRT.offsetMax = new Vector2(-8,0);
            EditorUtility.SetDirty(viewport.gameObject);
        }

        // Nuke and rebuild Content
        var content = viewport?.Find("Content");
        if (content == null) { Debug.LogError("Content not found"); return; }

        for (int i = content.childCount-1; i >= 0; i--)
            Object.DestroyImmediate(content.GetChild(i).gameObject);

        var cRT = content.GetComponent<RectTransform>();
        cRT.anchorMin = new Vector2(0,1); cRT.anchorMax = new Vector2(1,1);
        cRT.pivot     = new Vector2(0,1);
        cRT.offsetMin = cRT.offsetMax = Vector2.zero;

        foreach (var c2 in content.GetComponents<Component>())
        {
            if (c2 is RectTransform || c2 is CanvasRenderer) continue;
            if (c2 is ContentSizeFitter || c2 is VerticalLayoutGroup) Object.DestroyImmediate(c2);
        }

        var vlg = content.gameObject.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(2,2,2,2); vlg.spacing = 2;
        vlg.childForceExpandWidth=true; vlg.childForceExpandHeight=false;
        vlg.childControlWidth=true; vlg.childControlHeight=true;

        var csf = content.gameObject.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // SeedGrid
        var grid = MakeGO("SeedGrid", content);
        var glg = grid.AddComponent<GridLayoutGroup>();
        glg.cellSize = new Vector2(50,58); glg.spacing = new Vector2(3,3);
        glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        glg.constraintCount = 3; glg.childAlignment = TextAnchor.UpperLeft;
        glg.padding = new RectOffset(2,2,2,2);
        grid.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        grid.AddComponent<LayoutElement>().flexibleWidth = 1;

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
            var cell = MakeGO("Cell_"+n, grid.transform);
            cell.AddComponent<Image>().color = lk ? C("#807060") : C("#ddc898");
            var ol = cell.AddComponent<Outline>();
            ol.effectColor = C("#8a6830"); ol.effectDistance = new Vector2(1,-1);
            var scb = cell.AddComponent<SeedCellButton>();
            scb.seedName=n; scb.seedCost=cost; scb.isLocked=lk;

            // Badge
            var bdg = MakeGO("Badge", cell.transform);
            var bdgRT = bdg.GetComponent<RectTransform>();
            bdgRT.anchorMin=new Vector2(1,1); bdgRT.anchorMax=new Vector2(1,1);
            bdgRT.pivot=new Vector2(1,1); bdgRT.sizeDelta=new Vector2(16,11);
            bdgRT.anchoredPosition=new Vector2(-1,-1);
            bdg.AddComponent<Image>().color = lk ? C("#605040") : C("#287010");
            TMP("T", bdg.transform, cnt.ToString(), 7, Color.white, true);

            // Sprite
            if (i < plantIcons.Length && !lk)
            {
                var spr = Spr(plantIcons[i].sh, plantIcons[i].r, plantIcons[i].c);
                if (spr != null)
                {
                    var ico = MakeGO("Ico", cell.transform);
                    var irt = ico.GetComponent<RectTransform>();
                    irt.anchorMin=new Vector2(0.08f,0.36f);
                    irt.anchorMax=new Vector2(0.92f,0.82f);
                    irt.offsetMin=irt.offsetMax=Vector2.zero;
                    var ii = ico.AddComponent<Image>(); ii.sprite=spr; ii.preserveAspect=true; ii.raycastTarget=false;
                }
            }
            else if (lk)
            {
                var lkGO = MakeGO("Lock", cell.transform); Fill(lkGO);
                lkGO.AddComponent<Image>().color = new Color(0,0,0,0.3f);
                TMP("T", lkGO.transform, "LOCK", 8, Color.white, true);
            }

            // Name + Cost
            var nm = MakeGO("Name", cell.transform);
            nm.GetComponent<RectTransform>().anchorMin=new Vector2(0,0.18f);
            nm.GetComponent<RectTransform>().anchorMax=new Vector2(1,0.38f);
            TMP("T", nm.transform, n, 6, lk?C("#706050"):C("#3a2808"), true);

            var co = MakeGO("Cost", cell.transform);
            co.GetComponent<RectTransform>().anchorMin=new Vector2(0,0.02f);
            co.GetComponent<RectTransform>().anchorMax=new Vector2(1,0.20f);
            TMP("T", co.transform, "$"+cost, 6, lk?C("#706050"):C("#7a4c10"));

            EditorUtility.SetDirty(cell);
        }

        // Info bar
        var info = MakeGO("InfoBar", content);
        info.AddComponent<LayoutElement>().preferredHeight = 22;
        info.AddComponent<Image>().color = C("#1e4808");
        TMP("T", info.transform, "Select a seed  ·  click grid to plant", 8, C("#a0e860"));

        EditorUtility.SetDirty(content.gameObject);
        EditorUtility.SetDirty(scrollView.gameObject);
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("[FixScrollContent] Done! 12 seed cells rebuilt with sprites.");
    }
}
