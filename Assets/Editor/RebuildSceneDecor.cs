#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;
using System.Collections.Generic;

/// Rebuilds scene decorations: pond (water tilemap) + trees/bushes/flowers around the grid.
public static class RebuildSceneDecor
{
    public static void Execute()
    {
        // ── 1. Pond: add WaterLayer tilemap ──────────────────────────────────
        BuildPond();

        // ── 2. Decorations: remove old, respawn with variety ─────────────────
        RebuildDecorations();

        EditorUtility.SetDirty(GameObject.Find("Tilemap"));
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log("[RebuildSceneDecor] Done.");
    }

    // ── Pond ──────────────────────────────────────────────────────────────────
    static void BuildPond()
    {
        var tilemapRoot = GameObject.Find("Tilemap");
        if (tilemapRoot == null) { Debug.LogError("No Tilemap root found"); return; }

        // Remove existing WaterLayer if any
        var existing = tilemapRoot.transform.Find("WaterLayer");
        if (existing != null) Object.DestroyImmediate(existing.gameObject);

        // Create WaterLayer child
        var waterGO = new GameObject("WaterLayer");
        waterGO.transform.SetParent(tilemapRoot.transform, false);
        var tm  = waterGO.AddComponent<Tilemap>();
        var tmr = waterGO.AddComponent<TilemapRenderer>();
        tmr.sortingOrder = -8; // above grass (-10), below dirt (-5)

        // Load water sprite and wrap in a Tile asset
        var waterSprite = FindSprite("Water", "Water_0");
        if (waterSprite == null) { Debug.LogWarning("Water_0 sprite not found"); return; }

        var tile = ScriptableObject.CreateInstance<Tile>();
        tile.sprite = waterSprite;
        tile.color  = new Color(0.45f, 0.72f, 0.90f, 1f);

        // Paint a 3×2 pond in the lower-left area of the grid (grid starts at 0,0)
        // Pond sits just below the farm grid (y = -1, -2) and x = 2..4
        int[,] pondCells = {
            {2,-2}, {3,-2}, {4,-2},
            {2,-1}, {3,-1}, {4,-1},
        };
        for (int i = 0; i < pondCells.GetLength(0); i++)
            tm.SetTile(new Vector3Int(pondCells[i,0], pondCells[i,1], 0), tile);

        Debug.Log("[RebuildSceneDecor] Pond built at y=-1,-2");
    }

    // ── Decorations ───────────────────────────────────────────────────────────
    static void RebuildDecorations()
    {
        // Clear old Decorations container
        var decorRoot = GameObject.Find("Decorations");
        if (decorRoot != null)
        {
            // Remove all children
            for (int i = decorRoot.transform.childCount - 1; i >= 0; i--)
                Object.DestroyImmediate(decorRoot.transform.GetChild(i).gameObject);
        }
        else
        {
            decorRoot = new GameObject("Decorations");
        }

        // Load sprite pools
        var biom   = LoadAll("Basic_Grass_Biom_things"); // 0-44: mushrooms, flowers, ferns
        var biom1  = LoadAll("Basic Grass Biom things 1"); // more variety
        if ((biom == null || biom.Length == 0) && (biom1 == null || biom1.Length == 0))
        {
            Debug.LogWarning("[RebuildSceneDecor] No biom sprites found");
            return;
        }

        // Combine pools
        var allBiom = new List<Sprite>();
        if (biom  != null) allBiom.AddRange(biom);
        if (biom1 != null) allBiom.AddRange(biom1);
        var pool = allBiom.ToArray();

        var rng = new System.Random(99);

        // Grid bounds: x[0,20], y[0,4]. Camera shows roughly x[-4,24], y[-2,7]
        float gridX0 = 0f, gridX1 = 20f;
        float gridY0 = 0f, gridY1 = 4f;

        // ── Trees: top edge, above grid ──────────────────────────────────────
        // Use larger sprites (indices 27-35 in biom tend to be tree-like)
        int[] treeIdx = { 27, 28, 29, 36, 37, 38, 39, 40, 41 };
        PlaceRandom(decorRoot.transform, pool, treeIdx, rng,
            xMin: gridX0 - 1f, xMax: gridX1 + 1f,
            yMin: gridY1 + 0.2f, yMax: gridY1 + 2.0f,
            count: 14, scale: 1.3f, prefix: "Tree", order: 2);

        // ── Bushes: bottom edge, below grid ──────────────────────────────────
        int[] bushIdx = { 18, 19, 20, 21, 22, 23, 24, 25, 26 };
        PlaceRandom(decorRoot.transform, pool, bushIdx, rng,
            xMin: gridX0 - 1f, xMax: gridX1 + 1f,
            yMin: gridY0 - 1.8f, yMax: gridY0 - 0.3f,
            count: 10, scale: 1.1f, prefix: "Bush", order: 1);

        // ── Flowers left: left of grid ────────────────────────────────────────
        int[] flowerIdx = { 0, 1, 2, 3, 4, 5, 6, 7, 8 };
        PlaceRandom(decorRoot.transform, pool, flowerIdx, rng,
            xMin: gridX0 - 2.5f, xMax: gridX0 - 0.3f,
            yMin: gridY0, yMax: gridY1,
            count: 7, scale: 1.0f, prefix: "FlowerL", order: 1);

        // ── Flowers right: right of grid ──────────────────────────────────────
        PlaceRandom(decorRoot.transform, pool, flowerIdx, rng,
            xMin: gridX1 + 0.3f, xMax: gridX1 + 2.5f,
            yMin: gridY0, yMax: gridY1,
            count: 7, scale: 1.0f, prefix: "FlowerR", order: 1);

        // ── Scattered grass tufts inside/around grid (sparse) ─────────────────
        int[] tuffIdx = { 9, 10, 11, 12, 13, 14, 15, 16, 17 };
        PlaceRandom(decorRoot.transform, pool, tuffIdx, rng,
            xMin: gridX0, xMax: gridX1,
            yMin: gridY1 + 0.1f, yMax: gridY1 + 1.2f,
            count: 8, scale: 0.9f, prefix: "Tuft", order: 0);

        // ── Pond-side reeds (near pond at x≈2-4, y≈-1 to 0) ──────────────────
        int[] reedIdx = { 3, 4, 5, 12, 13, 14 };
        PlaceRandom(decorRoot.transform, pool, reedIdx, rng,
            xMin: 1.0f, xMax: 5.5f,
            yMin: -0.8f, yMax: 0.5f,
            count: 5, scale: 0.8f, prefix: "Reed", order: 1);

        Debug.Log("[RebuildSceneDecor] Decorations rebuilt.");
    }

    static void PlaceRandom(Transform parent, Sprite[] pool, int[] indices,
        System.Random rng, float xMin, float xMax, float yMin, float yMax,
        int count, float scale, string prefix, int order)
    {
        for (int i = 0; i < count; i++)
        {
            int si = indices[rng.Next(indices.Length)];
            if (si >= pool.Length) continue;
            var spr = pool[si];
            if (spr == null) continue;

            float sprU = spr.rect.width / spr.pixelsPerUnit;
            float x = Mathf.Lerp(xMin, xMax, (float)rng.NextDouble());
            float y = Mathf.Lerp(yMin, yMax, (float)rng.NextDouble());
            float scl = scale / sprU;

            var go = new GameObject($"{prefix}_{i}");
            go.transform.SetParent(parent, false);
            go.transform.position   = new Vector3(x, y, 0f);
            go.transform.localScale = Vector3.one * scl;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite       = spr;
            sr.sortingOrder = order;
        }
    }

    static Sprite FindSprite(string sheetHint, string spriteName)
    {
        string[] guids = AssetDatabase.FindAssets(sheetHint + " t:Texture2D",
            new[] { "Assets/Sprout Lands - Sprites - Basic pack" });
        foreach (var g in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(g);
            foreach (var a in AssetDatabase.LoadAllAssetsAtPath(path))
                if (a is Sprite s && s.name == spriteName) return s;
        }
        return null;
    }

    static Sprite[] LoadAll(string nameHint)
    {
        string[] guids = AssetDatabase.FindAssets(nameHint + " t:Texture2D",
            new[] { "Assets/Sprout Lands - Sprites - Basic pack" });
        foreach (var g in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(g);
            var list = new List<Sprite>();
            foreach (var a in AssetDatabase.LoadAllAssetsAtPath(path))
                if (a is Sprite s) list.Add(s);
            if (list.Count > 0) return list.ToArray();
        }
        return null;
    }
}
#endif
