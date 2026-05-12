using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;

/// Sets up the gear (Settings) button + settings popup menu.
/// Menu has: Close (X) button, Quit button.
public class SetupSettingsMenu
{
    public static void Execute()
    {
        var canvas = GameObject.Find("UICanvas");
        if (canvas == null) { Debug.LogError("UICanvas not found"); return; }

        var topHUD = canvas.transform.Find("HUD/TopHUD");
        if (topHUD == null) { Debug.LogError("TopHUD not found"); return; }

        // 1. Make GearIcon a Button
        var gearGO = topHUD.Find("GearIcon")?.gameObject;
        if (gearGO == null) { Debug.LogError("GearIcon not found"); return; }
        var gearImg = gearGO.GetComponent<Image>();
        gearImg.raycastTarget = true;
        if (gearGO.GetComponent<Button>() == null)
            gearGO.AddComponent<Button>();

        // 2. Create SettingsPopup (starts hidden)
        var existing = canvas.transform.Find("SettingsPopup");
        if (existing != null) Object.DestroyImmediate(existing.gameObject);

        var popup = new GameObject("SettingsPopup");
        popup.transform.SetParent(canvas.transform, false);
        var popupRect = popup.AddComponent<RectTransform>();
        // Anchor to top-right, near gear icon
        popupRect.anchorMin = new Vector2(1f, 1f);
        popupRect.anchorMax = new Vector2(1f, 1f);
        popupRect.pivot     = new Vector2(1f, 1f);
        popupRect.anchoredPosition = new Vector2(-10f, -10f);
        popupRect.sizeDelta = new Vector2(180f, 100f);

        var popupBG = popup.AddComponent<Image>();
        popupBG.color = new Color(0.10f, 0.12f, 0.10f, 0.95f);

        // Close (X) button top-right of popup
        var closeBtn = MakeButton(popup.transform, "ClosePopup", "✕",
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(-4f, -4f), new Vector2(24f, 24f), 12f,
            new Color(0.8f, 0.8f, 0.8f), new Color(0.15f, 0.15f, 0.15f, 0.5f));

        // Quit button
        var quitBtn = MakeButton(popup.transform, "QuitButton", "Quit Game",
            new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f),
            new Vector2(0f, 10f), new Vector2(-20f, 36f), 13f,
            new Color(0.95f, 0.35f, 0.35f), new Color(0.55f, 0.12f, 0.12f, 0.9f));

        // Title text
        var title = new GameObject("Title");
        title.transform.SetParent(popup.transform, false);
        var tr = title.AddComponent<RectTransform>();
        tr.anchorMin = new Vector2(0f, 1f); tr.anchorMax = new Vector2(1f, 1f);
        tr.pivot = new Vector2(0.5f, 1f);
        tr.anchoredPosition = new Vector2(0f, -6f);
        tr.sizeDelta = new Vector2(0f, 28f);
        var txt = title.AddComponent<TextMeshProUGUI>();
        txt.text = "Settings"; txt.fontSize = 13f; txt.fontStyle = FontStyles.Bold;
        txt.color = new Color(0.95f, 0.92f, 0.75f); txt.alignment = TextAlignmentOptions.Center;
        txt.raycastTarget = false;

        // Start hidden
        popup.SetActive(false);

        // 3. Wire SettingsMenuController onto GearIcon
        var ctrl = gearGO.GetComponent<SettingsMenuController>();
        if (ctrl == null) ctrl = gearGO.AddComponent<SettingsMenuController>();

        var so = new SerializedObject(ctrl);
        so.FindProperty("popup").objectReferenceValue    = popup;
        so.FindProperty("closeBtn").objectReferenceValue = closeBtn;
        so.FindProperty("quitBtn").objectReferenceValue  = quitBtn;
        so.ApplyModifiedProperties();

        // Wire GearIcon button onClick
        var gearBtn = gearGO.GetComponent<Button>();
        UnityEditor.Events.UnityEventTools.AddPersistentListener(
            gearBtn.onClick,
            ctrl.TogglePopup);

        EditorUtility.SetDirty(canvas);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log("[Settings] Setup complete: GearIcon → SettingsPopup with Quit button");
    }

    static Button MakeButton(Transform parent, string name, string label,
        Vector2 ancMin, Vector2 ancMax, Vector2 pivot,
        Vector2 ancPos, Vector2 size, float fontSize,
        Color textColor, Color bgColor)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var r = go.AddComponent<RectTransform>();
        r.anchorMin = ancMin; r.anchorMax = ancMax; r.pivot = pivot;
        r.anchoredPosition = ancPos; r.sizeDelta = size;
        var img = go.AddComponent<Image>();
        img.color = bgColor;
        var btn = go.AddComponent<Button>();
        var cols = btn.colors;
        cols.highlightedColor = new Color(bgColor.r + 0.15f, bgColor.g + 0.15f, bgColor.b + 0.15f);
        cols.pressedColor = new Color(bgColor.r - 0.1f, bgColor.g - 0.1f, bgColor.b - 0.1f);
        btn.colors = cols;
        var txt = new GameObject("Text");
        txt.transform.SetParent(go.transform, false);
        var tr = txt.AddComponent<RectTransform>();
        tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one;
        tr.offsetMin = Vector2.zero; tr.offsetMax = Vector2.zero;
        var tmp = txt.AddComponent<TextMeshProUGUI>();
        tmp.text = label; tmp.fontSize = fontSize; tmp.color = textColor;
        tmp.alignment = TextAlignmentOptions.Center; tmp.raycastTarget = false;
        return btn;
    }
}
