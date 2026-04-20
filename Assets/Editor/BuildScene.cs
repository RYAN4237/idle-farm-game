using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class BuildScene
{
    [MenuItem("CozyIdle/Build Scene")]
    public static void Execute()
    {
        GameObject gm = GameObject.Find("GameManager");
        if (gm == null) gm = new GameObject("GameManager");
        AddComp<ResourceSystem>(gm);
        AddComp<FocusSystem>(gm);
        AddComp<IdleSystem>(gm);

        GameObject canvasGO = GameObject.Find("UICanvas");
        if (canvasGO == null)
        {
            canvasGO = new GameObject("UICanvas");
            var c = canvasGO.AddComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay;
            c.sortingOrder = 100;
            var cs = canvasGO.AddComponent<CanvasScaler>();
            cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            cs.referenceResolution = new Vector2(1920, 1080);
            canvasGO.AddComponent<GraphicRaycaster>();
        }

        var bgPanel = MakeChild("BackgroundPanel", canvasGO.transform);
        var bgImg = AddComp<Image>(bgPanel);
        bgImg.color = new Color(0.56f, 0.73f, 0.87f, 1f);
        Stretch(bgPanel);

        var container = MakeChild("MainContainer", bgPanel.transform);
        var cr = container.GetComponent<RectTransform>();
        cr.anchorMin = new Vector2(0.25f, 0.1f);
        cr.anchorMax = new Vector2(0.75f, 0.9f);
        cr.offsetMin = Vector2.zero;
        cr.offsetMax = Vector2.zero;
        var vl = AddComp<VerticalLayoutGroup>(container);
        vl.childAlignment = TextAnchor.MiddleCenter;
        vl.spacing = 30f;
        vl.childControlWidth = true;
        vl.childControlHeight = false;
        vl.childForceExpandWidth = true;
        vl.childForceExpandHeight = false;

        var statusGO = MakeTMP("StatusText", container.transform, "Idle", 36); SetH(statusGO, 60);
        var timerGO = MakeTMP("TimerText", container.transform, "25:00", 96); SetH(timerGO, 120);

        var btnBox = MakeChild("ButtonContainer", container.transform);
        var hl = AddComp<HorizontalLayoutGroup>(btnBox);
        hl.childAlignment = TextAnchor.MiddleCenter;
        hl.spacing = 20f;
        hl.childControlWidth = false;
        hl.childControlHeight = false;
        SetH(btnBox, 70);

        var startBtn = MakeBtn("StartPauseButton", btnBox.transform, "Start", new Color(0.3f,0.7f,0.4f,1f), 200, 60);
        var resetBtn = MakeBtn("ResetButton", btnBox.transform, "Reset", new Color(0.8f,0.4f,0.4f,1f), 150, 60);

        var sep = MakeChild("Separator", container.transform); SetH(sep, 20);
        var fpLabel = MakeTMP("FPLabel", container.transform, "Focus Points", 28); SetH(fpLabel, 40);
        var fpValue = MakeTMP("FocusPointsText", container.transform, "0", 64); SetH(fpValue, 80);
        var incomeGO = MakeTMP("IncomeRateText", container.transform, "+1.0/s", 24); SetH(incomeGO, 40);
        incomeGO.GetComponent<TextMeshProUGUI>().color = new Color(0.4f,0.4f,0.4f,1f);

        var ui = AddComp<UIManager>(canvasGO);
        ui.timerText = timerGO.GetComponent<TextMeshProUGUI>();
        ui.startPauseButton = startBtn.GetComponent<Button>();
        ui.startPauseButtonText = startBtn.GetComponentInChildren<TextMeshProUGUI>();
        ui.resetButton = resetBtn.GetComponent<Button>();
        ui.focusPointsText = fpValue.GetComponent<TextMeshProUGUI>();
        ui.incomeRateText = incomeGO.GetComponent<TextMeshProUGUI>();
        ui.statusText = statusGO.GetComponent<TextMeshProUGUI>();
        ui.backgroundPanel = bgImg;

        EditorUtility.SetDirty(canvasGO);
        EditorUtility.SetDirty(gm);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        Debug.Log("[CozyIdle] Phase 1 scene built!");
    }

    static GameObject MakeChild(string n, Transform p)
    {
        var t = p.Find(n);
        if (t != null) return t.gameObject;
        var go = new GameObject(n, typeof(RectTransform));
        go.transform.SetParent(p, false);
        return go;
    }

    static GameObject MakeTMP(string n, Transform p, string txt, float size)
    {
        var go = MakeChild(n, p);
        var tmp = AddComp<TextMeshProUGUI>(go);
        tmp.text = txt;
        tmp.fontSize = size;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        return go;
    }

    static GameObject MakeBtn(string n, Transform p, string label, Color col, float w, float h)
    {
        var go = MakeChild(n, p);
        var img = AddComp<Image>(go);
        img.color = col;
        AddComp<Button>(go);
        go.GetComponent<RectTransform>().sizeDelta = new Vector2(w, h);
        var tgo = MakeChild("Text", go.transform);
        var tmp = AddComp<TextMeshProUGUI>(tgo);
        tmp.text = label; tmp.fontSize = 28;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        Stretch(tgo);
        return go;
    }

    static void Stretch(GameObject go)
    {
        var r = go.GetComponent<RectTransform>();
        r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one;
        r.offsetMin = Vector2.zero; r.offsetMax = Vector2.zero;
    }

    static void SetH(GameObject go, float h)
    {
        var le = AddComp<LayoutElement>(go);
        le.preferredHeight = h;
    }

    static T AddComp<T>(GameObject go) where T : Component
    {
        var c = go.GetComponent<T>();
        return c != null ? c : go.AddComponent<T>();
    }
}
