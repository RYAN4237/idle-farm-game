using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// Builds a layered farm scene: grass ground → pond → tilled-dirt plots →
/// trees / bushes / flowers scattered around the border.
/// Uses Sprout Lands Basic Pack sprites loaded via AssetDatabase (editor) or Resources (runtime).
public class FarmBackground : MonoBehaviour
{
    [Header("Grid (auto-found if null)")]
    public FarmGrid farmGrid;

    [Header("Sorting layers")]
    public int groundSort     = -20;
    public int waterSort      = -15;
    public int tilledSort     = -10;
    public int decorSort      =  -5;

    // Pond: a 3×2 tile rectangle placed left of centre
    static readonly Vector2Int PondOffset = new Vector2Int(1, 1);
    const int PondW = 3, PondH = 2;

    void Start()
    {
        if (farmGrid == null) farmGrid = FarmGrid.Instance;
        if (farmGrid == null) { Debug.LogWarning("[FarmBackground] No FarmGrid found."); return; }

        BuildGround();
        BuildPond();
        BuildTilledDirt();
        BuildDecorations();
    }

    // ── 1. Grass ground tiles (covers grid + 1-cell border) ──────────────────
    void BuildGround()
    {
        var tile = Load("Grass", "Grass_0") ?? MakeSolid(new Color(0.45f, 0.70f, 0.30f));
        float cs = farmGrid.cellSize;
        float sprU = tile.rect.width / tile.pixelsPerUnit;
        float scale = cs / sprU;

        int cols = farmGrid.gridWidth  + 2;
        int rows = farmGrid.gridHeight + 2;
        float ox = farmGrid.originX - cs;
        float oy = farmGrid.originY - cs;

        var root = new GameObject("GroundTiles");
        root.transform.SetParent(transform, false);

        // Grass variants for variety: 0,1,2,3 are clean tiles
        Sprite[] variants = { Load("Grass","Grass_0"), Load("Grass","Grass_1"),
                               Load("Grass","Grass_2"), Load("Grass","Grass_3") };

        for (int c = 0; c < cols; c++)
        for (int r = 0; r < rows; r++)
        {
            int vi = (c * 3 + r * 7) % 4; // deterministic variety
            var spr = variants[vi] ?? tile;
            float sU = spr.rect.width / spr.pixelsPerUnit;
            Spawn($"G{c}_{r}", root.transform,
                  new Vector3(ox + c*cs + cs*0.5f, oy + r*cs + cs*0.5f, 0.1f),
                  Vector3.one * (cs / sU), spr, groundSort);
        }
    }

    // ── 2. Pond (water tiles) ─────────────────────────────────────────────────
    void BuildPond()
    {
        var water = Load("Water", "Water_0");
        if (water == null) { Debug.LogWarning("[FarmBackground] Water_0 not found"); return; }

        float cs   = farmGrid.cellSize;
        float sprU = water.rect.width / water.pixelsPerUnit;   // 64px wide sprite

        var root = new GameObject("Pond");
        root.transform.SetParent(transform, false);

        float baseX = farmGrid.originX + PondOffset.x * cs;
        float baseY = farmGrid.originY - cs * 1.5f; // below the grid row

        for (int px = 0; px < PondW; px++)
        for (int py = 0; py < PondH; py++)
        {
            float x = baseX + px * cs + cs * 0.5f;
            float y = baseY + py * cs * 0.6f;
            // scale water tile to fit one cell width
            float scl = cs / sprU;
            Spawn($"W{px}_{py}", root.transform,
                  new Vector3(x, y, 0.05f), Vector3.one * scl, water, waterSort);
        }
    }

    // ── 3. Tilled dirt strips (2 short rows above and below grid centre) ──────
    void BuildTilledDirt()
    {
        var dirt = Load("Tilled_Dirt", "Tilled_Dirt_0")
                ?? Load("Tilled_Dirt_v2", "Tilled_Dirt_v2_0")
                ?? MakeSolid(new Color(0.55f, 0.38f, 0.18f));

        float cs   = farmGrid.cellSize;
        float sprU = dirt.rect.width / dirt.pixelsPerUnit;

        var root = new GameObject("TilledDirt");
        root.transform.SetParent(transform, false);

        // Two horizontal strips: rows 1 and 2 of the grid (y offset inside grid)
        int[] dirtRows = { 1, 2 };
        // Skip the pond columns
        for (int r = 0; r < dirtRows.Length; r++)
        {
            float y = farmGrid.originY + dirtRows[r] * cs + cs * 0.5f;
            for (int c = 0; c < farmGrid.gridWidth; c++)
            {
                // leave pond columns empty
                if (c >= PondOffset.x && c < PondOffset.x + PondW &&
                    dirtRows[r] >= PondOffset.y && dirtRows[r] < PondOffset.y + PondH)
                    continue;

                float x = farmGrid.originX + c * cs + cs * 0.5f;
                float scl = cs / sprU;
                Spawn($"D{c}_{r}", root.transform,
                      new Vector3(x, y, 0.02f), Vector3.one * scl, dirt, tilledSort);
            }
        }
    }

    // ── 4. Decorations: trees, bushes, flowers around border ─────────────────
    void BuildDecorations()
    {
        // Tree-like sprites: indices 0-8 in Basic_Grass_Biom_things are larger objects
        // (mushrooms, flowers, ferns). Use a curated subset for visual variety.
        int[] treeIdx  = { 0, 1, 2, 9, 10, 18, 19, 27, 28 };
        int[] bushIdx  = { 3, 4, 5, 11, 12, 20, 21, 29, 30 };
        int[] flowerIdx= { 6, 7, 8, 13, 14, 22, 23, 31, 32 };

        var root = new GameObject("Decorations");
        root.transform.SetParent(transform, false);

        float cs  = farmGrid.cellSize;
        float ox  = farmGrid.originX;
        float oy  = farmGrid.originY;
        float gw  = farmGrid.gridWidth  * cs;
        float gh  = farmGrid.gridHeight * cs;

        // Pre-load all biom sprites
        var biom = LoadAll("Basic_Grass_Biom_things");
        if (biom == null || biom.Length == 0) return;

        var rng = new System.Random(42); // deterministic seed

        // Top row: trees
        PlaceRow(root.transform, biom, treeIdx, rng,
                 ox, ox + gw, oy + gh + cs * 0.3f, oy + gh + cs * 0.9f,
                 12, cs, "Tree", decorSort + 1);

        // Bottom row: bushes
        PlaceRow(root.transform, biom, bushIdx, rng,
                 ox, ox + gw, oy - cs * 0.9f, oy - cs * 0.2f,
                 10, cs, "Bush", decorSort);

        // Left column: flowers
        PlaceCol(root.transform, biom, flowerIdx, rng,
                 ox - cs * 0.9f, ox - cs * 0.1f, oy, oy + gh,
                 6, cs, "FlowerL", decorSort);

        // Right column: flowers
        PlaceCol(root.transform, biom, flowerIdx, rng,
                 ox + gw + cs * 0.1f, ox + gw + cs * 0.9f, oy, oy + gh,
                 6, cs, "FlowerR", decorSort);
    }

    void PlaceRow(Transform parent, Sprite[] pool, int[] indices, System.Random rng,
                  float xMin, float xMax, float yMin, float yMax,
                  int count, float cs, string prefix, int order)
    {
        for (int i = 0; i < count; i++)
        {
            int si  = indices[rng.Next(indices.Length)];
            if (si >= pool.Length) continue;
            var spr = pool[si];
            float sprU = spr.rect.width / spr.pixelsPerUnit;
            float x = Mathf.Lerp(xMin, xMax, (float)rng.NextDouble());
            float y = Mathf.Lerp(yMin, yMax, (float)rng.NextDouble());
            float scl = cs * 1.1f / sprU;
            Spawn($"{prefix}_{i}", parent,
                  new Vector3(x, y, 0f), Vector3.one * scl, spr, order);
        }
    }

    void PlaceCol(Transform parent, Sprite[] pool, int[] indices, System.Random rng,
                  float xMin, float xMax, float yMin, float yMax,
                  int count, float cs, string prefix, int order)
    {
        for (int i = 0; i < count; i++)
        {
            int si  = indices[rng.Next(indices.Length)];
            if (si >= pool.Length) continue;
            var spr = pool[si];
            float sprU = spr.rect.width / spr.pixelsPerUnit;
            float x = Mathf.Lerp(xMin, xMax, (float)rng.NextDouble());
            float y = Mathf.Lerp(yMin, yMax, (float)rng.NextDouble());
            float scl = cs * 1.0f / sprU;
            Spawn($"{prefix}_{i}", parent,
                  new Vector3(x, y, 0f), Vector3.one * scl, spr, order);
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    static void Spawn(string name, Transform parent, Vector3 pos, Vector3 scale,
                      Sprite spr, int order)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position   = pos;
        go.transform.localScale = scale;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite       = spr;
        sr.sortingOrder = order;
    }

    static Sprite Load(string sheet, string name)
    {
#if UNITY_EDITOR
        // Try AssetDatabase first for exact sub-sprite
        string[] guids = AssetDatabase.FindAssets(sheet + " t:Texture2D",
                             new[]{"Assets/Sprout Lands - Sprites - Basic pack"});
        foreach (var g in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(g);
            var all  = AssetDatabase.LoadAllAssetsAtPath(path);
            foreach (var a in all)
                if (a is Sprite s && s.name == name) return s;
        }
#endif
        // Runtime fallback via Resources
        var res = Resources.LoadAll<Sprite>(sheet);
        if (res != null)
            foreach (var s in res)
                if (s.name == name) return s;
        return null;
    }

    static Sprite[] LoadAll(string sheet)
    {
#if UNITY_EDITOR
        string[] guids = AssetDatabase.FindAssets(sheet + " t:Texture2D",
                             new[]{"Assets/Sprout Lands - Sprites - Basic pack"});
        foreach (var g in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(g);
            var list = new List<Sprite>();
            foreach (var a in AssetDatabase.LoadAllAssetsAtPath(path))
                if (a is Sprite s) list.Add(s);
            if (list.Count > 0) return list.ToArray();
        }
#endif
        return Resources.LoadAll<Sprite>(sheet);
    }

    static Sprite MakeSolid(Color c)
    {
        var tex = new Texture2D(1, 1) { filterMode = FilterMode.Point };
        tex.SetPixel(0, 0, c);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
    }
}
