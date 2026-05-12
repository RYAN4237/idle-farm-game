using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using System.Linq;

/// Fix water rendering: replace tile-based water with a solid colored tilemap using a plain white pixel
public class FixWaterRendering
{
    [MenuItem("Tools/Fix Water Rendering")]
    public static void Execute()
    {
        var waterGO = GameObject.Find("Tilemap/WaterLayer") ?? GameObject.Find("WaterLayer");
        if (waterGO == null) { Debug.LogError("WaterLayer not found"); return; }

        var tm = waterGO.GetComponent<Tilemap>();
        tm.ClearAllTiles();

        // Use Water_0 sprite but tint it with the tilemap color property
        string waterPath = "Assets/Sprout Lands - Sprites - Basic pack/Tilesets/Water.png";
        var sprites = AssetDatabase.LoadAllAssetsAtPath(waterPath).OfType<Sprite>().ToArray();
        Debug.Log($"Water sprites: {sprites.Length} — {string.Join(", ", sprites.Take(6).Select(s => $"{s.name}({s.rect.width}x{s.rect.height})"))}");

        var waterSpr = sprites.FirstOrDefault(s => s.name == "Water_0") ?? sprites.FirstOrDefault();
        if (waterSpr == null) { Debug.LogError("No water sprite found"); return; }

        // Create ONE tile instance, reuse for all cells (Unity tilemap batches identical tile instances)
        var waterTile = ScriptableObject.CreateInstance<Tile>();
        waterTile.sprite = waterSpr;
        waterTile.color  = Color.white; // use tilemap color, not per-tile color

        // Set tilemap-level color to water blue
        tm.color = new Color(0.45f, 0.78f, 0.92f, 1f);

        // River y=2 full width
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

        EditorUtility.SetDirty(waterGO);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log($"[FixWaterRendering] Done — water tile={waterSpr.name} ({waterSpr.rect.width}x{waterSpr.rect.height})");
    }
}
