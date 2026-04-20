using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class FixButtonsAndBar
{
    public static void Execute()
    {
        var canvas = GameObject.Find("UICanvas");
        float BAR  = 0.26f;
        float LEFT = 0.20f;

        // ── ButtonBar: stretch anchor in canvas ──
        var barT = canvas.transform.Find("ButtonBar");
        if (barT != null)
        {
            var r = barT.GetComponent<RectTransform>();
            // Place in bottom of left timer section
            r.anchorMin        = new Vector2(0.005f, 0.01f);
            r.anchorMax        = new Vector2(LEFT - 0.005f, 0.09f);
            r.offsetMin        = new Vector2(2f, 2f);
            r.offsetMax        = new Vector2(-2f, -2f);
            r.anchoredPosition = Vector2.zero;
            r.sizeDelta        = Vector2.zero;
            r.pivot            = new Vector2(0.5f, 0.5f);
            EditorUtility.SetDirty(barT.gameObject);
        }

        // ── StartPauseButton: stretch within ButtonBar ──
        FixBtn(barT, "StartPauseButton", 0f, 0f, 0.57f, 1f,
            new Color(0.15f, 0.62f, 0.48f, 1f), "Start Focus", 11f);

        // ── ResetButton: stretch within ButtonBar ──
        FixBtn(barT, "ResetButton", 0.61f, 0f, 1f, 1f,
            new Color(0.28f, 0.30f, 0.35f, 1f), "Reset", 11f);

        // ── CycleDots: just above buttons ──
        var dotsT = canvas.transform.Find("CycleDots");
        if (dotsT != null)
        {
            var r = dotsT.GetComponent<RectTransform>();
            r.anchorMin        = new Vector2(0.01f, 0.085f);
            r.anchorMax        = new Vector2(LEFT - 0.01f, 0.115f);
            r.offsetMin        = Vector2.zero;
            r.offsetMax        = Vector2.zero;
            r.anchoredPosition = Vector2.zero;
            r.sizeDelta        = Vector2.zero;
            EditorUtility.SetDirty(dotsT.gameObject);
        }

        // ── AutoFarmerBtn: stretch anchor in canvas ──
        var afT = canvas.transform.Find("AutoFarmerBtn");
        if (afT != null)
        {
            float x0 = 1f - 0.22f;
            var r = afT.GetComponent<RectTransform>();
            r.anchorMin        = new Vector2(x0, BAR * 0.51f);
            r.anchorMax        = new Vector2(1f,  BAR * 0.66f);
            r.offsetMin        = new Vector2(3f, 2f);
            r.offsetMax        = new Vector2(-3f, -2f);
            r.anchoredPosition = Vector2.zero;
            r.sizeDelta        = Vector2.zero;
            r.pivot            = new Vector2(0.5f, 0.5f);

            var img = afT.GetComponent<Image>() ?? afT.gameObject.AddComponent<Image>();
            img.color = new Color(0.15f, 0.28f, 0.45f, 1f);
            var btn = afT.GetComponent<Button>();
            if (btn != null) btn.targetGraphic = img;
            EditorUtility.SetDirty(afT.gameObject);
        }

        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("FixButtonsAndBar complete + saved!");
    }

    static void FixBtn(Transform parent, string name,
        float ax, float ay, float bx, float by,
        Color col, string label, float fontSize)
    {
        if (parent == null) return;
        var t = parent.Find(name);
        if (t == null) return;

        var r = t.GetComponent<RectTransform>();
        r.anchorMin        = new Vector2(ax, ay);
        r.anchorMax        = new Vector2(bx, by);
        r.offsetMin        = new Vector2(2f, 2f);
        r.offsetMax        = new Vector2(-2f, -2f);
        r.anchoredPosition = Vector2.zero;
        r.sizeDelta        = Vector2.zero;
        r.pivot            = new Vector2(0.5f, 0.5f);

        var img = t.GetComponent<Image>() ?? t.gameObject.AddComponent<Image>();
        img.color = col;
        var btn = t.GetComponent<Button>();
        if (btn != null) btn.targetGraphic = img;

        var txt = t.GetComponentInChildren<TextMeshProUGUI>();
        if (txt != null) { txt.text = label; txt.fontSize = fontSize; txt.color = Color.white; }

        EditorUtility.SetDirty(t.gameObject);
    }
}
