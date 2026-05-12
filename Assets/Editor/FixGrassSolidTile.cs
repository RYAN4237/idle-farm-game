using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using System.Linq;

/// Rebuild GrassLayer using Grass_66 (true solid interior fill tile, y=0 row)
public class FixGrassSolidTile
{
    [MenuItem("Tools/Fix Grass Solid Tile")]
    public static void Execute()
    {
        string grassPath = "Assets/Sprout Lands - Sprites - Basic pack/Tilesets/Grass.png";
        string tileDir   = "Assets/Tiles";

        if (!AssetDatabase.IsValidFolder(tileDir))
            AssetDatabase.CreateFolder("Assets", "Tiles");

        var sprites = AssetDatabase.LoadAllAssetsAtPath(grassPath).OfType<Sprite>().ToArray();

        // Grass_66..76 are the solid interior tiles (y=0 row = bottom of sheet)
        // Use a weighted mix for visual variety
        var solidSprites = new[] {
            sprites.FirstOrDefault(s => s.name == "Grass_66"),
            sprites.FirstOrDefault(s => s.name == "Grass_67"),
            sprites.FirstOrDefault(s => s.name == "Grass_68"),
            sprites.FirstOrDefault(s => s.name == "Grass_69"),
            sprites.FirstOrDefault(s => s.name == "Grass_70"),
        }.Where(s => s != null).ToArray();

        if (solidSprites.Length == 0) { Debug.LogError("No Grass_66..70 sprites found"); return; }
        Debug.Log($"Found {solidSprites.Length} solid interior sprites: {string.Join(", ", solidSprites.Select(s => s.name))}");

        // Create persistent tile assets
        var tiles = new Tile[solidSprites.Length];
        for (int i = 0; i < solidSprites.Length; i++)
        {
            string tilePath = $"{tileDir}/GrassSolid_{i}.asset";
            AssetDatabase.DeleteAsset(tilePath);
            tiles[i] = ScriptableObject.CreateInstance<Tile>();
            tiles[i].sprite = solidSprites[i];
            tiles[i].name   = $"GrassSolid_{i}";
            AssetDatabase.CreateAsset(tiles[i], tilePath);
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // Reload from disk
        var savedTiles = tiles.Select((_, i) =>
            AssetDatabase.LoadAssetAtPath<Tile>($"{tileDir}/GrassSolid_{i}.asset"))
            .Where(t => t != null && t.sprite != null).ToArray();
        Debug.Log($"Saved and reloaded {savedTiles.Length} tiles");

        // Fill GrassLayer with weighted variety
        var grassGO = GameObject.Find("Tilemap/GrassLayer") ?? GameObject.Find("GrassLayer");
        if (grassGO == null) { Debug.LogError("GrassLayer not found"); return; }
        var tm = grassGO.GetComponent<Tilemap>();
        tm.ClearAllTiles();

        var rng = new System.Random(42);
        for (int x = -3; x < 23; x++)
        for (int y = -3; y < 8; y++)
        {
            // Weighted: 60% first tile (cleanest), rest split among others
            int pick = rng.Next(100);
            int idx = pick < 60 ? 0 : (pick < 80 ? 1 : rng.Next(savedTiles.Length));
            idx = Mathf.Clamp(idx, 0, savedTiles.Length - 1);
            tm.SetTile(new Vector3Int(x, y, 0), savedTiles[idx]);
        }

        EditorUtility.SetDirty(grassGO);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("[FixGrassSolidTile] Done — GrassLayer filled with solid interior tiles Grass_66..70");
    }
}
