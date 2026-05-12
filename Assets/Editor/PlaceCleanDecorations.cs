using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

/// Places scene decorations using individually extracted clean PNG sprites.
/// Sprites are in Assets/Textures/FFS_*.png — each is a full object with alpha.
public class PlaceCleanDecorations
{
    const string TEX_DIR = "Assets/Textures";
    const float PPU = 16f;

    [MenuItem("Tools/Place Clean Decorations")]
    public static void Execute()
    {
        EnsureAllImportSettings();
        AssetDatabase.Refresh();

        var old = GameObject.Find("Decorations");
        if (old != null) Object.DestroyImmediate(old);
        var root = new GameObject("Decorations");

        // ── Trees ─────────────────────────────────────────────────
        // Green tree: 176x192px at PPU=16 → 11×12 world units, base touches y=0
        PlaceSprite(root.transform, "GreenTree_L", "FFS_GreenTree", -1.5f, 5.5f, 3);
        PlaceSprite(root.transform, "GreenTree_R", "FFS_GreenTree", 20.5f, 5.5f, 3);

        // Apple tree: 144x144px → 9×9 world units
        PlaceSprite(root.transform, "AppleTree", "FFS_AppleTree", 17f, 5f, 3);

        // ── Bushes / Plants ───────────────────────────────────────
        // River runs y=2, grass bank above y=3. Bushes placed on upper bank (y=4).
        // Bush: 48x48px at PPU=16 → 3×3 world units
        PlaceSprite(root.transform, "Bush1", "FFS_Bush",  2f,  4f, 5);
        PlaceSprite(root.transform, "Bush2", "FFS_Bush2", 5f,  4f, 5);
        PlaceSprite(root.transform, "Bush3", "FFS_Bush",  14f, 4f, 5);
        PlaceSprite(root.transform, "Bush4", "FFS_Bush2", 18f, 4f, 5);
        PlaceSprite(root.transform, "Bush5", "FFS_Bush",  21f, 4f, 5);

        // ── Flowers ───────────────────────────────────────────────
        PlaceSprite(root.transform, "Flower1", "FFS_Flower", 4f,  4f, 6, 0.8f);
        PlaceSprite(root.transform, "Flower2", "FFS_Flower", 11f, 4f, 6, 0.8f);
        PlaceSprite(root.transform, "Flower3", "FFS_Flower", 19f, 4f, 6, 0.8f);

        // ── Rocks ─────────────────────────────────────────────────
        // Near river bank, below the river (lower grass area, y=1)
        PlaceSprite(root.transform, "Rocks1", "FFS_Rock", 15f,  1f, 5);
        PlaceSprite(root.transform, "Rocks2", "FFS_Rock", 16.8f, 0.8f, 5, 0.8f);

        // ── Bridge ────────────────────────────────────────────────
        // Bridge deck: 80x64px → 5×4 world units. River center y=2.
        PlaceSprite(root.transform, "BridgeDeck", "FFS_Bridge",     8.5f, 2.5f, 7);
        // Bridge railings: 224x32px → 14×2 world units. Sits on top of deck.
        PlaceSprite(root.transform, "BridgeFull", "FFS_BridgeFull", 8.5f, 3.2f, 8, 0.7f);

        // ── Wheat crops ───────────────────────────────────────────
        // Wheat: 64x64px → 4×4 world units, in field above river
        PlaceSprite(root.transform, "Wheat1", "FFS_Wheat", 10f,  5.5f, 4);
        PlaceSprite(root.transform, "Wheat2", "FFS_Wheat", 11.5f, 5.5f, 4);
        PlaceSprite(root.transform, "Wheat3", "FFS_Wheat", 13f,  5.5f, 4);

        EditorUtility.SetDirty(root);
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log("[PlaceCleanDecorations] Done.");
    }

    static void PlaceSprite(Transform parent, string id, string texName,
                             float wx, float wy, int order, float scale = 1f)
    {
        string path = $"{TEX_DIR}/{texName}.png";
        var spr = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (spr == null)
        {
            Debug.LogWarning($"[PlaceCleanDecorations] Missing: {path}");
            return;
        }

        var go = new GameObject(id);
        go.transform.SetParent(parent, false);
        go.transform.position   = new Vector3(wx, wy, 0f);
        go.transform.localScale = Vector3.one * scale;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite       = spr;
        sr.sortingOrder = order;

        EditorUtility.SetDirty(go);
    }

    static void EnsureAllImportSettings()
    {
        string[] textures = {
            "FFS_GreenTree", "FFS_AppleTree",
            "FFS_Bush", "FFS_Bush2", "FFS_Flower",
            "FFS_Rock", "FFS_Bridge", "FFS_BridgeFull",
            "FFS_Wheat", "FFS_Fence"
        };

        foreach (var name in textures)
        {
            string path = $"{TEX_DIR}/{name}.png";
            var ti = AssetImporter.GetAtPath(path) as TextureImporter;
            if (ti == null) continue;

            bool changed = false;
            if (ti.textureType != TextureImporterType.Sprite)
            { ti.textureType = TextureImporterType.Sprite; changed = true; }
            if (ti.spriteImportMode != SpriteImportMode.Single)
            { ti.spriteImportMode = SpriteImportMode.Single; changed = true; }
            if ((int)ti.spritePixelsPerUnit != (int)PPU)
            { ti.spritePixelsPerUnit = PPU; changed = true; }
            if (ti.filterMode != FilterMode.Point)
            { ti.filterMode = FilterMode.Point; changed = true; }
            if (ti.textureCompression != TextureImporterCompression.Uncompressed)
            { ti.textureCompression = TextureImporterCompression.Uncompressed; changed = true; }
            if (!ti.alphaIsTransparency)
            { ti.alphaIsTransparency = true; changed = true; }

            if (changed) ti.SaveAndReimport();
        }
    }
}
