using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

/// Generates pixel-perfect grass-water edge tiles procedurally.
/// Colors sampled directly from Sprout Lands Grass_66 and WaterSolid.
/// No external tileset dependency.
public class GenerateWaterEdgeTiles
{
    const string SPRITE_DIR = "Assets/Tiles/WaterEdgeSprites";
    const string TILE_DIR   = "Assets/Tiles";

    // Grass color (from Grass_66 pixel scan: RGBA 0.753, 0.831, 0.439)
    static readonly Color GRASS      = new Color(0.753f, 0.831f, 0.439f);
    static readonly Color GRASS_DARK = new Color(0.671f, 0.757f, 0.376f); // slightly darker shade
    static readonly Color WATER      = new Color(0.35f,  0.68f,  0.87f);  // our WaterSolid color
    static readonly Color WATER_DARK = new Color(0.25f,  0.55f,  0.78f);  // wave shadow
    static readonly Color EDGE       = new Color(0.42f,  0.58f,  0.22f);  // dark grass outline pixel

    // 16x16 tile patterns — 0=grass, 1=water, 2=edge(dark grass), 3=water_dark
    // Each string is 16 chars, 16 rows (top to bottom in visual space)
    // We define patterns for the 5 core types; corners are derived by rotation

    [MenuItem("Tools/Generate Water Edge Tiles")]
    public static void Execute()
    {
        if (!AssetDatabase.IsValidFolder(SPRITE_DIR))
            AssetDatabase.CreateFolder(TILE_DIR, "WaterEdgeSprites");

        // Delete old extracted tiles that used wrong source
        string[] oldNames = {"wc","wt","wb","wl","wr","wtl","wtr","wbl","wbr","witl","witr","wibl","wibr"};
        foreach (var n in oldNames)
        {
            AssetDatabase.DeleteAsset($"{SPRITE_DIR}/{n}.png");
            AssetDatabase.DeleteAsset($"{TILE_DIR}/WE_{n}.asset");
        }

        var tiles = new Dictionary<string, Tile>();

        // CENTER — all water
        tiles["wc"] = MakeTile("wc", (x, y) => WATER);

        // TOP EDGE — top half grass, bottom half water, edge line at y=8
        tiles["wt"] = MakeTile("wt", (x, y) => {
            if (y >= 9) return GRASS;
            if (y == 8) return EDGE;
            if (y == 7) return WATER_DARK;
            return WATER;
        });

        // BOTTOM EDGE — bottom half grass, top half water
        tiles["wb"] = MakeTile("wb", (x, y) => {
            if (y <= 6) return GRASS;
            if (y == 7) return EDGE;
            if (y == 8) return WATER_DARK;
            return WATER;
        });

        // LEFT EDGE — left half grass, right half water
        tiles["wl"] = MakeTile("wl", (x, y) => {
            if (x <= 6) return GRASS;
            if (x == 7) return EDGE;
            if (x == 8) return WATER_DARK;
            return WATER;
        });

        // RIGHT EDGE — right half grass, left half water
        tiles["wr"] = MakeTile("wr", (x, y) => {
            if (x >= 9) return GRASS;
            if (x == 8) return EDGE;
            if (x == 7) return WATER_DARK;
            return WATER;
        });

        // OUTER CORNER TL — water fills bottom-right, grass in top-left
        // Round corner shape
        tiles["wtl"] = MakeTile("wtl", (x, y) => {
            // Distance from top-left corner
            float d = Mathf.Sqrt(x * x + (15 - y) * (15 - y));
            if (d >= 10f) return WATER;
            if (d >= 9f)  return WATER_DARK;
            if (d >= 8f)  return EDGE;
            return GRASS;
        });

        // OUTER CORNER TR — water fills bottom-left, grass in top-right
        tiles["wtr"] = MakeTile("wtr", (x, y) => {
            float d = Mathf.Sqrt((15 - x) * (15 - x) + (15 - y) * (15 - y));
            if (d >= 10f) return WATER;
            if (d >= 9f)  return WATER_DARK;
            if (d >= 8f)  return EDGE;
            return GRASS;
        });

        // OUTER CORNER BL — water fills top-right, grass in bottom-left
        tiles["wbl"] = MakeTile("wbl", (x, y) => {
            float d = Mathf.Sqrt(x * x + y * y);
            if (d >= 10f) return WATER;
            if (d >= 9f)  return WATER_DARK;
            if (d >= 8f)  return EDGE;
            return GRASS;
        });

        // OUTER CORNER BR — water fills top-left, grass in bottom-right
        tiles["wbr"] = MakeTile("wbr", (x, y) => {
            float d = Mathf.Sqrt((15 - x) * (15 - x) + y * y);
            if (d >= 10f) return WATER;
            if (d >= 9f)  return WATER_DARK;
            if (d >= 8f)  return EDGE;
            return GRASS;
        });

        // INNER CORNER TL — mostly water, small grass patch at bottom-right
        tiles["witl"] = MakeTile("witl", (x, y) => {
            float d = Mathf.Sqrt((15 - x) * (15 - x) + y * y);
            if (d >= 7f)  return WATER;
            if (d >= 6f)  return WATER_DARK;
            if (d >= 5f)  return EDGE;
            return GRASS;
        });

        // INNER CORNER TR — mostly water, small grass patch at bottom-left
        tiles["witr"] = MakeTile("witr", (x, y) => {
            float d = Mathf.Sqrt(x * x + y * y);
            if (d >= 7f)  return WATER;
            if (d >= 6f)  return WATER_DARK;
            if (d >= 5f)  return EDGE;
            return GRASS;
        });

        // INNER CORNER BL — mostly water, small grass patch at top-right
        tiles["wibl"] = MakeTile("wibl", (x, y) => {
            float d = Mathf.Sqrt((15 - x) * (15 - x) + (15 - y) * (15 - y));
            if (d >= 7f)  return WATER;
            if (d >= 6f)  return WATER_DARK;
            if (d >= 5f)  return EDGE;
            return GRASS;
        });

        // INNER CORNER BR — mostly water, small grass patch at top-left
        tiles["wibr"] = MakeTile("wibr", (x, y) => {
            float d = Mathf.Sqrt(x * x + (15 - y) * (15 - y));
            if (d >= 7f)  return WATER;
            if (d >= 6f)  return WATER_DARK;
            if (d >= 5f)  return EDGE;
            return GRASS;
        });

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[GenerateWaterEdgeTiles] Created {tiles.Count} tiles");

        // Now rebuild the map using these new tiles
        RebuildWithNewTiles(tiles);
    }

    static Tile MakeTile(string name, System.Func<int, int, Color> colorFn)
    {
        var tex = new Texture2D(16, 16, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;

        for (int y = 0; y < 16; y++)
        for (int x = 0; x < 16; x++)
            tex.SetPixel(x, y, colorFn(x, y));

        tex.Apply();

        string pngPath = $"{SPRITE_DIR}/{name}.png";
        string absPath = System.IO.Path.Combine(
            System.IO.Path.GetDirectoryName(Application.dataPath),
            pngPath.Replace('/', System.IO.Path.DirectorySeparatorChar));
        System.IO.File.WriteAllBytes(absPath, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);

        AssetDatabase.ImportAsset(pngPath);
        var ti = AssetImporter.GetAtPath(pngPath) as TextureImporter;
        ti.textureType         = TextureImporterType.Sprite;
        ti.spriteImportMode    = SpriteImportMode.Single;
        ti.filterMode          = FilterMode.Point;
        ti.textureCompression  = TextureImporterCompression.Uncompressed;
        ti.spritePixelsPerUnit = 16;
        ti.SaveAndReimport();

        var spr = AssetDatabase.LoadAssetAtPath<Sprite>(pngPath);

        string tilePath = $"{TILE_DIR}/WE_{name}.asset";
        AssetDatabase.DeleteAsset(tilePath);
        var t = ScriptableObject.CreateInstance<Tile>();
        t.sprite = spr;
        t.name   = $"WE_{name}";
        AssetDatabase.CreateAsset(t, tilePath);

        return AssetDatabase.LoadAssetAtPath<Tile>(tilePath);
    }

    static void RebuildWithNewTiles(Dictionary<string, Tile> et)
    {
        // Map bounds
        const int X0 = -3, X1 = 23, Y0 = -3, Y1 = 8;

        var RIVER = new (int x, int y)[] {
            (-3,2),(-2,2),(-1,2),(0,2),
            (1,1),(1,2),(2,1),(3,1),
            (4,1),(4,2),(5,1),(5,2),(5,3),(6,2),(6,3),
            (7,2),(7,3),(8,2),(9,2),(9,3),
            (10,2),(10,3),(11,2),(11,3),(12,2),
            (12,1),(13,1),(13,2),(14,1),(14,2),(15,2),
            (16,2),(17,2),(18,2),(19,2),(20,2),(21,2),(22,2),
        };
        var POND = new (int x, int y)[] {
            (1,-2),(2,-2),(3,-2),(4,-2),(5,-2),(6,-2),
            (1,-1),(2,-1),(3,-1),(4,-1),(5,-1),(6,-1),
            (2,0),(3,0),(4,0),
            (0,-2),(0,-1),
            (6,0),(7,-1),(7,-2),
        };
        var POOL = new (int x, int y)[] {
            (16,5),(17,5),(18,5),(19,5),(20,5),
            (17,6),(18,6),(19,6),(20,6),
            (18,7),(19,7),
            (15,5),(15,4),(16,4),(17,4),
            (20,4),(21,5),
        };

        var water = new HashSet<(int,int)>();
        foreach (var p in RIVER) water.Add(p);
        foreach (var p in POND)  water.Add(p);
        foreach (var p in POOL)  water.Add(p);

        // Rebuild WaterLayer with correct edge tiles
        var waterGO = GameObject.Find("Tilemap/WaterLayer") ?? GameObject.Find("WaterLayer");
        var waterTm = waterGO.GetComponent<Tilemap>();
        waterTm.ClearAllTiles();
        waterTm.color = Color.white;

        var center = et.GetValueOrDefault("wc");

        foreach (var (wx, wy) in water)
        {
            bool T = water.Contains((wx,   wy+1));
            bool B = water.Contains((wx,   wy-1));
            bool L = water.Contains((wx-1, wy));
            bool R = water.Contains((wx+1, wy));

            Tile tile;
            // Outer corners (2 sides missing)
            if (!T && !L &&  B &&  R) tile = et.GetValueOrDefault("wtl");
            else if (!T && !R &&  B &&  L) tile = et.GetValueOrDefault("wtr");
            else if (!B && !L &&  T &&  R) tile = et.GetValueOrDefault("wbl");
            else if (!B && !R &&  T &&  L) tile = et.GetValueOrDefault("wbr");
            // Edges (1 side missing)
            else if (!T &&  B &&  L &&  R) tile = et.GetValueOrDefault("wt");
            else if (!B &&  T &&  L &&  R) tile = et.GetValueOrDefault("wb");
            else if (!L &&  T &&  B &&  R) tile = et.GetValueOrDefault("wl");
            else if (!R &&  T &&  B &&  L) tile = et.GetValueOrDefault("wr");
            // Lone tile or inner corners — use center
            else tile = center;

            waterTm.SetTile(new Vector3Int(wx, wy, 0), tile ?? center);
        }

        // Rebuild GrassLayer (skip water cells)
        var grassGO = GameObject.Find("Tilemap/GrassLayer") ?? GameObject.Find("GrassLayer");
        var grassTm = grassGO.GetComponent<Tilemap>();
        grassTm.ClearAllTiles();

        var grassSprites = AssetDatabase.LoadAllAssetsAtPath(
            "Assets/Sprout Lands - Sprites - Basic pack/Tilesets/Grass.png").OfType<Sprite>().ToArray();
        var grassTiles = Enumerable.Range(66, 6)
            .Select(i => AssetDatabase.LoadAssetAtPath<Tile>($"{TILE_DIR}/GSolid_{i}.asset"))
            .Where(t => t != null).ToArray();

        var rng = new System.Random(7331);
        int[] weights = {0,0,0,0,0,0,0,0,0,0, 1,1,1,1,1,1,1,1, 2,2,2,2,2, 3,3,3, 4,4, 5};
        for (int x = X0; x < X1; x++)
        for (int y = Y0; y < Y1; y++)
        {
            if (water.Contains((x, y))) continue;
            float n   = Mathf.PerlinNoise(x * 0.31f, y * 0.31f);
            int   idx = n > 0.75f ? rng.Next(2, grassTiles.Length) : weights[rng.Next(weights.Length)];
            grassTm.SetTile(new Vector3Int(x, y, 0), grassTiles[Mathf.Clamp(idx, 0, grassTiles.Length-1)]);
        }

        // Remove WaterEdgeLayer if it exists (no longer needed)
        var edgeGO = GameObject.Find("Tilemap/WaterEdgeLayer") ?? GameObject.Find("WaterEdgeLayer");
        if (edgeGO != null) Object.DestroyImmediate(edgeGO);

        EditorUtility.SetDirty(waterGO);
        EditorUtility.SetDirty(grassGO);
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log("[GenerateWaterEdgeTiles] Map rebuilt with procedural edge tiles");
    }
}
