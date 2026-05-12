using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// Popup shown on game launch when offline progress was earned.
/// Creates itself at runtime — no prefab needed.
public class OfflineNotification : MonoBehaviour
{
    public static void Show(float fp, float seconds, int harvests)
    {
        // Don't show if already open
        if (GameObject.Find("OfflineNotification") != null) return;

        var canvas = GameObject.Find("UICanvas");
        if (canvas == null) return;

        var go = new GameObject("OfflineNotification");
        go.transform.SetParent(canvas.transform, false);
        go.AddComponent<OfflineNotification>().Build(fp, seconds, harvests);
    }

    void Build(float fp, float seconds, int harvests)
    {
        // Full-screen dimmer
        var dimmer = new GameObject("Dimmer");
        dimmer.transform.SetParent(transform, false);
        var dr = dimmer.AddComponent<RectTransform>();
        dr.anchorMin = Vector2.zero; dr.anchorMax = Vector2.one;
        dr.offsetMin = Vector2.zero; dr.offsetMax = Vector2.zero;
        var dimImg = dimmer.AddComponent<Image>();
        dimImg.color = new Color(0, 0, 0, 0.6f);
        dimmer.AddComponent<Button>(); // clicking dimmer = close

        // Panel
        var panel = new GameObject("Panel");
        panel.transform.SetParent(transform, false);
        var pr = panel.AddComponent<RectTransform>();
        pr.anchorMin = new Vector2(0.5f, 0.5f);
        pr.anchorMax = new Vector2(0.5f, 0.5f);
        pr.sizeDelta = new Vector2(340, 180);
        var panelImg = panel.AddComponent<Image>();
        panelImg.color = new Vector4(0.18f, 0.14f, 0.09f, 0.97f);
        var ol = panel.AddComponent<Outline>();
        ol.effectColor = new Color(0.55f, 0.42f, 0.20f, 1f);
        ol.effectDistance = new Vector2(2, -2);

        // Format time string
        string timeStr = seconds >= 3600
            ? $"{(int)(seconds / 3600)}h {(int)(seconds % 3600 / 60)}m"
            : $"{(int)(seconds / 60)}m";

        // Title
        AddLabel(panel.transform, "Title",
            new Vector2(0, 0.72f), Vector2.one,
            "Welcome Back!", 16,
            new Color(1f, 0.85f, 0.2f, 1f), FontStyles.Bold);

        // Body
        string body = $"You were away for {timeStr}\n" +
                      $"{harvests} harvest{(harvests != 1 ? "s" : "")} completed\n" +
                      $"+ {(int)fp} FP earned while idle";
        AddLabel(panel.transform, "Body",
            new Vector2(0, 0.25f), new Vector2(1, 0.72f),
            body, 12,
            new Color(0.90f, 0.85f, 0.70f, 1f), FontStyles.Normal);

        // OK button
        var btnGO = new GameObject("OKBtn");
        btnGO.transform.SetParent(panel.transform, false);
        var br = btnGO.AddComponent<RectTransform>();
        br.anchorMin = new Vector2(0.25f, 0f);
        br.anchorMax = new Vector2(0.75f, 0.25f);
        br.offsetMin = new Vector2(0, 6);
        br.offsetMax = new Vector2(0, -6);
        btnGO.AddComponent<Image>().color = new Color(0.20f, 0.55f, 0.25f, 1f);
        var btn = btnGO.AddComponent<Button>();
        btn.onClick.AddListener(() => Destroy(gameObject));
        AddLabel(btnGO.transform, "Label",
            Vector2.zero, Vector2.one,
            "Collect!", 13, Color.white, FontStyles.Bold);

        // Ensure panel is on top
        transform.SetAsLastSibling();
    }

    static void AddLabel(Transform parent, string name,
        Vector2 ancMin, Vector2 ancMax, string text,
        float size, Color color, FontStyles style)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var r = go.AddComponent<RectTransform>();
        r.anchorMin = ancMin; r.anchorMax = ancMax;
        r.offsetMin = new Vector2(10, 2); r.offsetMax = new Vector2(-10, -2);
        var t = go.AddComponent<TextMeshProUGUI>();
        t.text = text; t.fontSize = size; t.color = color;
        t.fontStyle = style; t.alignment = TextAlignmentOptions.Center;
        t.raycastTarget = false;
    }
}
