using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using System.Linq;

/// Fix water: reset tilemap color to white, use per-tile color instead
public class FixWaterColor
{
    [MenuItem("Tools/Fix Water Color")]
    public static void Execute()
    {
        var waterGO = GameObject.Find("Tilemap/WaterLayer") ?? GameObject.Find("WaterLayer");
        if (waterGO == null) { Debug.LogError("WaterLayer not found"); return; }

        var tm = waterGO.GetComponent<Tilemap>();

        // Remove tilemap-level color tint (it tints the entire tilemap area, not just tiles)
        tm.color = Color.white;

        // Rebuild water tiles with color baked into each tile
        string waterPath = "Assets/Sprout Lands - Sprites - Basic pack/Tilesets/Water.png";
        var sprites = AssetDatabase.LoadAllAssetsAtPath(waterPath).OfType<Sprite>().ToArray();
        var waterSpr = sprites.FirstOrDefault(s => s.name == "Water_0") ?? sprites.FirstOrDefault();
        if (waterSpr == null) { Debug.LogError("No water sprite"); return; }

        tm.ClearAllTiles();

        // Create water tile with color baked in
        var waterTile = ScriptableObject.CreateInstance<Tile>();
        waterTile.sprite = waterSpr;
        waterTile.color  = new Color(0.45f, 0.78f, 0.92f, 1f);

        // River y=2
        for (int x = -3; x < 23; x++)
            tm.SetTile(new Vector3Int(x, 2, 0), waterTile);

        // Pond
        int[,] pond = {
            {1,-2},{2,-2},{3,-2},{4,-2},
            {1,-1},{2,-1},{3,-1},{4,-1},{5,-1},
            {2, 0},{3, 0},{4, 0}
        };
        for (int i = 0; i < pond.GetLength(0); i++)
            tm.SetTile(new Vector3Int(pond[i,0], pond[i,1], 0), waterTile);

        // Upper pool
        int[,] pool = {
            {16,5},{17,5},{18,5},
            {17,6},{18,6},{19,6}
        };
        for (int i = 0; i < pool.GetLength(0); i++)
            tm.SetTile(new Vector3Int(pool[i,0], pool[i,1], 0), waterTile);

        // Also fix GrassLayer sortingOrder — should be 0, WaterLayer should be 1
        var grassGO = GameObject.Find("Tilemap/GrassLayer") ?? GameObject.Find("GrassLayer");
        if (grassGO != null)
        {
            var grassRenderer = grassGO.GetComponent<TilemapRenderer>();
            grassRenderer.sortingOrder = 0;
            Debug.Log($"GrassLayer sortingOrder set to 0");
        }

        var waterRenderer = waterGO.GetComponent<TilemapRenderer>();
        waterRenderer.sortingOrder = 1;

        EditorUtility.SetDirty(waterGO);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("[FixWaterColor] Done — tilemap color reset to white, tile color baked in");
    }
}
