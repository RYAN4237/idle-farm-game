using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;

/// Refills the tilemap to properly cover the farm strip.
/// Grass top row + dirt rows + dark background rows.
public class RefillFarmTilemap
{
    public static void Execute()
    {
        var farmRoot = GameObject.Find("FarmTilemap");
        if (farmRoot == null) { Debug.LogError("FarmTilemap not found"); return; }

        var tilemap = farmRoot.GetComponentInChildren<Tilemap>();
        if (tilemap == null) { Debug.LogError("Tilemap component not found"); return; }

        string BASE = "Assets/2D Pixel Art Platformer Biome - American Forest/Tilemap/";
        var tileGrass = AssetDatabase.LoadAssetAtPath<TileBase>(BASE + "TileGround1.asset");
        var tileDirt  = AssetDatabase.LoadAssetAtPath<TileBase>(BASE + "TileGround2.asset");
        var tileBG    = AssetDatabase.LoadAssetAtPath<TileBase>(BASE + "TileBackGround1.asset");

        if (tileGrass == null || tileDirt == null || tileBG == null)
        {
            Debug.LogError("Tiles not found! Check path."); return;
        }

        tilemap.ClearAllTiles();

        // FarmGrid: originX=-15, width=30, so x: -15 to +15 = 30 cols
        // Cell size=1, so tile (0,0) in tilemap space = world (-15, -2)
        // FarmGrid height=4, rows 0-3
        // Layout: row3=grass top, row2=dirt, row1=dark bg, row0=dark bg
        // Extra: fill 4 more rows above and below for visual overflow
        int cols = 35;   // extra cols on each side
        int rows = 10;   // total rows including overflow

        for (int x = -2; x < cols; x++)
        {
            for (int y = -3; y < rows; y++)
            {
                var pos  = new Vector3Int(x, y, 0);
                TileBase tile;

                if      (y == 3) tile = tileGrass;  // top: grass
                else if (y == 2) tile = tileDirt;   // one below: dirt
                else             tile = tileBG;     // rest: dark background

                tilemap.SetTile(pos, tile);
            }
        }

        var tr = tilemap.GetComponent<TilemapRenderer>();
        if (tr != null) tr.sortingOrder = -20;

        // Position: FarmGrid origin is (-15, -2), tilemap cell=1x1
        farmRoot.transform.position = new Vector3(-15, -2, 2);

        EditorUtility.SetDirty(farmRoot);
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log($"[RefillFarmTilemap] Filled {cols * rows} tiles. Grass row at y=3.");
    }
}
