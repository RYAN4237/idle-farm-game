using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;

public class FillTilemapGround
{
    public static void Execute()
    {
        // ── 找到 FarmTilemap 里的 Tilemap ──
        var root = GameObject.Find("FarmTilemap");
        if (root == null) { Debug.LogError("FarmTilemap not found!"); return; }

        // 找到 Layer1（Tilemap组件所在子对象）
        var tilemap = root.GetComponentInChildren<Tilemap>();
        if (tilemap == null) { Debug.LogError("No Tilemap component found!"); return; }

        // ── 清除现有内容 ──
        tilemap.ClearAllTiles();

        // ── 加载 tile 资源 ──
        // 用 TileGround1（上边缘带草），TileGround2（纯土），TileBackGround1（背景）
        string basePath = "Assets/2D Pixel Art Platformer Biome - American Forest/Tilemap/";

        // 用于顶行（草地上边缘）
        var tileGrass = AssetDatabase.LoadAssetAtPath<TileBase>(basePath + "TileGround1.asset");
        // 用于中间行（纯土地）
        var tileDirt  = AssetDatabase.LoadAssetAtPath<TileBase>(basePath + "TileGround2.asset");
        // 用于背景行
        var tileBG    = AssetDatabase.LoadAssetAtPath<TileBase>(basePath + "TileBackGround1.asset");

        if (tileGrass == null) { Debug.LogError("TileGround1.asset not found"); return; }
        if (tileDirt  == null) { Debug.LogError("TileGround2.asset not found"); return; }
        if (tileBG    == null) { Debug.LogError("TileBackGround1.asset not found"); return; }

        // ── FarmGrid 参数（与 FarmGrid.cs 对应）──
        // originX=-19.2, originY=-2.56, cellSize=1.28, gridWidth=30, gridHeight=4
        // Tilemap tile size: 需要知道tile的PPU
        // 从 Tilemap 的 cell size 来确定
        var grid = root.GetComponent<Grid>();
        Vector3 cellSize = grid != null ? grid.cellSize : Vector3.one;
        Debug.Log($"Tilemap cell size: {cellSize}");

        // FarmGrid 世界坐标范围:
        // X: -19.2 ~ -19.2+30*1.28 = -19.2 ~ 19.2
        // Y: -2.56 ~ -2.56+4*1.28  = -2.56 ~  2.56
        // 转成 Tilemap 的 tile 坐标:
        // Tilemap origin 在 (-19.2, -2.56, 1)
        // 每个 tile = cellSize (通常 1x1 world unit)

        // 我们希望铺多少行多少列：
        // X方向: 38.4 / cellSize.x ≈ 30列（如果cell=1.28）或更多
        // Y方向: 5.12 / cellSize.y ≈ 4行

        int cols = Mathf.CeilToInt(38.4f / cellSize.x) + 4;  // 多铺一点
        int rows = Mathf.CeilToInt(5.12f / cellSize.y) + 2;

        Debug.Log($"Filling {cols} cols x {rows} rows");

        // Tilemap 的 tile 坐标原点就是 tilemap.transform.position
        // 由于我们把 FarmTilemap 放在 (-19.2, -2.56, 1)
        // tile (0,0) 对应世界坐标 (-19.2, -2.56)
        // 所以直接从 (0,0) 开始填

        for (int x = 0; x < cols; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                var pos = new Vector3Int(x, y, 0);
                if (y == rows - 1)
                    tilemap.SetTile(pos, tileGrass);   // 顶行：草地
                else if (y == rows - 2)
                    tilemap.SetTile(pos, tileDirt);    // 第二行：土地
                else
                    tilemap.SetTile(pos, tileBG);      // 其余行：背景土地
            }
        }

        // 额外再铺几行背景（让视野上下有更多覆盖）
        for (int x = 0; x < cols; x++)
        {
            for (int y = rows; y < rows + 4; y++)
            {
                tilemap.SetTile(new Vector3Int(x, y, 0), tileBG);
            }
            for (int y = -1; y >= -4; y--)
            {
                tilemap.SetTile(new Vector3Int(x, y, 0), tileBG);
            }
        }

        EditorUtility.SetDirty(tilemap.gameObject);
        Debug.Log($"Filled tilemap with {cols * (rows + 8)} tiles");

        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("FillTilemapGround done!");
    }
}
