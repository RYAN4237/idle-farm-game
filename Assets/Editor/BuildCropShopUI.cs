using UnityEngine;
using UnityEditor;
using TMPro;
using UnityEngine.UI;

public class BuildCropShopUI
{
    public static void Execute()
    {
        var canvas     = GameObject.Find("UICanvas");
        var rightPanel = canvas?.transform.Find("RightPanel");
        if (rightPanel == null) { Debug.LogError("RightPanel not found"); return; }

        // ── Remove old StatsPanel area, push stats up ──
        var stats = rightPanel.Find("TopRightPanel");

        // ── Create CropShopPanel at bottom of RightPanel ──
        var oldShop = rightPanel.Find("CropShopPanel");
        if (oldShop != null) Object.DestroyImmediate(oldShop.gameObject);

        var shopGO = new GameObject("CropShopPanel");
        shopGO.transform.SetParent(rightPanel, false);

        var shopRect = shopGO.AddComponent<RectTransform>();
        shopRect.anchorMin        = new Vector2(0f, 0f);
        shopRect.anchorMax        = new Vector2(1f, 0.42f);
        shopRect.offsetMin        = new Vector2(6f, 6f);
        shopRect.offsetMax        = new Vector2(-6f, 0f);
        shopRect.anchoredPosition = Vector2.zero;
        shopRect.sizeDelta        = Vector2.zero;

        // Background
        var shopBG = shopGO.AddComponent<Image>();
        shopBG.color         = new Color(0.08f, 0.10f, 0.13f, 0.8f);
        shopBG.raycastTarget = false;

        // Title
        var titleGO = MakeText(shopGO.transform, "ShopTitle", "CROPS",
            new Vector2(0f, 0.88f), new Vector2(1f, 1.00f), 11f, new Color(0.55f,0.55f,0.55f,1f));

        // Crop shop is managed by CropShopUIController
        var ctrl = shopGO.AddComponent<CropShopUIController>();
        EditorUtility.SetDirty(shopGO);

        // ── Move Stats panel above shop ──
        if (stats != null)
        {
            var r = stats.GetComponent<RectTransform>();
            r.anchorMin        = new Vector2(0f, 0.42f);
            r.anchorMax        = new Vector2(1f, 0.48f);
            r.offsetMin        = new Vector2(6f, 0f);
            r.offsetMax        = new Vector2(-6f, 0f);
            r.anchoredPosition = Vector2.zero;
            r.sizeDelta        = Vector2.zero;

            // Make stats horizontal: FP + rate side by side
            var fpText     = stats.Find("FocusPointsText");
            var incomeText = stats.Find("IncomeRateText");
            var fpLabel    = stats.Find("FPLabel");
            var sessText   = stats.Find("SessionCountText");

            SetAnchor(fpLabel,    0f, 0f, 0.35f, 1f, 10f, new Color(0.55f,0.55f,0.55f,1f));
            SetAnchor(fpText,     0.30f, 0f, 0.65f, 1f, 18f, Color.white);
            SetAnchor(incomeText, 0.63f, 0f, 1f,   1f, 12f, new Color(0.20f,0.85f,0.70f,1f));
            if (sessText != null) sessText.gameObject.SetActive(false);
            EditorUtility.SetDirty(stats.gameObject);
        }

        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("BuildCropShopUI complete + saved!");
    }

    static GameObject MakeText(Transform parent, string name, string text,
        Vector2 ancMin, Vector2 ancMax, float size, Color color)
    {
        var go   = new GameObject(name);
        go.transform.SetParent(parent, false);
        var r    = go.AddComponent<RectTransform>();
        r.anchorMin = ancMin; r.anchorMax = ancMax;
        r.offsetMin = Vector2.zero; r.offsetMax = Vector2.zero;
        r.anchoredPosition = Vector2.zero; r.sizeDelta = Vector2.zero;
        var tmp  = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text; tmp.fontSize = size; tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;
        return go;
    }

    static void SetAnchor(Transform t, float ax, float ay, float bx, float by,
        float size, Color color)
    {
        if (t == null) return;
        var r = t.GetComponent<RectTransform>();
        r.anchorMin = new Vector2(ax,ay); r.anchorMax = new Vector2(bx,by);
        r.offsetMin = Vector2.zero; r.offsetMax = Vector2.zero;
        r.anchoredPosition = Vector2.zero; r.sizeDelta = Vector2.zero;
        var tmp = t.GetComponent<TextMeshProUGUI>();
        if (tmp != null) { tmp.fontSize = size; tmp.color = color; tmp.alignment = TextAlignmentOptions.Center; }
        EditorUtility.SetDirty(t.gameObject);
    }
}
