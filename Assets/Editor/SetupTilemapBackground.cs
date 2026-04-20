using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;

public class SetupTilemapBackground
{
    public static void Execute()
    {
        // ── 1. 删除旧的 GrassBG ──────────────────────────────
        var oldBG = GameObject.Find("GrassBG");
        if (oldBG != null) { Object.DestroyImmediate(oldBG); Debug.Log("Deleted GrassBG"); }

        // ── 2. 关掉 FarmMapScroller 的草地贴图，让它不再生成 __GrassBG__ ──
        var cam = GameObject.Find("Main Camera");
        if (cam != null)
        {
            var scroller = cam.GetComponent<FarmMapScroller>();
            if (scroller != null)
            {
                scroller.grassTexture = null;   // 清空贴图 → Start时不会创建草地背景
                EditorUtility.SetDirty(cam);
                Debug.Log("Cleared grassTexture on FarmMapScroller");
            }
        }

        // ── 3. 放置 Tilemap prefab 到场景 ──────────────────────
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/2D Pixel Art Platformer Biome - American Forest/Tilemap/Tilemap.prefab");

        if (prefab == null)
        {
            Debug.LogError("Tilemap.prefab not found!");
            return;
        }

        // 删除旧的如果有
        var existing = GameObject.Find("Tilemap");
        if (existing != null) Object.DestroyImmediate(existing);

        var tilemapGO = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        tilemapGO.name = "FarmTilemap";

        // ── 4. 定位和缩放 ─────────────────────────────────────
        // 目标：铺满 FarmGrid 的区域
        // FarmGrid: originX=-19.2, originY=-2.56, width=30*1.28=38.4, height=4*1.28=5.12
        // 中心: x = -19.2 + 38.4/2 = 0, y = -2.56 + 5.12/2 = 0
        tilemapGO.transform.position = new Vector3(-19.2f, -2.56f, 1f); // z=1 在grid和plots后面

        // ── 5. 找到 Tilemap 组件，设置 Sorting Order ──────────
        var tilemaps = tilemapGO.GetComponentsInChildren<Tilemap>();
        foreach (var tm in tilemaps)
        {
            var tr = tm.GetComponent<TilemapRenderer>();
            if (tr != null)
            {
                tr.sortingOrder = -10;
                Debug.Log($"Set sortingOrder=-10 on {tm.gameObject.name}");
            }
        }

        EditorUtility.SetDirty(tilemapGO);

        // ── 6. 截图看看 Tilemap 包含哪些 tile ────────────────
        Debug.Log($"FarmTilemap placed. Children: {tilemapGO.transform.childCount}");
        foreach (Transform child in tilemapGO.transform)
            Debug.Log($"  - {child.name}");

        // ── 7. 保存 ──────────────────────────────────────────
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log("SetupTilemapBackground done!");
    }
}
