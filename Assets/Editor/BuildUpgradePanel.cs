using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;

public static class BuildUpgradePanel
{
    static readonly Color PanelBG   = new(0.15f, 0.12f, 0.08f, 0.97f);
    static readonly Color BorderCol = new(0.55f, 0.42f, 0.20f, 1f);
    static readonly Color BtnGreen  = new(0.20f, 0.55f, 0.25f, 1f);
    static readonly Color TextLight = new(0.95f, 0.90f, 0.75f, 1f);

    [MenuItem("Tools/Build Upgrade Panel")]
    public static void Execute()
    {
        var canvas = GameObject.Find("UICanvas");
        if (canvas == null) { Debug.LogError("UICanvas not found"); return; }

        // Remove old
        var old = canvas.transform.Find("UpgradeSystem");
        if (old != null) Undo.DestroyObjectImmediate(old.gameObject);

        // Root container
        var root = new GameObject("UpgradeSystem");
        Undo.RegisterCreatedObjectUndo(root, "BuildUpgradePanel");
        root.transform.SetParent(canvas.transform, false);
        var rootRT = root.AddComponent<RectTransform>();
        rootRT.anchorMin = Vector2.zero; rootRT.anchorMax = Vector2.one;
        rootRT.offsetMin = Vector2.zero; rootRT.offsetMax = Vector2.zero;

        // ── UPGRADE button (top-left corner, visible always) ──
        var btnGO = new GameObject("UpgradeBtn");
        btnGO.transform.SetParent(root.transform, false);
        var btnRT = btnGO.AddComponent<RectTransform>();
        btnRT.anchorMin = new Vector2(0, 1);
        btnRT.anchorMax = new Vector2(0, 1);
        btnRT.pivot = new Vector2(0, 1);
        btnRT.anchoredPosition = new Vector2(8, -4);
        btnRT.sizeDelta = new Vector2(70, 24);
        btnGO.AddComponent<Image>().color = BtnGreen;
        var openBtn = btnGO.AddComponent<Button>();

        var btnLabel = new GameObject("Label");
        btnLabel.transform.SetParent(btnGO.transform, false);
        var btnLabelRT = btnLabel.AddComponent<RectTransform>();
        btnLabelRT.anchorMin = Vector2.zero; btnLabelRT.anchorMax = Vector2.one;
        btnLabelRT.offsetMin = Vector2.zero; btnLabelRT.offsetMax = Vector2.zero;
        var btnTMP = btnLabel.AddComponent<TextMeshProUGUI>();
        btnTMP.text = "UPGRADE"; btnTMP.fontSize = 11; btnTMP.color = Color.white;
        btnTMP.alignment = TextAlignmentOptions.Center; btnTMP.raycastTarget = false;

        // ── Multiplier display (next to button) ──
        var multGO = new GameObject("MultLabel");
        multGO.transform.SetParent(root.transform, false);
        var multRT = multGO.AddComponent<RectTransform>();
        multRT.anchorMin = new Vector2(0, 1);
        multRT.anchorMax = new Vector2(0, 1);
        multRT.pivot = new Vector2(0, 1);
        multRT.anchoredPosition = new Vector2(82, -4);
        multRT.sizeDelta = new Vector2(60, 24);
        var multTMP = multGO.AddComponent<TextMeshProUGUI>();
        multTMP.text = "x1.00"; multTMP.fontSize = 12;
        multTMP.color = new Color(1f, 0.85f, 0.2f, 1f);
        multTMP.alignment = TextAlignmentOptions.MidlineLeft;
        multTMP.raycastTarget = false;

        // ── Panel (popup, hidden by default) ──
        var panel = new GameObject("UpgradePanel");
        panel.transform.SetParent(root.transform, false);
        var panelRT = panel.AddComponent<RectTransform>();
        panelRT.anchorMin = new Vector2(0.1f, 0.05f);
        panelRT.anchorMax = new Vector2(0.9f, 0.95f);
        panelRT.offsetMin = Vector2.zero;
        panelRT.offsetMax = Vector2.zero;
        panel.AddComponent<Image>().color = PanelBG;
        var outline = panel.AddComponent<Outline>();
        outline.effectColor = BorderCol; outline.effectDistance = new Vector2(2, -2);

        // Title
        var title = new GameObject("Title");
        title.transform.SetParent(panel.transform, false);
        var titleRT = title.AddComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0, 0.85f); titleRT.anchorMax = Vector2.one;
        titleRT.offsetMin = new Vector2(10, 0); titleRT.offsetMax = new Vector2(-40, -4);
        var titleTMP = title.AddComponent<TextMeshProUGUI>();
        titleTMP.text = "UNLOCK TREE"; titleTMP.fontSize = 14;
        titleTMP.color = TextLight; titleTMP.fontStyle = FontStyles.Bold;
        titleTMP.alignment = TextAlignmentOptions.MidlineLeft; titleTMP.raycastTarget = false;

        // Close button
        var closeGO = new GameObject("CloseBtn");
        closeGO.transform.SetParent(panel.transform, false);
        var closeRT = closeGO.AddComponent<RectTransform>();
        closeRT.anchorMin = new Vector2(1, 1);
        closeRT.anchorMax = new Vector2(1, 1);
        closeRT.pivot = new Vector2(1, 1);
        closeRT.anchoredPosition = new Vector2(-4, -4);
        closeRT.sizeDelta = new Vector2(28, 20);
        closeGO.AddComponent<Image>().color = new Color(0.6f, 0.15f, 0.15f, 1f);
        var closeBtn = closeGO.AddComponent<Button>();
        var closeLbl = new GameObject("X");
        closeLbl.transform.SetParent(closeGO.transform, false);
        var closeLblRT = closeLbl.AddComponent<RectTransform>();
        closeLblRT.anchorMin = Vector2.zero; closeLblRT.anchorMax = Vector2.one;
        closeLblRT.offsetMin = Vector2.zero; closeLblRT.offsetMax = Vector2.zero;
        var closeTMP = closeLbl.AddComponent<TextMeshProUGUI>();
        closeTMP.text = "X"; closeTMP.fontSize = 12; closeTMP.color = Color.white;
        closeTMP.alignment = TextAlignmentOptions.Center; closeTMP.raycastTarget = false;

        // Node container with grid layout
        var container = new GameObject("NodeContainer");
        container.transform.SetParent(panel.transform, false);
        var contRT = container.AddComponent<RectTransform>();
        contRT.anchorMin = new Vector2(0, 0); contRT.anchorMax = new Vector2(1, 0.83f);
        contRT.offsetMin = new Vector2(8, 8); contRT.offsetMax = new Vector2(-8, -4);
        var glg = container.AddComponent<GridLayoutGroup>();
        glg.cellSize = new Vector2(180, 60);
        glg.spacing = new Vector2(8, 8);
        glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        glg.constraintCount = 4;
        glg.childAlignment = TextAnchor.UpperLeft;

        // ── Wire UpgradePanelController ──
        var ctrl = root.AddComponent<UpgradePanelController>();
        var so = new SerializedObject(ctrl);
        so.FindProperty("_panel").objectReferenceValue = panel;
        so.FindProperty("_openButton").objectReferenceValue = openBtn;
        so.FindProperty("_closeButton").objectReferenceValue = closeBtn;
        so.FindProperty("_multiplierLabel").objectReferenceValue = multTMP;
        so.FindProperty("_nodeContainer").objectReferenceValue = container.transform;
        so.ApplyModifiedProperties();

        panel.SetActive(false);

        EditorUtility.SetDirty(canvas);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("[BuildUpgradePanel] Done! Run Tools/Create Unlock Node Assets first, then assign nodes to UnlockTree.");
    }
}
