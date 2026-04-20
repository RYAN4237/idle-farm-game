using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class Step2SceneBuilder
{
    public static void Execute()
    {
        var gm = GameObject.Find("GameManager");
        var canvas = GameObject.Find("UICanvas");
        if (gm == null || canvas == null)
        {
            Debug.LogError("[Step2] GameManager or UICanvas not found!");
            return;
        }

        // Add FeedbackSystem to GameManager
        var fb = gm.GetComponent<FeedbackSystem>();
        if (fb == null) fb = gm.AddComponent<FeedbackSystem>();

        // Create popup anchor on canvas
        Transform anchor = canvas.transform.Find("PopupAnchor");
        GameObject anchorGO;
        if (anchor == null)
        {
            anchorGO = new GameObject("PopupAnchor", typeof(RectTransform));
            anchorGO.transform.SetParent(canvas.transform, false);
            var rect = anchorGO.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.55f);
            rect.anchorMax = new Vector2(0.5f, 0.55f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(400, 100);
        }
        else
        {
            anchorGO = anchor.gameObject;
        }
        fb.popupParent = anchorGO.transform;

        // Container
        var container = GameObject.Find("UICanvas/BackgroundPanel/MainContainer");
        if (container == null)
        {
            Debug.LogError("[Step2] MainContainer not found!");
            return;
        }

        TMP_FontAsset tmpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
            "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
        if (tmpFont == null) tmpFont = TMP_Settings.defaultFontAsset;
        Material tmpMat = tmpFont != null ? tmpFont.material : null;

        // Duration row (before TimerText)
        Transform timerT = container.transform.Find("TimerText");
        int timerIdx = timerT != null ? timerT.GetSiblingIndex() : 1;

        Transform durRow = container.transform.Find("DurationRow");
        if (durRow == null)
        {
            var durRowGO = new GameObject("DurationRow", typeof(RectTransform));
            durRowGO.transform.SetParent(container.transform, false);
            durRowGO.transform.SetSiblingIndex(timerIdx);
            var hl = durRowGO.AddComponent<HorizontalLayoutGroup>();
            hl.childAlignment = TextAnchor.MiddleCenter;
            hl.spacing = 10f;
            hl.childControlWidth = false;
            hl.childControlHeight = false;
            var le = durRowGO.AddComponent<LayoutElement>();
            le.preferredHeight = 40;

            var decGO = MakeBtn("DecBtn", durRowGO.transform, "-5", new Color(0.6f, 0.35f, 0.35f, 1f), 60, 35, tmpFont, tmpMat);
            var labelGO = MakeTMP("DurationLabel", durRowGO.transform, "25 min", 22, tmpFont, tmpMat);
            labelGO.GetComponent<RectTransform>().sizeDelta = new Vector2(120, 35);
            var incGO = MakeBtn("IncBtn", durRowGO.transform, "+5", new Color(0.35f, 0.6f, 0.35f, 1f), 60, 35, tmpFont, tmpMat);

            var uiMgr = canvas.GetComponent<UIManager>();
            if (uiMgr != null)
            {
                uiMgr.decreaseDurationBtn = decGO.GetComponent<Button>();
                uiMgr.increaseDurationBtn = incGO.GetComponent<Button>();
                uiMgr.durationLabelText = labelGO.GetComponent<TextMeshProUGUI>();
            }
        }

        // Session count text (after IncomeRateText)
        Transform incomeT = container.transform.Find("IncomeRateText");
        Transform sessT = container.transform.Find("SessionCountText");
        if (sessT == null && incomeT != null)
        {
            var sessGO = MakeTMP("SessionCountText", container.transform, "Sessions: 0", 20, tmpFont, tmpMat);
            sessGO.transform.SetSiblingIndex(incomeT.GetSiblingIndex() + 1);
            var sle = sessGO.AddComponent<LayoutElement>();
            sle.preferredHeight = 30;
            sessGO.GetComponent<TextMeshProUGUI>().color = new Color(0.35f, 0.35f, 0.35f, 0.8f);

            var uiMgr = canvas.GetComponent<UIManager>();
            if (uiMgr != null)
                uiMgr.sessionCountText = sessGO.GetComponent<TextMeshProUGUI>();
        }

        EditorUtility.SetDirty(gm);
        EditorUtility.SetDirty(canvas);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        Debug.Log("[Step2] Scene updated: FeedbackSystem + popup anchor + duration controls + session counter!");
    }

    static GameObject MakeBtn(string n, Transform p, string label, Color col, float w, float h, TMP_FontAsset font, Material mat)
    {
        var go = new GameObject(n, typeof(RectTransform));
        go.transform.SetParent(p, false);
        var img = go.AddComponent<Image>();
        img.color = col;
        go.AddComponent<Button>();
        go.GetComponent<RectTransform>().sizeDelta = new Vector2(w, h);
        var tgo = MakeTMP("Text", go.transform, label, 18, font, mat);
        var tr = tgo.GetComponent<RectTransform>();
        tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one;
        tr.offsetMin = Vector2.zero; tr.offsetMax = Vector2.zero;
        return go;
    }

    static GameObject MakeTMP(string n, Transform p, string text, float size, TMP_FontAsset font, Material mat)
    {
        var go = new GameObject(n, typeof(RectTransform));
        go.transform.SetParent(p, false);
        go.AddComponent<CanvasRenderer>();
        var tmp = go.AddComponent<TextMeshProUGUI>();
        if (font != null) tmp.font = font;
        if (mat != null) tmp.fontSharedMaterial = mat;
        tmp.text = text;
        tmp.fontSize = size;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.enableWordWrapping = false;
        tmp.overflowMode = TextOverflowModes.Overflow;
        return go;
    }
}
