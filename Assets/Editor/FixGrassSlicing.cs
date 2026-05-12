using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using System.Linq;

/// Checks Grass.png sprite slicing and fixes GrassLayer with only Grass_0 as solid fill
public class FixGrassSlicing
{
    [MenuItem("Tools/Fix Grass Slicing + Solid Fill")]
    public static void Execute()
    {
        string grassPath = "Assets/Sprout Lands - Sprites - Basic pack/Tilesets/Grass.png";

        // Check current sprites
        var sprites = AssetDatabase.LoadAllAssetsAtPath(grassPath).OfType<Sprite>().ToArray();
        Debug.Log($"Grass.png has {sprites.Length} sprites. Names: {string.Join(", ", sprites.Take(10).Select(s => s.name))}");

        if (sprites.Length < 10)
        {
            Debug.Log("Re-slicing Grass.png as 11x7 grid of 16x16 tiles...");
            var ti = AssetImporter.GetAtPath(grassPath) as TextureImporter;
            if (ti == null) { Debug.LogError("Cannot load Grass.png importer"); return; }

            ti.spriteImportMode = SpriteImportMode.Multiple;
            ti.filterMode = FilterMode.Point;
            ti.textureCompression = TextureImporterCompression.Uncompressed;

            // Grass.png is 176x112 = 11 cols x 7 rows of 16x16
            int cols = 11, rows = 7;
            var meta = new SpriteMetaData[cols * rows];
            int idx = 0;
            // Unity texture coords: y=0 is bottom, so row 0 in texture = bottom row visually
            for (int row = 0; row < rows; row++)
            for (int col = 0; col < cols; col++)
            {
                meta[idx] = new SpriteMetaData
                {
                    name      = $"Grass_{idx}",
                    rect      = new Rect(col * 16, row * 16, 16, 16),
                    pivot     = new Vector2(0.5f, 0.5f),
                    alignment = 9
                };
                idx++;
            }
            ti.spritesheet = meta;
            EditorUtility.SetDirty(ti);
            ti.SaveAndReimport();
            AssetDatabase.Refresh();

            sprites = AssetDatabase.LoadAllAssetsAtPath(grassPath).OfType<Sprite>().ToArray();
            Debug.Log($"After re-slice: {sprites.Length} sprites. First 5: {string.Join(", ", sprites.Take(5).Select(s => s.name))}");
        }

        // Now fill GrassLayer with ONLY Grass_0 (solid, fully opaque tile) everywhere
        // This guarantees no black holes regardless of what other tiles look like
        var grass0 = sprites.FirstOrDefault(s => s.name == "Grass_0");
        if (grass0 == null)
        {
            grass0 = sprites.FirstOrDefault();
            Debug.LogWarning($"Grass_0 not found, using {grass0?.name ?? "nothing"}");
        }
        if (grass0 == null) { Debug.LogError("No grass sprites found at all!"); return; }

        var grassGO = GameObject.Find("Tilemap/GrassLayer") ?? GameObject.Find("GrassLayer");
        if (grassGO == null) { Debug.LogError("GrassLayer not found"); return; }
        var tm = grassGO.GetComponent<Tilemap>();
        tm.ClearAllTiles();

        var solidTile = ScriptableObject.CreateInstance<UnityEngine.Tilemaps.Tile>();
        solidTile.sprite = grass0;

        // Fill entire scene area with the solid grass tile
        for (int x = -3; x < 23; x++)
        for (int y = -3; y < 8; y++)
            tm.SetTile(new Vector3Int(x, y, 0), solidTile);

        EditorUtility.SetDirty(grassGO);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log($"[FixGrassSlicing] Done — {sprites.Length} sprites found, GrassLayer filled with {grass0.name}");
    }
}
