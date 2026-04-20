using UnityEngine;
using UnityEditor;
using TMPro;
using UnityEngine.UI;

public class AddSceneDecor
{
    public static void Execute()
    {
        var canvas = GameObject.Find("UICanvas");
        if (canvas == null) { Debug.LogError("UICanvas not found"); return; }

        var uiSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

        // ── 1. Clouds (3 white rounded squares in sky area) ──
        RemoveChild(canvas, "Clouds");
        var cloudsGO = new GameObject("Clouds");
        cloudsGO.transform.SetParent(canvas.transform, false);
        cloudsGO.AddComponent<RectTransform>();

        // Place clouds in left 70%, top 60% of screen
        var cloudData = new (float ax, float ay, float bx, float by, float alpha)[]
        {
            (0.04f, 0.72f, 0.20f, 0.84f, 0.85f),
            (0.25f, 0.78f, 0.38f, 0.88f, 0.75f),
            (0.48f, 0.70f, 0.62f, 0.82f, 0.80f),
        };

        foreach (var c in cloudData)
        {
            var cloudGO = new GameObject("Cloud");
            cloudGO.transform.SetParent(cloudsGO.transform, false);
            cloudGO.AddComponent<RectTransform>();

            var img   = cloudGO.AddComponent<Image>();
            img.sprite = uiSprite;
            img.color  = new Color(1f, 1f, 1f, c.alpha);
            img.raycastTarget = false;

            var r = cloudGO.GetComponent<RectTransform>();
            r.anchorMin = new Vector2(c.ax, c.ay);
            r.anchorMax = new Vector2(c.bx, c.by);
            r.offsetMin = Vector2.zero;
            r.offsetMax = Vector2.zero;
        }

        // Move clouds behind other UI elements
        cloudsGO.transform.SetSiblingIndex(2);
        EditorUtility.SetDirty(cloudsGO);

        // ── 2. Back row plots: slightly darker for depth ──
        var backPlotColor = new Color(0.38f, 0.26f, 0.12f, 1f); // darker dirt
        for (int i = 1; i <= 3; i++)
        {
            var go = GameObject.Find("FarmPlot_" + i);
            if (go == null) continue;
            var plot = go.GetComponent<FarmPlot>();
            if (plot != null) plot.emptyColor = backPlotColor;
            var sr = go.GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = backPlotColor;
            EditorUtility.SetDirty(go);
        }

        // ── 3. Grass: add a darker mid-strip for depth ──
        RemoveChild(canvas, "GrassMid");
        var grassMid = new GameObject("GrassMid");
        grassMid.transform.SetParent(canvas.transform, false);
        grassMid.AddComponent<RectTransform>();
        var gmImg = grassMid.AddComponent<Image>();
        gmImg.color = new Color(0.22f, 0.40f, 0.10f, 1f);
        gmImg.raycastTarget = false;
        var gmRect = grassMid.GetComponent<RectTransform>();
        gmRect.anchorMin = new Vector2(0f, 0.10f);
        gmRect.anchorMax = new Vector2(0.70f, 0.18f);
        gmRect.offsetMin = Vector2.zero;
        gmRect.offsetMax = Vector2.zero;
        grassMid.transform.SetSiblingIndex(1);
        EditorUtility.SetDirty(grassMid);

        // ── 4. Divider line between farm and timer panel ──
        RemoveChild(canvas, "PanelDivider");
        var divider = new GameObject("PanelDivider");
        divider.transform.SetParent(canvas.transform, false);
        divider.AddComponent<RectTransform>();
        var divImg = divider.AddComponent<Image>();
        divImg.color = new Color(0.25f, 0.28f, 0.32f, 1f);
        divImg.raycastTarget = false;
        var divRect = divider.GetComponent<RectTransform>();
        divRect.anchorMin = new Vector2(0.70f, 0f);
        divRect.anchorMax = new Vector2(0.702f, 1f);
        divRect.offsetMin = Vector2.zero;
        divRect.offsetMax = Vector2.zero;
        EditorUtility.SetDirty(divider);

        // ── 5. Save ──
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("AddSceneDecor complete + saved!");
    }

    static void RemoveChild(GameObject parent, string name)
    {
        var t = parent.transform.Find(name);
        if (t != null) Object.DestroyImmediate(t.gameObject);
    }
}
