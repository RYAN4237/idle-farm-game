using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class SetupDragFarm
{
    public static void Execute()
    {
        // 删除旧FarmPlot
        var oldPlots = GameObject.FindObjectsOfType<FarmPlot>();
        foreach (var p in oldPlots) Object.DestroyImmediate(p.gameObject);

        var fc = GameObject.Find("FarmContainer");
        if (fc != null && fc.transform.childCount == 0)
            Object.DestroyImmediate(fc);

        // FarmGrid 到 Main Camera
        var cam = GameObject.Find("Main Camera");
        if (cam == null) { Debug.LogError("Main Camera not found!"); return; }
        var grid = cam.GetComponent<FarmGrid>() ?? cam.AddComponent<FarmGrid>();
        grid.cellSize = 1.28f; grid.gridWidth = 30; grid.gridHeight = 4;
        grid.originX = -19.2f; grid.originY = -2.56f;
        EditorUtility.SetDirty(cam);

        // PlacementManager 到 Main Camera
        if (cam.GetComponent<PlacementManager>() == null)
            cam.AddComponent<PlacementManager>();

        // 侧边栏图标
        var canvas = GameObject.Find("UICanvas");
        if (canvas == null) { Debug.LogError("UICanvas not found!"); return; }
        var panel = canvas.transform.Find("ExpandablePanel");

        var oldIcon = panel?.Find("PlotIconGrid");
        if (oldIcon != null) Object.DestroyImmediate(oldIcon.gameObject);

        var iconGridGO = new GameObject("PlotIconGrid");
        iconGridGO.transform.SetParent(panel, false);
        var igRT = iconGridGO.AddComponent<RectTransform>();
        igRT.anchorMin = Vector2.zero; igRT.anchorMax = Vector2.one;
        igRT.offsetMin = new Vector2(10f, 50f); igRT.offsetMax = new Vector2(-10f, -50f);
        var glg = iconGridGO.AddComponent<GridLayoutGroup>();
        glg.cellSize = new Vector2(80f, 80f); glg.spacing = new Vector2(10f, 10f);
        glg.childAlignment = TextAnchor.UpperLeft;

        CreatePlotIcon(iconGridGO.transform, "Farm Plot", new Color(0.25f, 0.55f, 0.25f));

        EditorUtility.SetDirty(canvas);
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("[SetupDragFarm] Done!");
    }

    static void CreatePlotIcon(Transform parent, string label, Color col)
    {
        var go  = new GameObject("PlotIcon_" + label);
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = col;

        var innerGO  = new GameObject("Icon");
        innerGO.transform.SetParent(go.transform, false);
        var innerRT  = innerGO.AddComponent<RectTransform>();
        innerRT.anchorMin = new Vector2(0.15f, 0.25f);
        innerRT.anchorMax = new Vector2(0.85f, 0.75f);
        innerRT.offsetMin = innerRT.offsetMax = Vector2.zero;
        innerGO.AddComponent<Image>().color = new Color(0.45f, 0.28f, 0.10f);

        var txtGO = new GameObject("Label");
        txtGO.transform.SetParent(go.transform, false);
        var txtRT = txtGO.AddComponent<RectTransform>();
        txtRT.anchorMin = Vector2.zero; txtRT.anchorMax = new Vector2(1f, 0.28f);
        txtRT.offsetMin = txtRT.offsetMax = Vector2.zero;
        var tmp = txtGO.AddComponent<TextMeshProUGUI>();
        tmp.text = label; tmp.fontSize = 10f;
        tmp.color = Color.white; tmp.alignment = TextAlignmentOptions.Center;

        var drag = go.AddComponent<DraggablePlotIcon>();
        drag.iconImage      = img;
        drag.normalColor    = col;
        drag.highlightColor = col * 1.3f;
        drag.activeColor    = new Color(0.95f, 0.75f, 0.15f);
    }
}
