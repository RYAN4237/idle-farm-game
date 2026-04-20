using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class WireSeedCells
{
    static readonly (string name, int cost, bool locked)[] Seeds =
    {
        ("Wheat",    10,  false), ("Corn",    20,  false), ("Carrot",  15,  false),
        ("Tomato",   25,  false), ("Potato",  18,  false), ("Pumpkin", 40,  false),
        ("Strawb.",  50,  false), ("Waterml", 80,  false), ("Sunflwr", 60,  false),
        ("Rose",    120,  false), ("Mushroom",200, true),  ("Dragon",  500, true),
    };

    public static void Execute()
    {
        var canvas = GameObject.Find("UICanvas");
        if (canvas == null) { Debug.LogError("UICanvas not found"); return; }

        // ── 1. 修复面板 RectTransform ─────────────────────────────────
        var panel = canvas.transform.Find("ExpandablePanel")?.gameObject;
        if (panel == null) { Debug.LogError("ExpandablePanel not found"); return; }

        var panelRT = panel.GetComponent<RectTransform>();
        // 右边贴边，高度占屏幕80%，宽度300px固定
        panelRT.anchorMin       = new Vector2(1f, 0.1f);
        panelRT.anchorMax       = new Vector2(1f, 0.9f);
        panelRT.pivot           = new Vector2(1f, 0.5f);
        panelRT.sizeDelta       = new Vector2(300f, 0f);
        panelRT.anchoredPosition= new Vector2(300f, 0f); // 隐藏状态（右边外）
        EditorUtility.SetDirty(panel);

        // ── 2. 修复 GridLayout cellSize ───────────────────────────────
        var seedGrid = canvas.transform.Find(
            "ExpandablePanel/Content/Middle/GridWrap/SeedGrid")?.gameObject;
        if (seedGrid != null)
        {
            var glg = seedGrid.GetComponent<GridLayoutGroup>();
            if (glg != null)
            {
                glg.cellSize        = new Vector2(76f, 72f);   // 稍大格子
                glg.spacing         = new Vector2(4f, 4f);
                glg.constraintCount = 3;                        // 3列
            }
            EditorUtility.SetDirty(seedGrid);
        }

        // ── 3. 给每个 Cell 挂 SeedCellButton ─────────────────────────
        int i = 0;
        foreach (var (name, cost, locked) in Seeds)
        {
            var cellPath = $"ExpandablePanel/Content/Middle/GridWrap/SeedGrid/Cell_{name}";
            var cell = canvas.transform.Find(cellPath)?.gameObject;
            if (cell == null) { Debug.LogWarning($"Cell not found: {cellPath}"); i++; continue; }

            // 移除旧Button（如果有）
            var oldBtn = cell.GetComponent<Button>();
            if (oldBtn != null) Object.DestroyImmediate(oldBtn);

            // 挂 SeedCellButton
            var scb = cell.GetComponent<SeedCellButton>() ?? cell.AddComponent<SeedCellButton>();
            scb.seedName = name;
            scb.seedCost = cost;
            scb.isLocked = locked;

            EditorUtility.SetDirty(cell);
            Debug.Log($"  Wired SeedCellButton: {name} (${cost}, locked={locked})");
            i++;
        }

        // ── 4. 更新 UIManager 的 shown/hidden 位置 ────────────────────
        var uiMgr = canvas.GetComponent<UIManager>();
        if (uiMgr != null)
        {
            // UIManager的shownPos=0, hiddenPos=300 (在Update里用anchoredPosition.x)
            // 面板现在pivot=(1,0.5)，所以anchoredPosition.x=0时靠右边，300时在外面
        }

        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("[WireSeedCells] Done! All seed cells wired, panel fixed.");
    }
}
