using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

/// Imports FocusFarmSpriteSheet.png with correct settings and slices it at 16x16 grid.
/// Then places trees, bushes, rocks, bridge, crops into the scene to match reference image.
public class ImportFocusFarmSheet
{
    const string SHEET_PATH = "Assets/Textures/FocusFarmSpriteSheet.png";
    const int PPU  = 16;
    const int CELL = 16;

    [MenuItem("Tools/Import Focus Farm Sheet")]
    public static void Execute()
    {
        // ── 1. Import settings ──────────────────────────────────────────────
        var ti = AssetImporter.GetAtPath(SHEET_PATH) as TextureImporter;
        if (ti == null) { Debug.LogError("FocusFarmSpriteSheet.png not found at " + SHEET_PATH); return; }

        ti.textureType         = TextureImporterType.Sprite;
        ti.spriteImportMode    = SpriteImportMode.Multiple;
        ti.spritePixelsPerUnit = PPU;
        ti.filterMode          = FilterMode.Point;
        ti.textureCompression  = TextureImporterCompression.Uncompressed;
        ti.mipmapEnabled       = false;
        ti.alphaIsTransparency = true;

        // ── 2. Auto-slice 16×16 grid ────────────────────────────────────────
        // Get texture dimensions via import settings (before reimport)
        int texW = 1365, texH = 768;

        var metas = new List<SpriteMetaData>();
        int idx = 0;
        for (int row = 0; row < texH / CELL; row++)
        for (int col = 0; col < texW / CELL; col++)
        {
            // Unity rect: origin = bottom-left
            float x = col * CELL;
            float y = texH - (row + 1) * CELL;
            metas.Add(new SpriteMetaData
            {
                name      = $"FFS_{idx:000}",
                rect      = new Rect(x, y, CELL, CELL),
                pivot     = new Vector2(0.5f, 0.5f),
                alignment = (int)SpriteAlignment.Center,
            });
            idx++;
        }

        ti.spritesheet = metas.ToArray();
        ti.SaveAndReimport();
        AssetDatabase.Refresh();

        Debug.Log($"[FocusFarm] Imported {metas.Count} sprites ({texW / CELL}×{texH / CELL} grid)");

        PlaceSceneObjects();
    }

    // ── Sprite helpers ──────────────────────────────────────────────────────
    static Sprite[] _allSprites;

    static Sprite[] AllSprites()
    {
        if (_allSprites == null)
            _allSprites = AssetDatabase.LoadAllAssetsAtPath(SHEET_PATH).OfType<Sprite>().ToArray();
        return _allSprites;
    }

    // row/col in the sheet grid (row 0 = top, col 0 = left)
    static Sprite S(int row, int col) =>
        AllSprites().FirstOrDefault(s => s.name == $"FFS_{row * (1365 / CELL) + col:000}");

    // Build a multi-cell sprite by blending — returns first cell (we'll use individual SpriteRenderers)
    static GameObject MakeSR(string name, Transform parent, Sprite spr, Vector3 worldPos, int order = 0, float scale = 1f)
    {
        if (spr == null) return null;
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position   = worldPos;
        go.transform.localScale = Vector3.one * scale;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite       = spr;
        sr.sortingOrder = order;
        return go;
    }

    // ── Place decorations in scene ──────────────────────────────────────────
    static void PlaceSceneObjects()
    {
        // Find or create a "Decorations" parent
        var decoGO = GameObject.Find("Decorations");
        if (decoGO != null) Object.DestroyImmediate(decoGO);
        decoGO = new GameObject("Decorations");

        var all = AllSprites();
        if (all.Length == 0) { Debug.LogError("[FocusFarm] No sprites loaded — run again after import."); return; }

        int cols = 1365 / CELL; // 85 columns

        // ── Trees (Environment Objects section, top of sheet ~rows 0-5, cols ~18-40)
        // Large green tree: rows 0-4, cols 18-23 (6-wide, 5-tall = 96×80px)
        // We approximate with 4 single-cell reads from the canopy
        PlaceTree(decoGO.transform, "Tree_Left1",    cols, -2f,  5.5f, 5);
        PlaceTree(decoGO.transform, "Tree_Left2",    cols,  0f,  6f,   5);
        PlaceTree(decoGO.transform, "Tree_Right1",   cols, 18f,  5f,   5);
        PlaceTree(decoGO.transform, "Tree_Right2",   cols, 20f,  6.5f, 5);
        PlaceTree(decoGO.transform, "Tree_Right3",   cols, 22f,  5f,   5);

        // ── Apple tree (rows 0-4, cols ~30-35) — right side near pond
        PlaceAppleTree(decoGO.transform, "AppleTree", cols, 16f, 5.5f, 5);

        // ── Bushes / shrubs (small, rows ~6-8 area)
        PlaceBush(decoGO.transform, "Bush1", cols,  3f, 1.5f, 3);
        PlaceBush(decoGO.transform, "Bush2", cols,  6f, 1.5f, 3);
        PlaceBush(decoGO.transform, "Bush3", cols, 12f, 1.5f, 3);
        PlaceBush(decoGO.transform, "Bush4", cols, 15f, 2f,   3);
        PlaceBush(decoGO.transform, "Bush5", cols, 19f, 1.5f, 3);

        // ── Rocks near river bank
        PlaceRock(decoGO.transform, "Rock1", cols, 14f, 3.5f, 3);
        PlaceRock(decoGO.transform, "Rock2", cols, 15f, 3f,   3);
        PlaceRock(decoGO.transform, "Rock3", cols, 16f, 3.5f, 3);

        // ── Bridge over river (row 2, y≈2 in world)
        PlaceBridge(decoGO.transform, cols, 9f, 2f);

        // ── Crop patch (3×3 pumpkins) near center
        PlaceCrops(decoGO.transform, cols, 10f, 4f);

        EditorUtility.SetDirty(decoGO);
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log("[FocusFarm] Scene decorations placed.");
    }

    // ── Tree: use a 2-wide tall sprite from Environment section ─────────────
    static void PlaceTree(Transform parent, string id, int cols, float wx, float wy, int order)
    {
        // Green tree canopy top — approx row 1, col 19 (large tree top-left of env section)
        var top  = S(1, 19);
        var trunk = S(4, 19);
        if (top   != null) MakeSR(id + "_top",   parent, top,   new Vector3(wx, wy,       0), order,     3f);
        if (trunk != null) MakeSR(id + "_trunk",  parent, trunk, new Vector3(wx, wy - 1.5f, 0), order - 1, 2f);
    }

    static void PlaceAppleTree(Transform parent, string id, int cols, float wx, float wy, int order)
    {
        var top   = S(1, 30);
        var trunk = S(4, 30);
        if (top   != null) MakeSR(id + "_top",   parent, top,   new Vector3(wx, wy,       0), order,     3f);
        if (trunk != null) MakeSR(id + "_trunk",  parent, trunk, new Vector3(wx, wy - 1.5f, 0), order - 1, 2f);
    }

    static void PlaceBush(Transform parent, string id, int cols, float wx, float wy, int order)
    {
        // Small bush — row 6, col 22 area
        var spr = S(6, 22);
        if (spr != null) MakeSR(id, parent, spr, new Vector3(wx, wy, 0), order, 2f);
    }

    static void PlaceRock(Transform parent, string id, int cols, float wx, float wy, int order)
    {
        // Rock cluster — row 7, col 26 area
        var spr = S(7, 26);
        if (spr != null) MakeSR(id, parent, spr, new Vector3(wx, wy, 0), order, 2f);
    }

    static void PlaceBridge(Transform parent, int cols, float wx, float wy)
    {
        // Bridge section — Structure section row ~16, cols ~0-8
        // Place 3 bridge planks across the river
        for (int i = 0; i < 3; i++)
        {
            var spr = S(16, i * 2);
            if (spr != null)
                MakeSR($"Bridge_{i}", parent, spr, new Vector3(wx + i * 1f, wy, 0), 4, 2f);
        }
    }

    static void PlaceCrops(Transform parent, int cols, float wx, float wy)
    {
        // Crops section — row ~32, cols ~40-50 area
        for (int cx = 0; cx < 3; cx++)
        for (int cy = 0; cy < 3; cy++)
        {
            var spr = S(32, 40 + cx);
            if (spr != null)
                MakeSR($"Crop_{cx}_{cy}", parent, spr,
                    new Vector3(wx + cx * 0.7f, wy + cy * 0.7f, 0), 3, 1.5f);
        }
    }
}
