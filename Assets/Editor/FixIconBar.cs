using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class FixIconBar
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

        // 删除旧IconBar，从头重建
        var old = canvas.transform.Find("RightIconBar");
        if (old != null) Object.DestroyImmediate(old.gameObject);

        var bar = new GameObject("RightIconBar");
        bar.transform.SetParent(canvas.transform, false);
        var barRT = bar.AddComponent<RectTransform>();

        // 贴右边，居中
        barRT.anchorMin = new Vector2(1, 0.5f);
        barRT.anchorMax = new Vector2(1, 0.5f);
        barRT.pivot     = new Vector2(1, 0.5f);
        barRT.sizeDelta = new Vector2(48, 168);
        barRT.anchoredPosition = Vector2.zero;

        var barImg = bar.AddComponent<Image>();
        barImg.color = Hex("#3a2008");

        // 3个按钮：固定大小42x42，垂直排列
        var defs = new (string name, string sheet, int r, int c, Color bg)[]
        {
            ("SeedButton",    "Basic Plants.png",              0, 0, Hex("#3a7a10")),
            ("BuildButton",   "Basic tools and meterials.png", 0, 2, Hex("#7a5010")),
            ("UpgradeButton", "Basic Grass Biom things 1.png", 0, 3, Hex("#104870")),
        };

        float btnW = 42f, btnH = 42f, gap = 3f, pad = 3f;
        float totalH = defs.Length * btnH + (defs.Length - 1) * gap + pad * 2;

        for (int i = 0; i < defs.Length; i++)
        {
            var (name, sheet, r, c, bg) = defs[i];
            var btn = new GameObject(name);
            btn.transform.SetParent(bar.transform, false);
            var bRT = btn.AddComponent<RectTransform>();

            // 固定大小，从顶部向下排列
            float yFromTop = pad + i * (btnH + gap);
            bRT.anchorMin = new Vector2(0.5f, 1f);
            bRT.anchorMax = new Vector2(0.5f, 1f);
            bRT.pivot     = new Vector2(0.5f, 1f);
            bRT.sizeDelta = new Vector2(btnW, btnH);
            bRT.anchoredPosition = new Vector2(0, -yFromTop);

            var bImg = btn.AddComponent<Image>();
            bImg.color = bg;

            var button = btn.AddComponent<Button>();
            var cs = button.colors;
            cs.normalColor      = Color.white;
            cs.highlightedColor = new Color(1f, 1f, 0.6f);
            cs.pressedColor     = new Color(0.7f, 0.7f, 0.7f);
            button.colors = cs;
            button.targetGraphic = bImg;

            var ol = btn.AddComponent<Outline>();
            ol.effectColor = Hex("#1a0c04");
            ol.effectDistance = new Vector2(1.5f, -1.5f);

            // Sprite icon — fill entire button
            var spr = GetSprite(sheet, r, c);
            if (spr != null)
            {
                var ico = new GameObject("Icon");
                ico.transform.SetParent(btn.transform, false);
                var iRT = ico.AddComponent<RectTransform>();
                iRT.anchorMin = new Vector2(0.08f, 0.08f);
                iRT.anchorMax = new Vector2(0.92f, 0.92f);
                iRT.offsetMin = iRT.offsetMax = Vector2.zero;
                var iImg = ico.AddComponent<Image>();
                iImg.sprite = spr;
                iImg.preserveAspect = true;
                iImg.raycastTarget  = false;
                Debug.Log($"  {name}: sprite={spr.name}");
            }
            else
            {
                Debug.LogWarning($"  {name}: sprite NOT found!");
            }

            EditorUtility.SetDirty(btn);
        }

        // 重新接线 UIManager
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
        Debug.Log("[FixIconBar] Done!");
    }
}
