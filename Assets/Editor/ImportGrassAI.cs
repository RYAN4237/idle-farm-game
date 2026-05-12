using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using UnityEditor.U2D.Sprites;
using System.Linq;
using System.IO;
using System.Collections.Generic;

public class ImportGrassAI
{
    const string PATH     = "Assets/Sprites/Grass_AI.png";
    const string TILE_DIR = "Assets/Tiles3";
    const int    T        = 48;
    const int    COLS     = 2;

    [MenuItem("Tools/Import & Apply Grass AI")]
    public static void Execute()
    {
        // 1. Configure importer
        var ti = AssetImporter.GetAtPath(PATH) as TextureImporter;
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

        var spriteRects = new List<SpriteRect>();
        for (int i = 0; i < COLS; i++)
        {
            spriteRects.Add(new SpriteRect
            {
                name      = $"GrassAI_{i}",
                rect      = new Rect(i * T, 0, T, T),
                pivot     = Vector2.one * 0.5f,
                alignment = SpriteAlignment.Center,
                spriteID  = GUID.Generate(),
            });
        }
        dp.SetSpriteRects(spriteRects.ToArray());
        dp.Apply();
        (dp.targetObject as AssetImporter).SaveAndReimport();

        var sprites = AssetDatabase.LoadAllAssetsAtPath(PATH).OfType<Sprite>().ToArray();
        Debug.Log($"[GrassAI] {sprites.Length} sprites loaded");
        if (sprites.Length == 0) { Debug.LogError("[GrassAI] No sprites found!"); return; }

        // 3. Build tiles
        if (!Directory.Exists(TILE_DIR)) Directory.CreateDirectory(TILE_DIR);
        var mat = FindUnlitMat();

        var tiles = new Tile[COLS];
        for (int i = 0; i < COLS; i++)
        {
            var sp = sprites.FirstOrDefault(s => s.name == $"GrassAI_{i}");
            if (sp == null) { Debug.LogError($"[GrassAI] sprite GrassAI_{i} missing"); return; }
            string tp = $"{TILE_DIR}/GrassAI_{i}.asset";
            var t = AssetDatabase.LoadAssetAtPath<Tile>(tp);
            if (t == null) { t = ScriptableObject.CreateInstance<Tile>(); t.name = $"GrassAI_{i}"; AssetDatabase.CreateAsset(t, tp); }
            t.sprite = sp;
            EditorUtility.SetDirty(t);
            tiles[i] = t;
        }
        AssetDatabase.SaveAssets();

        // 4. Replace grass on Map_Ground tilemap
        var tmGo = GameObject.Find("Tilemap/Map_Ground");
        if (tmGo == null) { Debug.LogError("[GrassAI] Map_Ground not found"); return; }
        var tm = tmGo.GetComponent<Tilemap>();
        var tr = tmGo.GetComponent<TilemapRenderer>();
        if (mat != null) tr.sharedMaterial = mat;

        foreach (var pos in tm.cellBounds.allPositionsWithin)
        {
            if (!tm.HasTile(pos)) continue;
            if (pos.y >= 0 && pos.y <= 3) continue;
            int idx = ((pos.x * 7 + pos.y * 13) & 0xFF) % COLS;
            tm.SetTile(pos, tiles[idx]);
        }

        Debug.Log("[GrassAI] Done!");
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
