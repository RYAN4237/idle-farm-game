using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

/// Clean rebuild: solid grass only (Grass_66..76), organic water shapes, no broken edge tiles
public class CleanOrganicRebuild
{
    const string GRASS_PATH = "Assets/Sprout Lands - Sprites - Basic pack/Tilesets/Grass.png";
    const string TILE_DIR   = "Assets/Tiles";

    const int X0 = -3, X1 = 23, Y0 = -3, Y1 = 8;

    // Winding river — same organic shape as OrganicMapRebuild
    static readonly (int x, int y)[] RIVER = {
        (-3,2),(-2,2),(-1,2),(0,2),
        (1,2),(2,2),(2,1),(3,1),(3,2),
        (4,1),(4,2),(5,1),(5,2),(5,3),
        (6,2),(6,3),(7,3),(7,2),(8,2),
        (9,2),(9,3),(10,3),(10,2),(11,2),
        (12,2),(12,1),(13,1),(13,2),(14,2),
        (15,2),(15,1),(16,1),(16,2),(17,2),
        (18,2),(19,2),(20,2),(21,2),(22,2),
    };

    static readonly (int x, int y)[] POND = {
        (2,-1),(3,-1),(4,-1),(3,-2),(4,-2),(5,-2),
        (5,-1),(5,0),(4,0),
        (2,-2),(1,-2),(1,-1),
        (6,-2),(6,-1),
    };

    static readonly (int x, int y)[] POOL = {
        (16,5),(17,5),(18,5),(19,5),
        (17,6),(18,6),(19,6),(20,6),
        (18,7),(19,7),
        (16,4),(17,4),
    };

    [MenuItem("Tools/Clean Organic Rebuild")]
    public static void Execute()
    {
        if (!AssetDatabase.IsValidFolder(TILE_DIR))
            AssetDatabase.CreateFolder("Assets", "Tiles");

        // Delete transition layer — it's causing problems
        var transGO = GameObject.Find("Tilemap/TransitionLayer") ?? GameObject.Find("TransitionLayer");
        if (transGO != null)
        {
            GameObject.DestroyImmediate(transGO);
            Debug.Log("TransitionLayer removed");
        }

        var grassSprites = AssetDatabase.LoadAllAssetsAtPath(GRASS_PATH).OfType<Sprite>().ToArray();
        if (grassSprites.Length == 0) { Debug.LogError("No grass sprites"); return; }

        // Build persistent tile assets for Grass_66..71 only (verified fully-opaque tiles)
        // Grass_72..76 have alpha=0 at corners despite being in the same sprite row
        var solidTiles = new List<Tile>();
        for (int i = 66; i <= 71; i++)
        {
            var spr = grassSprites.FirstOrDefault(s => s.name == $"Grass_{i}");
            if (spr == null) continue;

            string path = $"{TILE_DIR}/GSolid_{i}.asset";
            AssetDatabase.DeleteAsset(path);
            var t = ScriptableObject.CreateInstance<Tile>();
            t.sprite = spr;
            t.name   = $"GSolid_{i}";
            AssetDatabase.CreateAsset(t, path);
            solidTiles.Add(t);
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // Reload from disk
        var loadedTiles = Enumerable.Range(66, 6)
            .Select(i => AssetDatabase.LoadAssetAtPath<Tile>($"{TILE_DIR}/GSolid_{i}.asset"))
            .Where(t => t != null && t.sprite != null)
            .ToArray();
        Debug.Log($"Loaded {loadedTiles.Length} solid grass tiles");

        // ── Rebuild GrassLayer ──
        var grassGO = GameObject.Find("Tilemap/GrassLayer") ?? GameObject.Find("GrassLayer");
        if (grassGO == null) { Debug.LogError("GrassLayer not found"); return; }
        var grassTm = grassGO.GetComponent<Tilemap>();
        var grassRend = grassGO.GetComponent<TilemapRenderer>();
        grassTm.ClearAllTiles();
        grassRend.sortingOrder = 0;
        grassRend.mode = TilemapRenderer.Mode.Chunk;

        var rng = new System.Random(7331);

        // Weight distribution: 66=most common (plain), then decreasing frequency
        // Only Grass_66..71 confirmed fully opaque (alpha=1 everywhere)
        int[] weightedIdx = {
            0,0,0,0,0, 0,0,0,0,0,  // idx 0 (Grass_66) ~35%
            1,1,1,1,1, 1,1,1,       // idx 1 (Grass_67) ~27%
            2,2,2,2,2,              // idx 2 (Grass_68) ~17%
            3,3,3,                  // idx 3 (Grass_69) ~10%
            4,4,                    // idx 4 (Grass_70) ~7%
            5                       // idx 5 (Grass_71) ~3%
        };

        for (int x = X0; x < X1; x++)
        for (int y = Y0; y < Y1; y++)
        {
            // Perlin noise breaks uniform grid tiling pattern
            float n = Mathf.PerlinNoise(x * 0.29f, y * 0.29f);
            int tileIdx;
            if (n > 0.75f)
                tileIdx = rng.Next(2, loadedTiles.Length); // occasional variety
            else
                tileIdx = weightedIdx[rng.Next(weightedIdx.Length)];

            tileIdx = Mathf.Clamp(tileIdx, 0, loadedTiles.Length - 1);
            grassTm.SetTile(new Vector3Int(x, y, 0), loadedTiles[tileIdx]);
        }

        EditorUtility.SetDirty(grassGO);

        // ── Rebuild WaterLayer ──
        var waterGO = GameObject.Find("Tilemap/WaterLayer") ?? GameObject.Find("WaterLayer");
        if (waterGO == null) { Debug.LogError("WaterLayer not found"); return; }
        var waterTm   = waterGO.GetComponent<Tilemap>();
        var waterRend = waterGO.GetComponent<TilemapRenderer>();
        waterTm.ClearAllTiles();
        waterTm.color = Color.white;
        waterRend.sortingOrder = 1;
        waterRend.mode = TilemapRenderer.Mode.Chunk;

        var solidWater = AssetDatabase.LoadAssetAtPath<Tile>($"{TILE_DIR}/WaterSolid.asset");
        if (solidWater == null) { Debug.LogError("WaterSolid.asset not found — run Fix Water Solid Color first"); return; }

        foreach (var (x,y) in RIVER) waterTm.SetTile(new Vector3Int(x, y, 0), solidWater);
        foreach (var (x,y) in POND)  waterTm.SetTile(new Vector3Int(x, y, 0), solidWater);
        foreach (var (x,y) in POOL)  waterTm.SetTile(new Vector3Int(x, y, 0), solidWater);

        EditorUtility.SetDirty(waterGO);

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log("[CleanOrganicRebuild] Done");
    }
}
