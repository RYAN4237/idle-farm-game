using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using System.IO;

/// Creates a two-layer Tilemap for the farm:
///   Layer -2: Grass background (Sprout Lands Grass.png, Grass_0 sprite)
///   Layer -1: Tilled Dirt overlay on farm grid cells (Tilled_Dirt.png)
/// Run via: Farm > Build Sprout Lands Tilemap
public class BuildSproutLandsTilemap
{
    const string SPROUT  = "Assets/Sprout Lands - Sprites - Basic pack/Tilesets/";
    const string TILES   = "Assets/Data/Tiles/";   // where generated .asset tiles go

    [MenuItem("Farm/Build Sprout Lands Tilemap")]
    public static void Execute()
    {
        EnsureTileDir();
        ConfigureSpriteSheets();

        // Remove old
        DestroyIfExists("SproutLandsTilemap");

        var root = BuildTilemapRoot();
        var grassLayer = AddTilemapLayer(root, "GrassLayer",  -12, new Color(1,1,1,1));
        var dirtLayer  = AddTilemapLayer(root, "DirtLayer",   -10, new Color(1,1,1,1));

        FillGrass(grassLayer);
        FillDirt(dirtLayer);

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();
        Debug.Log("[BuildSproutLandsTilemap] Done.");
    }

    // ── Helpers ──────────────────────────────────────────────────────
    static void EnsureTileDir()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Data"))
            AssetDatabase.CreateFolder("Assets", "Data");
        if (!AssetDatabase.IsValidFolder(TILES.TrimEnd('/')))
            AssetDatabase.CreateFolder("Assets/Data", "Tiles");
    }

    static void DestroyIfExists(string name)
    {
        var go = GameObject.Find(name);
        if (go != null) { Object.DestroyImmediate(go); Debug.Log($"Removed old {name}"); }
    }

    // Ensure Sprout Lands sheets are imported as Multiple/Point sprites
    static void ConfigureSpriteSheets()
    {
        var sheets = new (string path, int tileW, int tileH)[]
        {
            (SPROUT + "Grass.png",        16, 16),   // note: non-uniform — we just need any sprite
            (SPROUT + "Tilled_Dirt.png",  16, 16),
        };

        foreach (var (path, tw, th) in sheets)
        {
            var ti = AssetImporter.GetAtPath(path) as TextureImporter;
            if (ti == null) continue;
            if (ti.spriteImportMode == SpriteImportMode.Multiple &&
                ti.filterMode       == FilterMode.Point) continue;  // already done

            ti.textureType         = TextureImporterType.Sprite;
            ti.spriteImportMode    = SpriteImportMode.Multiple;
            ti.filterMode          = FilterMode.Point;
            ti.textureCompression  = TextureImporterCompression.Uncompressed;
            ti.alphaIsTransparency = true;
            ti.mipmapEnabled       = false;
            // Keep existing sprite cuts (don't overwrite if already sliced)
            EditorUtility.SetDirty(ti);
            ti.SaveAndReimport();
        }
    }

    static GameObject BuildTilemapRoot()
    {
        var root = new GameObject("SproutLandsTilemap");

        // FarmGrid: originX=-19.2, originY=-2.56, cellSize=1.28
        // Tilemap cell = 16px sprite at PPU=16 → 1 world unit per cell
        // We need cell size = 1.28 → use Grid cell size (1.28, 1.28, 0)
        var grid = root.AddComponent<Grid>();
        grid.cellSize = new Vector3(1.28f, 1.28f, 0f);

        // Position so z=0.5 (behind farm plots at z=0)
        root.transform.position = new Vector3(0, 0, 0.5f);
        return root;
    }

    static Tilemap AddTilemapLayer(GameObject root, string name, int sortOrder, Color tint)
    {
        var go = new GameObject(name);
        go.transform.SetParent(root.transform, false);
        var tm = go.AddComponent<Tilemap>();
        tm.color = tint;
        var tr = go.AddComponent<TilemapRenderer>();
        tr.sortingOrder = sortOrder;
        return tm;
    }

    // ── Grass layer: fill entire farm area + 2-cell border ───────────
    static void FillGrass(Tilemap tm)
    {
        // FarmGrid params
        float originX = -19.2f, originY = -2.56f, cs = 1.28f;
        int   gW = 30, gH = 4;

        // Load Grass_0 sprite → make Tile asset
        var spr = LoadSprite(SPROUT + "Grass.png", "Grass_0");
        if (spr == null)
        {
            Debug.LogWarning("[Tilemap] Grass_0 not found. Trying first available grass sprite.");
            spr = LoadFirstSprite(SPROUT + "Grass.png");
        }
        if (spr == null) { Debug.LogError("[Tilemap] No Grass sprite found!"); return; }

        var tile = GetOrCreateTile("Grass_0_tile", spr);

        // Extend 2 cells beyond grid on each side
        int border = 2;
        for (int cx = -border; cx < gW + border; cx++)
        for (int cy = -border; cy < gH + border; cy++)
        {
            // Convert grid cell → world → tilemap cell
            float wx = originX + cx * cs + cs * 0.5f;
            float wy = originY + cy * cs + cs * 0.5f;
            var tilePos = tm.WorldToCell(new Vector3(wx, wy, 0));
            tm.SetTile(tilePos, tile);
        }
        Debug.Log($"[Tilemap] Grass layer filled: {(gW+2*border)*(gH+2*border)} cells");
    }

    // ── Tilled Dirt layer: only on farm grid cells ────────────────────
    static void FillDirt(Tilemap tm)
    {
        float originX = -19.2f, originY = -2.56f, cs = 1.28f;
        int   gW = 30, gH = 4;

        // Tilled_Dirt_0 = plain center fill tile
        var spr = LoadSprite(SPROUT + "Tilled_Dirt.png", "Tilled_Dirt_0");
        if (spr == null) spr = LoadFirstSprite(SPROUT + "Tilled_Dirt.png");
        if (spr == null) { Debug.LogError("[Tilemap] No Tilled_Dirt sprite found!"); return; }

        var tile = GetOrCreateTile("TilledDirt_0_tile", spr);

        for (int cx = 0; cx < gW; cx++)
        for (int cy = 0; cy < gH; cy++)
        {
            float wx = originX + cx * cs + cs * 0.5f;
            float wy = originY + cy * cs + cs * 0.5f;
            var tilePos = tm.WorldToCell(new Vector3(wx, wy, 0));
            tm.SetTile(tilePos, tile);
        }
        Debug.Log($"[Tilemap] Tilled Dirt layer filled: {gW*gH} cells");
    }

    // ── Tile Asset management ─────────────────────────────────────────
    static Tile GetOrCreateTile(string assetName, Sprite spr)
    {
        string assetPath = TILES + assetName + ".asset";
        var existing = AssetDatabase.LoadAssetAtPath<Tile>(assetPath);
        if (existing != null)
        {
            existing.sprite = spr;
            EditorUtility.SetDirty(existing);
            return existing;
        }

        var tile = ScriptableObject.CreateInstance<Tile>();
        tile.sprite     = spr;
        tile.colliderType = Tile.ColliderType.None;
        AssetDatabase.CreateAsset(tile, assetPath);
        return tile;
    }

    static Sprite LoadSprite(string texPath, string spriteName)
    {
        foreach (var obj in AssetDatabase.LoadAllAssetsAtPath(texPath))
            if (obj is Sprite s && s.name == spriteName) return s;
        return null;
    }

    static Sprite LoadFirstSprite(string texPath)
    {
        foreach (var obj in AssetDatabase.LoadAllAssetsAtPath(texPath))
            if (obj is Sprite s) return s;
        return null;
    }
}
