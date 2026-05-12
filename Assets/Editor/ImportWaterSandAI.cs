using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using UnityEditor.U2D.Sprites;
using System.Linq;
using System.IO;
using System.Collections.Generic;

/// Imports WaterSand_AI.png (192x96, 4 water + 4 sand tiles, 48px each)
/// and replaces Map_Water + Map_Sand tilemaps.
public class ImportWaterSandAI
{
    const string PATH     = "Assets/Sprites/WaterSand_AI.png";
    const string TILE_DIR = "Assets/Tiles3";
    const int    T        = 48;
    const int    COLS     = 4;   // 4 per row
    // Row 0 = water (y index 1 in Unity coords = top of texture)
    // Row 1 = sand  (y index 0 in Unity coords = bottom of texture)
    // NOTE: Unity flips Y for texture rects — row0 of PNG = bottom rect, row1 = top rect.
    // Our PNG layout: row0(y=0..47)=water, row1(y=48..95)=sand.
    // In Rect coords (origin bottom-left): water y=48, sand y=0.

    [MenuItem("Tools/Import & Apply WaterSand AI")]
    public static void Execute()
    {
        // 1. Configure importer
        var ti = AssetImporter.GetAtPath(PATH) as TextureImporter;
        if (ti == null) { Debug.LogError($"[WaterSandAI] No TextureImporter at {PATH}"); return; }

        ti.textureType         = TextureImporterType.Sprite;
        ti.spriteImportMode    = SpriteImportMode.Multiple;
        ti.spritePixelsPerUnit = T;
        ti.filterMode          = FilterMode.Point;
        ti.textureCompression  = TextureImporterCompression.Uncompressed;
        ti.isReadable          = true;
        EditorUtility.SetDirty(ti);
        ti.SaveAndReimport();

        // 2. Slice via ISpriteEditorDataProvider
        var factory = new SpriteDataProviderFactories();
        factory.Init();
        var dp = factory.GetSpriteEditorDataProviderFromObject(ti);
        dp.InitSpriteEditorDataProvider();

        // PNG is 192×96. Unity rect origin = bottom-left.
        // Row 0 (PNG top, water): pixel rows 0-47 from top = rect y = 96-48 = 48
        // Row 1 (PNG bottom, sand): pixel rows 48-95 from top = rect y = 0
        var spriteRects = new List<SpriteRect>();
        int texH = 96;
        string[] rowNames = { "Water", "Sand" };
        int[] rowPngY   = { 0,  48 }; // top pixel of each row in PNG (y=0 = top)
        for (int row = 0; row < 2; row++)
        {
            int rectY = texH - rowPngY[row] - T; // convert top-of-PNG to Unity bottom-left rect
            for (int col = 0; col < COLS; col++)
            {
                spriteRects.Add(new SpriteRect
                {
                    name      = $"WS_{rowNames[row]}_{col}",
                    rect      = new Rect(col * T, rectY, T, T),
                    pivot     = Vector2.one * 0.5f,
                    alignment = SpriteAlignment.Center,
                    spriteID  = GUID.Generate(),
                });
            }
        }

        dp.SetSpriteRects(spriteRects.ToArray());
        dp.Apply();
        (dp.targetObject as AssetImporter).SaveAndReimport();

        var sprites = AssetDatabase.LoadAllAssetsAtPath(PATH).OfType<Sprite>().ToArray();
        Debug.Log($"[WaterSandAI] {sprites.Length} sprites loaded: {string.Join(", ", sprites.Select(s => s.name))}");
        if (sprites.Length != 8) { Debug.LogError("[WaterSandAI] Expected 8 sprites!"); return; }

        // 3. Build tile assets
        if (!Directory.Exists(TILE_DIR)) Directory.CreateDirectory(TILE_DIR);
        var mat = FindUnlitMat();

        Sprite[] waterSprites = new Sprite[COLS];
        Sprite[] sandSprites  = new Sprite[COLS];
        for (int col = 0; col < COLS; col++)
        {
            waterSprites[col] = sprites.First(s => s.name == $"WS_Water_{col}");
            sandSprites[col]  = sprites.First(s => s.name == $"WS_Sand_{col}");
        }

        Tile[] waterTiles = MakeTiles(waterSprites, "WSWater");
        Tile[] sandTiles  = MakeTiles(sandSprites,  "WSSand");
        AssetDatabase.SaveAssets();

        // 4. Apply to Map_Water
        var waterGo = GameObject.Find("Tilemap/Map_Water");
        if (waterGo != null)
        {
            var tm = waterGo.GetComponent<Tilemap>();
            var tr = waterGo.GetComponent<TilemapRenderer>();
            if (mat != null) tr.sharedMaterial = mat;
            foreach (var pos in tm.cellBounds.allPositionsWithin)
            {
                if (!tm.HasTile(pos)) continue;
                int idx = ((pos.x * 3 + pos.y) & 0x7) % COLS;
                tm.SetTile(pos, waterTiles[idx]);
            }
            tm.RefreshAllTiles();
            Debug.Log("[WaterSandAI] Map_Water updated");
        }
        else Debug.LogWarning("[WaterSandAI] Map_Water not found");

        // 5. Apply to Map_Sand
        var sandGo = GameObject.Find("Tilemap/Map_Sand");
        if (sandGo != null)
        {
            var tm = sandGo.GetComponent<Tilemap>();
            var tr = sandGo.GetComponent<TilemapRenderer>();
            if (mat != null) tr.sharedMaterial = mat;
            foreach (var pos in tm.cellBounds.allPositionsWithin)
            {
                if (!tm.HasTile(pos)) continue;
                int idx = ((pos.x * 5) & 0x7) % COLS;
                tm.SetTile(pos, sandTiles[idx]);
            }
            tm.RefreshAllTiles();
            Debug.Log("[WaterSandAI] Map_Sand updated");
        }
        else Debug.LogWarning("[WaterSandAI] Map_Sand not found");

        Debug.Log("[WaterSandAI] Done!");
    }

    static Tile[] MakeTiles(Sprite[] sprites, string prefix)
    {
        var tiles = new Tile[sprites.Length];
        for (int i = 0; i < sprites.Length; i++)
        {
            var sp = sprites[i];
            string tp = $"{TILE_DIR}/{prefix}_{i}.asset";
            var t = AssetDatabase.LoadAssetAtPath<Tile>(tp);
            if (t == null) { t = ScriptableObject.CreateInstance<Tile>(); t.name = $"{prefix}_{i}"; AssetDatabase.CreateAsset(t, tp); }
            t.sprite = sp;
            EditorUtility.SetDirty(t);
            tiles[i] = t;
        }
        return tiles;
    }

    static Material FindUnlitMat()
    {
        foreach (var guid in AssetDatabase.FindAssets("Sprite-Unlit-Default t:Material"))
        {
            var p = AssetDatabase.GUIDToAssetPath(guid);
            if (p.Contains("com.unity.render-pipelines")) return AssetDatabase.LoadAssetAtPath<Material>(p);
        }
        return null;
    }
}
