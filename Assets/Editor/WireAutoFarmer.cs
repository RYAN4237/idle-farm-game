using UnityEngine;
using UnityEditor;
using TMPro;
using UnityEngine.UI;

public class WireAutoFarmer
{
    public static void Execute()
    {
        var gm = GameObject.Find("GameManager");
        if (gm == null) { Debug.LogError("GameManager not found"); return; }

        // Add AutoFarmer
        if (gm.GetComponent<AutoFarmer>() == null)
        {
            gm.AddComponent<AutoFarmer>();
            Debug.Log("AutoFarmer added.");
        }

        // ── Add upgrade button inside CropShopPanel ──
        var canvas     = GameObject.Find("UICanvas");
        var rightPanel = canvas?.transform.Find("RightPanel");
        var shopPanel  = rightPanel?.Find("CropShopPanel");
        if (shopPanel == null) { Debug.LogError("CropShopPanel not found"); return; }

        // Remove old button if exists
        var old = shopPanel.Find("AutoFarmerBtn");
        if (old != null) Object.DestroyImmediate(old.gameObject);

        // Button at top of shop panel
        var btnGO   = new GameObject("AutoFarmerBtn");
        btnGO.transform.SetParent(shopPanel, false);

        var rect    = btnGO.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.02f, 0.88f);
        rect.anchorMax = new Vector2(0.98f, 1.00f);
        rect.offsetMin = new Vector2(3f, 2f);
        rect.offsetMax = new Vector2(-3f, -2f);

        var img     = btnGO.AddComponent<Image>();
        img.color   = new Color(0.20f, 0.30f, 0.42f, 1f);

        var btn     = btnGO.AddComponent<Button>();
        var cols    = btn.colors;
        cols.highlightedColor = new Color(0.28f, 0.40f, 0.55f, 1f);
        btn.colors  = cols;
        btn.onClick.AddListener(() =>
        {
            if (AutoFarmer.Instance != null && AutoFarmer.Instance.TryUpgrade())
                UpdateButtonText(btnGO);
            else
                Debug.Log("AutoFarmer: Not enough FP or max level reached.");
        });

        var labelGO = new GameObject("Label");
        labelGO.transform.SetParent(btnGO.transform, false);
        var lr = labelGO.AddComponent<RectTransform>();
        lr.anchorMin = Vector2.zero; lr.anchorMax = Vector2.one;
        lr.offsetMin = Vector2.zero; lr.offsetMax = Vector2.zero;
        var ltmp = labelGO.AddComponent<TextMeshProUGUI>();
        ltmp.text      = "🤖 Auto-Farmer Lv1\n200 FP";
        ltmp.fontSize  = 10f;
        ltmp.color     = new Color(0.80f, 0.90f, 1.00f, 1f);
        ltmp.alignment = TextAlignmentOptions.Center;
        ltmp.raycastTarget = false;

        EditorUtility.SetDirty(gm);
        EditorUtility.SetDirty(shopPanel.gameObject);

        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("WireAutoFarmer complete + saved!");
    }

    static void UpdateButtonText(GameObject btn)
    {
        var af  = AutoFarmer.Instance;
        var tmp = btn.GetComponentInChildren<TextMeshProUGUI>();
        if (tmp == null || af == null) return;

        if (!af.CanUpgrade())
            tmp.text = "🤖 Auto-Farmer MAX";
        else
            tmp.text = $"🤖 Auto-Farmer Lv{af.CurrentLevel + 1}\n{af.UpgradeCost()} FP";
    }
}
