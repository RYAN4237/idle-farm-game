using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

/// Places scene decorations using FocusFarmSpriteSheet sprites.
/// Sheet: 1365x768, 16px grid → 85×48 cells.
/// Cell index = row * 85 + col  (row 0=top, col 0=left)
public class PlaceFarmDecorations
{
    const string SHEET = "Assets/Textures/FocusFarmSpriteSheet.png";
    const int COLS = 85; // 1365/16

    [MenuItem("Tools/Place Farm Decorations")]
    public static void Execute()
    {
        _sprites = null; // reset cache

        // Wipe old Decorations group
        var old = GameObject.Find("Decorations");
        if (old != null) Object.DestroyImmediate(old);

        var root = new GameObject("Decorations");

        // ── TREES ─────────────────────────────────────────────────────────────
        // Green tree canopy: multi-cell, centered around cols 43-49, rows 5-12
        // We stack 3 rows of 4 sprites each (48×48px native = 3×3 world units at PPU=16)
        PlaceMultiSprite(root.transform, "GreenTree_L", new int[,]{
            {5,43},{5,44},{5,45},{5,46},
            {6,43},{6,44},{6,45},{6,46},
            {7,43},{7,44},{7,45},{7,46},
            {8,43},{8,44},{8,45},{8,46},
            {9,43},{9,44},{9,45},{9,46},
            {10,43},{10,44},{10,45},{10,46},
        }, -2f, 5.5f, 10);

        PlaceMultiSprite(root.transform, "GreenTree_R", new int[,]{
            {5,43},{5,44},{5,45},{5,46},
            {6,43},{6,44},{6,45},{6,46},
            {7,43},{7,44},{7,45},{7,46},
            {8,43},{8,44},{8,45},{8,46},
            {9,43},{9,44},{9,45},{9,46},
            {10,43},{10,44},{10,45},{10,46},
        }, 20f, 5.5f, 10);

        // Apple tree: cols 57-62, rows 4-10 (confirmed apple red)
        PlaceMultiSprite(root.transform, "AppleTree", new int[,]{
            {4,57},{4,58},{4,59},{4,60},
            {5,57},{5,58},{5,59},{5,60},
            {6,57},{6,58},{6,59},{6,60},
            {7,57},{7,58},{7,59},{7,60},
            {8,57},{8,58},{8,59},{8,60},
            {9,57},{9,58},{9,59},{9,60},
        }, 17f, 5f, 10);

        // ── BUSHES ────────────────────────────────────────────────────────────
        // Round green bushes: rows 21-22, cols 39-41 (2×2 cells each = 32×32px)
        PlaceMultiSprite(root.transform, "Bush1", new int[,]{
            {21,39},{21,40},{22,39},{22,40}
        }, 3f, 1.5f, 5);

        PlaceMultiSprite(root.transform, "Bush2", new int[,]{
            {21,42},{21,43},{22,42},{22,43}
        }, 5.5f, 1.5f, 5);

        PlaceMultiSprite(root.transform, "Bush3", new int[,]{
            {21,44},{21,45},{22,44},{22,45}
        }, 13f, 1.5f, 5);

        PlaceMultiSprite(root.transform, "Bush4", new int[,]{
            {21,39},{21,40},{22,39},{22,40}
        }, 17f, 2f, 5);

        PlaceMultiSprite(root.transform, "Bush5", new int[,]{
            {21,42},{21,43},{22,42},{22,43}
        }, 19.5f, 1.5f, 5);

        // ── FLOWER / SMALL PLANTS ─────────────────────────────────────────────
        // Red flower: row 21, col 47
        PlaceSingle(root.transform, "Flower1", 21, 47, 4f, 3f, 4);
        PlaceSingle(root.transform, "Flower2", 21, 47, 11f, 2.5f, 4);
        PlaceSingle(root.transform, "Flower3", 22, 48, 21f, 3f, 4);

        // ── ROCKS ─────────────────────────────────────────────────────────────
        // Rock cluster: rows 21-22, cols 73-75
        PlaceMultiSprite(root.transform, "Rocks1", new int[,]{
            {21,73},{21,74},{22,73},{22,74}
        }, 15f, 3.5f, 6);

        PlaceMultiSprite(root.transform, "Rocks2", new int[,]{
            {21,73},{21,74},{22,73},{22,74}
        }, 16.5f, 3f, 6);

        // ── BRIDGE ────────────────────────────────────────────────────────────
        // Bridge deck: rows 26-27, cols 1-8 (wide plank)
        // Bridge railings: rows 26-27, cols 9-12
        PlaceMultiSprite(root.transform, "BridgeDeck", new int[,]{
            {26,1},{26,2},{26,3},{26,4},
            {27,1},{27,2},{27,3},{27,4},
        }, 8f, 2.2f, 8);

        PlaceMultiSprite(root.transform, "BridgeRail_L", new int[,]{
            {26,9},{26,10},{27,9},{27,10}
        }, 7.5f, 2.8f, 9);

        PlaceMultiSprite(root.transform, "BridgeRail_R", new int[,]{
            {26,9},{26,10},{27,9},{27,10}
        }, 10.5f, 2.8f, 9);

        // ── WHEAT CROPS ───────────────────────────────────────────────────────
        // Wheat: rows 32-34, cols 39-41 (3×3 grid of 32×32 wheat sprites)
        PlaceMultiSprite(root.transform, "Wheat1", new int[,]{
            {32,39},{32,40},{33,39},{33,40}
        }, 10f, 4.5f, 6);

        PlaceMultiSprite(root.transform, "Wheat2", new int[,]{
            {32,41},{32,42},{33,41},{33,42}
        }, 11f, 4.5f, 6);

        PlaceMultiSprite(root.transform, "Wheat3", new int[,]{
            {32,43},{32,44},{33,43},{33,44}
        }, 12f, 4.5f, 6);

        PlaceMultiSprite(root.transform, "Wheat4", new int[,]{
            {32,39},{32,40},{33,39},{33,40}
        }, 10f, 5.5f, 6);

        PlaceMultiSprite(root.transform, "Wheat5", new int[,]{
            {32,41},{32,42},{33,41},{33,42}
        }, 11f, 5.5f, 6);

        PlaceMultiSprite(root.transform, "Wheat6", new int[,]{
            {32,43},{32,44},{33,43},{33,44}
        }, 12f, 5.5f, 6);

        // ── FENCE ─────────────────────────────────────────────────────────────
        // Use any fence sprite from bottom half
        // Fence pieces around crops: rows 28-29, cols 9-13
        for (int i = 0; i < 4; i++)
            PlaceSingle(root.transform, $"Fence_top_{i}", 28, 9 + (i % 2), 9.5f + i, 6.2f, 5);
        for (int i = 0; i < 4; i++)
            PlaceSingle(root.transform, $"Fence_bot_{i}", 29, 9 + (i % 2), 9.5f + i, 4.0f, 5);

        EditorUtility.SetDirty(root);
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log("[PlaceFarm] Decorations placed successfully.");
    }

    // ── Sprite cache ──────────────────────────────────────────────────────────
    static Sprite[] _sprites;
    static Sprite[] Sprites() =>
        _sprites ??= AssetDatabase.LoadAllAssetsAtPath(SHEET).OfType<Sprite>().ToArray();

    static Sprite GetSprite(int row, int col)
    {
        string name = $"FFS_{row * COLS + col:000}";
        return Sprites().FirstOrDefault(s => s.name == name);
    }

    // Place one cell as a SpriteRenderer child
    static void PlaceSingle(Transform parent, string id, int row, int col,
                             float wx, float wy, int order, float scale = 1f)
    {
        var spr = GetSprite(row, col);
        if (spr == null) return;
        var go = new GameObject(id);
        go.transform.SetParent(parent, false);
        go.transform.position    = new Vector3(wx, wy, 0f);
        go.transform.localScale  = Vector3.one * scale;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite       = spr;
        sr.sortingOrder = order;
        EditorUtility.SetDirty(go);
    }

    // Place a multi-cell object: each [row,col] entry is one SpriteRenderer tile
    // worldX/Y = center of the entire assembled object
    static void PlaceMultiSprite(Transform parent, string id, int[,] cells,
                                  float cx, float cy, int order)
    {
        int count = cells.GetLength(0);
        // Determine bounding box in cell space to compute offsets
        int minRow = int.MaxValue, maxRow = int.MinValue;
        int minCol = int.MaxValue, maxCol = int.MinValue;
        for (int i = 0; i < count; i++)
        {
            minRow = Mathf.Min(minRow, cells[i, 0]);
            maxRow = Mathf.Max(maxRow, cells[i, 0]);
            minCol = Mathf.Min(minCol, cells[i, 1]);
            maxCol = Mathf.Max(maxCol, cells[i, 1]);
        }
        float objW = (maxCol - minCol + 1); // in cells
        float objH = (maxRow - minRow + 1);

        var group = new GameObject(id);
        group.transform.SetParent(parent, false);
        group.transform.position = new Vector3(cx, cy, 0f);

        for (int i = 0; i < count; i++)
        {
            int r = cells[i, 0], c = cells[i, 1];
            var spr = GetSprite(r, c);
            if (spr == null) continue;

            // Offset from center: each cell = 1 world unit at PPU=16
            float ox = (c - minCol) - (objW - 1) * 0.5f;
            float oy = -((r - minRow) - (objH - 1) * 0.5f); // invert Y

            var tile = new GameObject($"{id}_{r}_{c}");
            tile.transform.SetParent(group.transform, false);
            tile.transform.localPosition = new Vector3(ox, oy, 0f);
            var sr = tile.AddComponent<SpriteRenderer>();
            sr.sprite       = spr;
            sr.sortingOrder = order;
        }
        EditorUtility.SetDirty(group);
    }
}
