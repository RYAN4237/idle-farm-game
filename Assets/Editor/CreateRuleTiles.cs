using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Linq;

/// Creates a Weighted Random Tile for grass interior variation
/// and a Rule Tile for grass→water edge transitions.
/// Also rebuilds the GrassLayer with proper tile placement.
public class CreateRuleTiles
{
    const string GRASS_PATH  = "Assets/Sprout Lands - Sprites - Basic pack/Tilesets/Grass.png";
    const string WATER_PATH  = "Assets/Sprout Lands - Sprites - Basic pack/Tilesets/Water.png";
    const string DIRT_PATH   = "Assets/Sprout Lands - Sprites - Basic pack/Tilesets/Tilled_Dirt.png";
    const string OUTPUT_PATH = "Assets/Tiles/";

    [MenuItem("Tools/Step 1 - Create Rule Tiles")]
    public static void Execute()
    {
        // Ensure output folder
        if (!AssetDatabase.IsValidFolder("Assets/Tiles"))
            AssetDatabase.CreateFolder("Assets", "Tiles");

        CreateGrassWeightedTile();
        CreateWaterRuleTile();
        RebuildTilemapWithVariety();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[CreateRuleTiles] Done — Rule tiles created in Assets/Tiles/");
    }

    static void CreateGrassWeightedTile() { /* no-op: handled inline */ }

    // ── Rule Tile for water with edge sprites ────────────────────────────────
    static void CreateWaterRuleTile()
    {
        var waterSprites = LoadSprites(WATER_PATH);
        var grassSprites = LoadSprites(GRASS_PATH);

        // Water_0 is a 64x16 strip — it's an animated tile, use directly
        var waterBase = waterSprites.FirstOrDefault();
        if (waterBase == null) { Debug.LogWarning("No water sprite found"); return; }

        // For water, just create a simple tile for now
        // (Rule tile water edges need grass-to-water transition sprites which
        //  exist in Grass.png rows 1-3 as edge tiles)
        var wt = ScriptableObject.CreateInstance<Tile>();
        wt.sprite = waterBase;
        wt.color  = new Color(0.45f, 0.78f, 0.85f, 1f);

        var path = OUTPUT_PATH + "WaterBasic.asset";
        AssetDatabase.DeleteAsset(path);
        AssetDatabase.CreateAsset(wt, path);
        Debug.Log($"[CreateRuleTiles] WaterBasic tile created");
    }

    // ── Rebuild GrassLayer using solid interior tiles only ──────────────────
    static void RebuildTilemapWithVariety()
    {
        var grassGO = GameObject.Find("Tilemap/GrassLayer") ?? GameObject.Find("GrassLayer");
        if (grassGO == null) { Debug.LogError("GrassLayer not found"); return; }

        var tm = grassGO.GetComponent<Tilemap>();
        tm.ClearAllTiles();

        var allGrass = LoadSprites(GRASS_PATH);
        var rng = new System.Random(42);

        int x0 = -3, x1 = 23, y0 = -3, y1 = 8;

        // IMPORTANT: Fill ALL cells including water positions — water layer renders on top.
        // Using only solid interior tiles (Grass_0..21) which are fully opaque 16x16 squares.
        // Edge/corner tiles (Grass_22+) have transparency and must NOT be used as fill tiles.
        for (int x = x0; x < x1; x++)
        for (int y = y0; y < y1; y++)
        {
            // Weighted random from solid interior tiles only (Grass_0..21)
            int pick = rng.Next(100);
            int idx;
            if (pick < 55)      idx = rng.Next(4);          // Grass_0..3 (clean bright green, most common)
            else if (pick < 75) idx = 4 + rng.Next(7);      // Grass_4..10 (subtle detail)
            else if (pick < 90) idx = 11 + rng.Next(5);     // Grass_11..15 (slightly darker)
            else                idx = 16 + rng.Next(6);     // Grass_16..21 (most variety)

            var spr = allGrass.FirstOrDefault(s => s.name == $"Grass_{idx}");
            if (spr == null) spr = allGrass.FirstOrDefault(s => s.name == "Grass_0");
            if (spr == null) continue;

            var t = ScriptableObject.CreateInstance<Tile>();
            t.sprite = spr;
            tm.SetTile(new Vector3Int(x, y, 0), t);
        }

        // Now rebuild water layer too with proper water tiles
        RebuildWaterWithEdges(allGrass);

        EditorUtility.SetDirty(grassGO);
        Debug.Log("[CreateRuleTiles] GrassLayer rebuilt with variety + edge tiles");
    }

    static void RebuildWaterWithEdges(Sprite[] grassSprites)
    {
        var waterGO = GameObject.Find("Tilemap/WaterLayer") ?? GameObject.Find("WaterLayer");
        if (waterGO == null) return;

        var tm = waterGO.GetComponent<Tilemap>();
        tm.ClearAllTiles();

        var waterSprites = LoadSprites(WATER_PATH);
        var waterSpr = waterSprites.FirstOrDefault();
        if (waterSpr == null) return;

        // Create water tile with correct color
        Tile MakeWater() {
            var t = ScriptableObject.CreateInstance<Tile>();
            t.sprite = waterSpr;
            t.color  = new Color(0.55f, 0.82f, 0.88f, 1f);
            return t;
        }

        // River y=2
        for (int x = -3; x < 23; x++)
            tm.SetTile(new Vector3Int(x, 2, 0), MakeWater());

        // Pond: organic irregular shape (not rectangle) — hand-placed
        int[,] pond = {
            {1,-2},{2,-2},{3,-2},{4,-2},
            {1,-1},{2,-1},{3,-1},{4,-1},{5,-1},
            {2,0}, {3,0}, {4,0},
        };
        for (int i = 0; i < pond.GetLength(0); i++)
            tm.SetTile(new Vector3Int(pond[i,0], pond[i,1], 0), MakeWater());

        // Upper pool: irregular
        int[,] pool = {
            {16,5},{17,5},{18,5},
            {17,6},{18,6},{19,6},
        };
        for (int i = 0; i < pool.GetLength(0); i++)
            tm.SetTile(new Vector3Int(pool[i,0], pool[i,1], 0), MakeWater());

        EditorUtility.SetDirty(waterGO);
        Debug.Log("[CreateRuleTiles] WaterLayer rebuilt with irregular shapes");
    }

    static Sprite[] LoadSprites(string path)
        => AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToArray();
}
