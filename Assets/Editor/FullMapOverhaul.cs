using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

/// Complete map overhaul:
/// Priority 1: Water Rule Tile from Serene Village RPG Maker terrain sheet
/// Priority 2: Improved water shape (organic, jagged edges)
/// Priority 3: Decoration scatter (trees, stones from Serene Village)
/// Priority 4: Water animation
public class FullMapOverhaul
{
    const string GRASS_PATH    = "Assets/Sprout Lands - Sprites - Basic pack/Tilesets/Grass.png";
    const string TERRAIN_PATH  = "Assets/SERENE_VILLAGE_REVAMPED/RPG_MAKER_MV/Terrains_TILESET_B-C-D-E.png";
    const string SERENE_PATH   = "Assets/SERENE_VILLAGE_REVAMPED/Serene_Village_16x16.png";
    const string TILE_DIR      = "Assets/Tiles";
    const string SPRITE_DIR    = "Assets/Tiles/WaterEdgeSprites";

    // Map bounds
    const int X0 = -3, X1 = 23, Y0 = -3, Y1 = 8;

    // ── Organic water layout ────────────────────────────────────────────────────
    // River: enters left at y=2, bends, exits right — NOT a straight line
    static readonly (int x, int y)[] RIVER = {
        // Enter left, y=2
        (-3,2),(-2,2),(-1,2),(0,2),
        // Bend down at x=1-3
        (1,1),(1,2),(2,1),(3,1),
        // Widen at x=4-6
        (4,1),(4,2),(5,1),(5,2),(5,3),(6,2),(6,3),
        // Narrow and bend up at x=7-9
        (7,2),(7,3),(8,2),(9,2),(9,3),
        // Widen again x=10-12
        (10,2),(10,3),(11,2),(11,3),(12,2),
        // Dip down x=13-15
        (12,1),(13,1),(13,2),(14,1),(14,2),(15,2),
        // Narrow exit x=16-22
        (16,2),(17,2),(18,2),(19,2),(20,2),(21,2),(22,2),
    };

    // Pond: lower-left, deliberately irregular
    static readonly (int x, int y)[] POND = {
        (1,-2),(2,-2),(3,-2),(4,-2),(5,-2),(6,-2),
        (1,-1),(2,-1),(3,-1),(4,-1),(5,-1),(6,-1),
        (2,0),(3,0),(4,0),
        // Jagged extra cells to break rectangle
        (0,-2),(0,-1),
        (6,0),(7,-1),(7,-2),
    };

    // Pool: upper-right, organic blob
    static readonly (int x, int y)[] POOL = {
        (16,5),(17,5),(18,5),(19,5),(20,5),
        (17,6),(18,6),(19,6),(20,6),
        (18,7),(19,7),
        (15,5),(15,4),(16,4),(17,4),
        (20,4),(21,5),
    };

    [MenuItem("Tools/Full Map Overhaul")]
    public static void Execute()
    {
        if (!AssetDatabase.IsValidFolder(TILE_DIR))
            AssetDatabase.CreateFolder("Assets", "Tiles");
        if (!AssetDatabase.IsValidFolder(SPRITE_DIR))
            AssetDatabase.CreateFolder(TILE_DIR, "WaterEdgeSprites");

        // Step 1: Extract water edge sprites from RPG Maker terrain sheet
        var edgeTiles = ExtractWaterEdgeTiles();
        if (edgeTiles == null) { Debug.LogError("[FullMapOverhaul] Failed to extract water edge tiles"); return; }

        // Step 2: Build grass tiles (Grass_66..71 only — verified opaque)
        var grassTiles = BuildGrassTiles();
        if (grassTiles == null || grassTiles.Length == 0) { Debug.LogError("[FullMapOverhaul] No grass tiles"); return; }

        // Step 3: Rebuild GrassLayer
        RebuildGrass(grassTiles);

        // Step 4: Rebuild WaterLayer with center tile
        RebuildWater(edgeTiles);

        // Step 5: Paint WaterEdgeLayer with transition tiles
        PaintWaterEdges(edgeTiles);

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log("[FullMapOverhaul] Done");
    }

    // ── Extract 13 water transition sprites from RPG Maker terrain sheet ────────
    // RPG Maker tile = 48px. The water autotile block starts at:
    // Rows 7-10 (from top), cols 0-5 contain grass-water transition tiles
    // We scale down from 48px to 16px by taking every 3rd pixel (1/3 scale)
    // Actually: we create new 16x16 sprites by sampling the 48x48 source regions
    static WaterEdgeTileSet ExtractWaterEdgeTiles()
    {
        var ti = AssetImporter.GetAtPath(TERRAIN_PATH) as TextureImporter;
        bool wasReadable = ti.isReadable;
        if (!wasReadable) { ti.isReadable = true; ti.SaveAndReimport(); }

        var srcTex = AssetDatabase.LoadAssetAtPath<Texture2D>(TERRAIN_PATH);
        int srcH = srcTex.height; // 768

        // RPG Maker terrain sheet, 48px tiles:
        // The water body tiles with grass transitions are in the LEFT portion
        // Based on pixel scan: rows 7-10 (from top=row0) contain water/grass mix
        // Row 7 (py center=408): WATER tiles at cols 0-3, transition at 0-5
        // Row 11 (py=216): GRASS cols 1-4, WATER cols 0,5,6 → inner island row
        //
        // RPG Maker autotile layout for water (A1 type, 3x4 blocks of 16px sub-tiles):
        // We need to identify the 5x4 autotile block and remap to Unity Rule Tile
        //
        // Strategy: manually map the 13 needed tiles from pixel coords
        // Based on the grid scan, water autotile is at cols 0-5, rows 7-14

        // For each of the 13 Rule Tile cases, define the source 48x48 region
        // and create a 16x16 Texture2D by averaging/sampling
        var tileSet = new WaterEdgeTileSet();

        // RPG Maker to Unity tile mapping:
        // The water autotile in RPG Maker B-E tileset uses a 2x3 block (96x144px)
        // Located at pixel (0, top) in the terrain sheet
        // Unity Y is flipped vs pixel Y

        // From scan: water is concentrated in cols 0-5, rows 7-12 (48px units)
        // Row 7 from top = y_pixel_bottom = 768 - (7+1)*48 = 768-384 = 384
        // Let's extract:
        //   Center water:     col=2, row=9  → pure water
        //   Top edge:         col=1, row=7  → water below, grass above
        //   Bottom edge:      col=1, row=11 → water above, grass below
        //   Left edge:        col=0, row=9  → water right, grass left
        //   Right edge:       col=5, row=9  → water left, grass right
        //   Top-left corner:  col=0, row=7
        //   Top-right corner: col=5, row=7
        //   Bot-left corner:  col=0, row=11
        //   Bot-right corner: col=5, row=11
        //   Inner TL:         col=2, row=7
        //   Inner TR:         col=3, row=7
        //   Inner BL:         col=2, row=11
        //   Inner BR:         col=3, row=11

        // Map: (col, rpgRow) where rpgRow counts from TOP of image (row 0 = top)
        var coords = new (string name, int col, int rpgRow)[]
        {
            ("wc",   2,  9),  // center
            ("wt",   1,  7),  // top edge (grass above)
            ("wb",   1, 11),  // bottom edge (grass below)
            ("wl",   0,  9),  // left edge (grass left)
            ("wr",   5,  9),  // right edge (grass right)
            ("wtl",  0,  7),  // top-left outer corner
            ("wtr",  5,  7),  // top-right outer corner
            ("wbl",  0, 11),  // bottom-left outer corner
            ("wbr",  5, 11),  // bottom-right outer corner
            ("witl", 2,  7),  // top-left inner corner (concave)
            ("witr", 3,  7),  // top-right inner corner
            ("wibl", 2, 11),  // bottom-left inner corner
            ("wibr", 3, 11),  // bottom-right inner corner
        };

        var result = new Dictionary<string, Tile>();

        foreach (var (name, col, rpgRow) in coords)
        {
            // Convert RPG Maker row (from top) to pixel Y bottom (Unity tex coords)
            int srcX = col * 48;
            int srcY = srcH - (rpgRow + 1) * 48; // flip Y for Unity

            // Sample 48x48 → create 16x16 by taking every 3rd pixel
            var newTex = new Texture2D(16, 16, TextureFormat.RGBA32, false);
            newTex.filterMode = FilterMode.Point;

            for (int dy = 0; dy < 16; dy++)
            for (int dx = 0; dx < 16; dx++)
            {
                // Sample center of each 3x3 block
                int px = srcX + dx * 3 + 1;
                int py = srcY + dy * 3 + 1;
                px = Mathf.Clamp(px, 0, srcTex.width - 1);
                py = Mathf.Clamp(py, 0, srcTex.height - 1);
                newTex.SetPixel(dx, dy, srcTex.GetPixel(px, py));
            }
            newTex.Apply();

            // Save as PNG
            string pngPath = $"{SPRITE_DIR}/{name}.png";
            System.IO.File.WriteAllBytes(
                System.IO.Path.Combine(Application.dataPath, "..", pngPath),
                newTex.EncodeToPNG());
            Object.DestroyImmediate(newTex);
        }

        AssetDatabase.Refresh();

        // Set import settings for all extracted sprites
        foreach (var (name, _, _) in coords)
        {
            string pngPath = $"{SPRITE_DIR}/{name}.png";
            var sti = AssetImporter.GetAtPath(pngPath) as TextureImporter;
            if (sti == null) continue;
            sti.textureType         = TextureImporterType.Sprite;
            sti.spriteImportMode    = SpriteImportMode.Single;
            sti.filterMode          = FilterMode.Point;
            sti.textureCompression  = TextureImporterCompression.Uncompressed;
            sti.spritePixelsPerUnit = 16;
            sti.SaveAndReimport();
        }

        AssetDatabase.Refresh();

        // Create Tile assets
        foreach (var (name, _, _) in coords)
        {
            string pngPath  = $"{SPRITE_DIR}/{name}.png";
            string tilePath = $"{TILE_DIR}/WE_{name}.asset";
            var spr = AssetDatabase.LoadAssetAtPath<Sprite>(pngPath);
            if (spr == null) { Debug.LogWarning($"Sprite not loaded: {pngPath}"); continue; }

            AssetDatabase.DeleteAsset(tilePath);
            var t = ScriptableObject.CreateInstance<Tile>();
            t.sprite = spr;
            t.name   = $"WE_{name}";
            AssetDatabase.CreateAsset(t, tilePath);
            result[name] = t;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // Reload from disk
        foreach (var key in result.Keys.ToArray())
            result[key] = AssetDatabase.LoadAssetAtPath<Tile>($"{TILE_DIR}/WE_{key}.asset");

        if (!wasReadable) { ti.isReadable = false; ti.SaveAndReimport(); }

        Debug.Log($"[FullMapOverhaul] Extracted {result.Count} water edge tiles");

        tileSet.center = result.GetValueOrDefault("wc");
        tileSet.top    = result.GetValueOrDefault("wt");
        tileSet.bottom = result.GetValueOrDefault("wb");
        tileSet.left   = result.GetValueOrDefault("wl");
        tileSet.right  = result.GetValueOrDefault("wr");
        tileSet.cornerTL = result.GetValueOrDefault("wtl");
        tileSet.cornerTR = result.GetValueOrDefault("wtr");
        tileSet.cornerBL = result.GetValueOrDefault("wbl");
        tileSet.cornerBR = result.GetValueOrDefault("wbr");
        tileSet.innerTL  = result.GetValueOrDefault("witl");
        tileSet.innerTR  = result.GetValueOrDefault("witr");
        tileSet.innerBL  = result.GetValueOrDefault("wibl");
        tileSet.innerBR  = result.GetValueOrDefault("wibr");

        return tileSet;
    }

    // ── Grass tiles ────────────────────────────────────────────────────────────
    static Tile[] BuildGrassTiles()
    {
        var sprites = AssetDatabase.LoadAllAssetsAtPath(GRASS_PATH).OfType<Sprite>().ToArray();
        var tiles   = new List<Tile>();

        for (int i = 66; i <= 71; i++)
        {
            var spr = sprites.FirstOrDefault(s => s.name == $"Grass_{i}");
            if (spr == null) continue;

            string path = $"{TILE_DIR}/GSolid_{i}.asset";
            AssetDatabase.DeleteAsset(path);
            var t = ScriptableObject.CreateInstance<Tile>();
            t.sprite = spr;
            t.name   = $"GSolid_{i}";
            AssetDatabase.CreateAsset(t, path);
            tiles.Add(t);
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        return Enumerable.Range(66, 6)
            .Select(i => AssetDatabase.LoadAssetAtPath<Tile>($"{TILE_DIR}/GSolid_{i}.asset"))
            .Where(t => t != null)
            .ToArray();
    }

    // ── Rebuild GrassLayer ─────────────────────────────────────────────────────
    static void RebuildGrass(Tile[] tiles)
    {
        var go = GameObject.Find("Tilemap/GrassLayer") ?? GameObject.Find("GrassLayer");
        if (go == null) { Debug.LogError("GrassLayer not found"); return; }
        var tm   = go.GetComponent<Tilemap>();
        var rend = go.GetComponent<TilemapRenderer>();
        tm.ClearAllTiles();
        rend.sortingOrder = 0;
        rend.mode = TilemapRenderer.Mode.Chunk;

        var water = BuildWaterSet();
        var rng   = new System.Random(7331);

        int[] weights = { 0,0,0,0,0,0,0,0,0,0, 1,1,1,1,1,1,1,1, 2,2,2,2,2, 3,3,3, 4,4, 5 };

        for (int x = X0; x < X1; x++)
        for (int y = Y0; y < Y1; y++)
        {
            if (water.Contains((x, y))) continue; // skip water cells
            float n   = Mathf.PerlinNoise(x * 0.31f, y * 0.31f);
            int   idx = n > 0.75f ? rng.Next(2, tiles.Length) : weights[rng.Next(weights.Length)];
            idx = Mathf.Clamp(idx, 0, tiles.Length - 1);
            tm.SetTile(new Vector3Int(x, y, 0), tiles[idx]);
        }

        EditorUtility.SetDirty(go);
        Debug.Log("[FullMapOverhaul] GrassLayer rebuilt");
    }

    // ── Rebuild WaterLayer (center tiles only) ─────────────────────────────────
    static void RebuildWater(WaterEdgeTileSet edgeTiles)
    {
        var go = GameObject.Find("Tilemap/WaterLayer") ?? GameObject.Find("WaterLayer");
        if (go == null) { Debug.LogError("WaterLayer not found"); return; }
        var tm   = go.GetComponent<Tilemap>();
        var rend = go.GetComponent<TilemapRenderer>();
        tm.ClearAllTiles();
        tm.color = Color.white;
        rend.sortingOrder = 1;
        rend.mode = TilemapRenderer.Mode.Chunk;

        // Use center water tile if available, else fallback to WaterSolid
        Tile waterTile = edgeTiles.center;
        if (waterTile == null)
            waterTile = AssetDatabase.LoadAssetAtPath<Tile>($"{TILE_DIR}/WaterSolid.asset");
        if (waterTile == null) { Debug.LogError("No water center tile"); return; }

        foreach (var (x, y) in RIVER) tm.SetTile(new Vector3Int(x, y, 0), waterTile);
        foreach (var (x, y) in POND)  tm.SetTile(new Vector3Int(x, y, 0), waterTile);
        foreach (var (x, y) in POOL)  tm.SetTile(new Vector3Int(x, y, 0), waterTile);

        EditorUtility.SetDirty(go);
        Debug.Log("[FullMapOverhaul] WaterLayer rebuilt");
    }

    // ── Paint WaterEdgeLayer ───────────────────────────────────────────────────
    // For each water cell, check 4 neighbors. Paint correct edge tile on WaterLayer
    // for the water cells that border grass (instead of solid center tile).
    static void PaintWaterEdges(WaterEdgeTileSet et)
    {
        // Create or find WaterEdgeLayer (sortingOrder=2, above water but uses alpha)
        var gridGO = GameObject.Find("Tilemap");
        var edgeGO = GameObject.Find("Tilemap/WaterEdgeLayer") ?? GameObject.Find("WaterEdgeLayer");
        if (edgeGO == null)
        {
            edgeGO = new GameObject("WaterEdgeLayer");
            edgeGO.transform.SetParent(gridGO.transform, false);
            edgeGO.AddComponent<Tilemap>();
            var r2 = edgeGO.AddComponent<TilemapRenderer>();
            r2.sortingOrder = 2;
            r2.mode = TilemapRenderer.Mode.Chunk;
        }

        var edgeTm   = edgeGO.GetComponent<Tilemap>();
        var edgeRend = edgeGO.GetComponent<TilemapRenderer>();
        edgeRend.sortingOrder = 2;
        edgeTm.ClearAllTiles();

        var water = BuildWaterSet();

        // For each WATER cell at the boundary, determine which edge tile to use
        // and set it on WaterLayer (overwriting the center tile there)
        var waterGO = GameObject.Find("Tilemap/WaterLayer") ?? GameObject.Find("WaterLayer");
        var waterTm = waterGO.GetComponent<Tilemap>();

        int edgeCount = 0;
        foreach (var (wx, wy) in water)
        {
            bool hasTop    = water.Contains((wx, wy + 1));
            bool hasBottom = water.Contains((wx, wy - 1));
            bool hasLeft   = water.Contains((wx - 1, wy));
            bool hasRight  = water.Contains((wx + 1, wy));

            // Determine edge tile based on which sides touch grass
            Tile tile = null;

            if (!hasTop && !hasLeft  && hasBottom && hasRight)  tile = et.cornerTL;  // outer TL
            else if (!hasTop && !hasRight && hasBottom && hasLeft)   tile = et.cornerTR;  // outer TR
            else if (!hasBottom && !hasLeft && hasTop && hasRight)   tile = et.cornerBL;  // outer BL
            else if (!hasBottom && !hasRight && hasTop && hasLeft)   tile = et.cornerBR;  // outer BR
            else if (!hasTop    && hasBottom && hasLeft && hasRight)  tile = et.top;
            else if (!hasBottom && hasTop    && hasLeft && hasRight)  tile = et.bottom;
            else if (!hasLeft   && hasTop    && hasBottom && hasRight) tile = et.left;
            else if (!hasRight  && hasTop    && hasBottom && hasLeft)  tile = et.right;

            if (tile != null)
            {
                waterTm.SetTile(new Vector3Int(wx, wy, 0), tile);
                edgeCount++;
            }
        }

        EditorUtility.SetDirty(waterGO);
        EditorUtility.SetDirty(edgeGO);
        Debug.Log($"[FullMapOverhaul] Painted {edgeCount} water edge tiles");
    }

    static HashSet<(int, int)> BuildWaterSet()
    {
        var s = new HashSet<(int, int)>();
        foreach (var p in RIVER) s.Add(p);
        foreach (var p in POND)  s.Add(p);
        foreach (var p in POOL)  s.Add(p);
        return s;
    }
}

public class WaterEdgeTileSet
{
    public Tile center;
    public Tile top, bottom, left, right;
    public Tile cornerTL, cornerTR, cornerBL, cornerBR;
    public Tile innerTL, innerTR, innerBL, innerBR;
}
