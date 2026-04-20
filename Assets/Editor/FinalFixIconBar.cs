using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class FinalFixIconBar
{
    static readonly string OBJ = "Assets/Sprout Lands - Sprites - Basic pack/Objects/";

    static Sprite GetSpr(string sheet, int r, int c)
    {
        string k = System.IO.Path.GetFileNameWithoutExtension(sheet) + "_" + r + "_" + c;
        foreach (var o in AssetDatabase.LoadAllAssetsAtPath(OBJ + sheet))
            if (o is Sprite s && s.name == k) return s;
        return null;
    }

    static Color C(string h) { ColorUtility.TryParseHtmlString(h, out Color c); return c; }

    public static void Execute()
    {
        var canvas = GameObject.Find("UICanvas");
        if (canvas == null) { Debug.LogError("UICanvas not found"); return; }

        // Delete and recreate
        var old = canvas.transform.Find("RightIconBar");
        if (old != null) Object.DestroyImmediate(old.gameObject);

        var bar = new GameObject("RightIconBar");
        bar.transform.SetParent(canvas.transform, false);
        var barRT = bar.AddComponent<RectTransform>();
        // Full height right strip, 52px wide, below 32px topbar
        barRT.anchorMin = new Vector2(1, 0);
        barRT.anchorMax = new Vector2(1, 1);
        barRT.pivot     = new Vector2(1, 0.5f);
        barRT.offsetMin = new Vector2(-52, 32);
        barRT.offsetMax = new Vector2(0, 0);

        var barImg = bar.AddComponent<Image>();
        barImg.color = C("#241408");

        var defs = new (string n, string sh, int r, int c, Color col)[]
        {
            ("SeedButton",    "Basic Plants.png",              0, 0, C("#2a7010")),
            ("BuildButton",   "Basic tools and meterials.png", 0, 2, C("#7a4010")),
            ("UpgradeButton", "Basic Grass Biom things 1.png", 0, 3, C("#103870")),
        };

        for (int i = 0; i < defs.Length; i++)
        {
            var (n, sh, r, c, col) = defs[i];
            var btn = new GameObject(n);
            btn.transform.SetParent(bar.transform, false);
            var bRT = btn.AddComponent<RectTransform>();
            bRT.anchorMin = new Vector2(0, 1);
            bRT.anchorMax = new Vector2(1, 1);
            bRT.pivot     = new Vector2(0.5f, 1);
            bRT.sizeDelta = new Vector2(0, 44);
            bRT.anchoredPosition = new Vector2(0, -(6 + i * 48f));

            // Image with the actual color we want
            var img = btn.AddComponent<Image>();
            img.color = col;

            // Button with normalColor=white so it doesn't modify the image color
            var button = btn.AddComponent<Button>();
            button.targetGraphic = img;
            var cs = ColorBlock.defaultColorBlock;
            cs.normalColor      = Color.white;          // white * col = col (correct!)
            cs.highlightedColor = new Color(1.3f,1.3f,0.8f); // brighter highlight
            cs.pressedColor     = new Color(0.7f,0.7f,0.7f);
            cs.selectedColor    = Color.white;
            cs.colorMultiplier  = 1f;
            button.colors = cs;

            // Outline
            var ol = btn.AddComponent<Outline>();
            ol.effectColor = new Color(0,0,0,0.6f);
            ol.effectDistance = new Vector2(1.5f,-1.5f);

            // Sprite icon
            var spr = GetSpr(sh, r, c);
            if (spr != null)
            {
                var ico = new GameObject("Icon");
                ico.transform.SetParent(btn.transform, false);
                var iRT = ico.AddComponent<RectTransform>();
                iRT.anchorMin = new Vector2(0.08f,0.08f);
                iRT.anchorMax = new Vector2(0.92f,0.92f);
                iRT.offsetMin = iRT.offsetMax = Vector2.zero;
                var iImg = ico.AddComponent<Image>();
                iImg.sprite = spr;
                iImg.preserveAspect = true;
                iImg.raycastTarget = false;
                Debug.Log($"  {n}: loaded sprite {spr.name}");
            }

            EditorUtility.SetDirty(btn);
        }

        // Rewire UIManager
        var uiMgr = canvas.GetComponent<UIManager>();
        if (uiMgr != null)
        {
            uiMgr.seedButton    = bar.transform.Find("SeedButton").GetComponent<Button>();
            uiMgr.buildButton   = bar.transform.Find("BuildButton").GetComponent<Button>();
            uiMgr.upgradeButton = bar.transform.Find("UpgradeButton").GetComponent<Button>();
            EditorUtility.SetDirty(canvas);
        }

        EditorUtility.SetDirty(bar);
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("[FinalFixIconBar] Done!");
    }
}
