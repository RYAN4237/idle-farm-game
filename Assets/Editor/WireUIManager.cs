using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class WireUIManager
{
    public static void Execute()
    {
        var canvas = GameObject.Find("UICanvas");
        if (canvas == null) { Debug.LogError("UICanvas not found"); return; }

        var ui = canvas.GetComponent<UIManager>() ?? canvas.AddComponent<UIManager>();

        // Panel
        var panel = canvas.transform.Find("ExpandablePanel");
        if (panel != null)
        {
            ui.expandablePanel = panel.GetComponent<RectTransform>();

            // Title (inside Content/TitleBar/TitleText)
            var titleTxt = panel.Find("Content/TitleBar/TitleText");
            if (titleTxt != null) ui.panelTitle = titleTxt.GetComponent<TextMeshProUGUI>();

            // Close button (inside Content/BottomBar/CloseBtn)
            var closeBtn = panel.Find("Content/BottomBar/CloseBtn");
            if (closeBtn != null) ui.closeButton = closeBtn.GetComponent<Button>();
        }

        // Icon buttons
        var iconBar = canvas.transform.Find("RightIconBar");
        if (iconBar != null)
        {
            var sb = iconBar.Find("SeedButton");
            if (sb != null) ui.seedButton = sb.GetComponent<Button>();
            var bb = iconBar.Find("BuildButton");
            if (bb != null) ui.buildButton = bb.GetComponent<Button>();
            var ub = iconBar.Find("UpgradeButton");
            if (ub != null) ui.upgradeButton = ub.GetComponent<Button>();
        }

        // Panel starts hidden (off to the right)
        if (ui.expandablePanel != null)
            ui.expandablePanel.anchoredPosition = new Vector2(300, 0);

        EditorUtility.SetDirty(canvas);
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("[WireUIManager] Wired! S/B/U → panel, close btn connected.");
    }
}
