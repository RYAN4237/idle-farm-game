using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

/// One-click full setup:
///   1. Configures Sprout Lands sprites in Resources as Multiple/PixelArt
///   2. Adds FarmBackground + FarmerCharacter to active scene
///   3. Injects sprite references directly into FarmerCharacter
///   4. Calls BuildSproutLandsTilemap + ApplySproutLandsUI
public class SetupSproutLandsResources
{
    const string SproutBase = "Assets/Sprout Lands - Sprites - Basic pack/";

    [MenuItem("Farm/Setup Sprout Lands Resources")]
    public static void Execute()
    {
        EnsureResourcesCopied();
        ConfigureResourceSprites();
        ConfigureUIPackSprites();
        WireScene();
        BuildSproutLandsTilemap.Execute();
        ApplySproutLandsUI.Execute();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[SetupSproutLands] Done.");
    }

    // ── Step 1: ensure PNGs are in Resources ─────────────────────────
    static void EnsureResourcesCopied()
    {
        var copies = new (string src, string dst)[]
        {
            (SproutBase + "Tilesets/Tilled_Dirt.png",                    "Assets/Resources/Tilled_Dirt.png"),
            (SproutBase + "Tilesets/Grass.png",                          "Assets/Resources/Grass.png"),
            (SproutBase + "Tilesets/Fences.png",                         "Assets/Resources/Fences.png"),
            (SproutBase + "Objects/Basic Plants.png",                    "Assets/Resources/Basic Plants.png"),
            (SproutBase + "Characters/Basic Charakter Spritesheet.png",  "Assets/Resources/Basic Charakter Spritesheet.png"),
        };

        foreach (var (src, dst) in copies)
        {
            if (!File.Exists(dst))
            {
                File.Copy(src, dst);
                Debug.Log($"Copied {Path.GetFileName(src)} to Resources");
            }
        }
        AssetDatabase.Refresh();
    }

    // ── Step 2: configure as Multiple/Point sprites ──────────────────
    static readonly (string path, int tw, int th)[] GridSheets =
    {
        ("Assets/Resources/Tilled_Dirt.png",                   16, 16),
        ("Assets/Resources/Grass.png",                         16, 16),
        ("Assets/Resources/Fences.png",                        16, 16),
        ("Assets/Resources/Basic Plants.png",                  16, 16),
        ("Assets/Resources/Basic Charakter Spritesheet.png",   48, 48),
    };

    static void ConfigureResourceSprites()
    {
        foreach (var (path, tw, th) in GridSheets)
            ConfigureGridSheet(path, tw, th);
    }

    static void ConfigureGridSheet(string path, int tw, int th)
    {
        var ti = AssetImporter.GetAtPath(path) as TextureImporter;
        if (ti == null) { Debug.LogWarning($"Not found: {path}"); return; }

        ti.textureType         = TextureImporterType.Sprite;
        ti.spriteImportMode    = SpriteImportMode.Multiple;
        ti.filterMode          = FilterMode.Point;
        ti.textureCompression  = TextureImporterCompression.Uncompressed;
        ti.alphaIsTransparency = true;
        ti.maxTextureSize      = 2048;
        ti.mipmapEnabled       = false;
        ti.SaveAndReimport();  // first pass to get real texture size

        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        if (tex == null) { Debug.LogWarning($"Texture null: {path}"); return; }

        int cols = tex.width  / tw;
        int rows = tex.height / th;
        string baseName = Path.GetFileNameWithoutExtension(path);

        var metas = new List<SpriteMetaData>();
        for (int r = 0; r < rows; r++)
        for (int c = 0; c < cols; c++)
        {
            metas.Add(new SpriteMetaData
            {
                name      = $"{baseName}_{r * cols + c}",
                rect      = new Rect(c * tw, tex.height - (r + 1) * th, tw, th),
                alignment = 0,
                pivot     = new Vector2(0.5f, 0.5f)
            });
        }

        ti.spritesheet = metas.ToArray();
        EditorUtility.SetDirty(ti);
        ti.SaveAndReimport();
        Debug.Log($"[SetupSproutLands] {Path.GetFileName(path)}: {cols}×{rows} = {metas.Count} sprites");
    }

    // ── Step 3: wire scene objects ────────────────────────────────────
    static void WireScene()
    {
        // FarmBackground
        if (GameObject.Find("FarmBackground") == null)
        {
            var go = new GameObject("FarmBackground");
            go.AddComponent<FarmBackground>();
            Debug.Log("[SetupSproutLands] Added FarmBackground.");
        }

        // FarmerCharacter — add and inject sprite references
        var farmerGO = GameObject.Find("Farmer");
        if (farmerGO == null)
        {
            farmerGO = new GameObject("Farmer");
            farmerGO.AddComponent<SpriteRenderer>();
        }

        var farmer = farmerGO.GetComponent<FarmerCharacter>()
                  ?? farmerGO.AddComponent<FarmerCharacter>();

        // Inject sprites from original Sprout Lands path (not Resources copy)
        // Character sheet meta: _0–_3 = down, _4–_7 = up, _8–_11 = left, _12–_15 = right
        string charPath = SproutBase + "Characters/Basic Charakter Spritesheet.png";
        var allChar = AssetDatabase.LoadAllAssetsAtPath(charPath);

        var sprDict = new Dictionary<string, Sprite>();
        foreach (var a in allChar)
            if (a is Sprite s) sprDict[s.name] = s;

        farmer.walkDownFrames  = GetFrames(sprDict, "Basic Charakter Spritesheet", 0, 4);
        farmer.walkUpFrames    = GetFrames(sprDict, "Basic Charakter Spritesheet", 4, 4);
        farmer.walkLeftFrames  = GetFrames(sprDict, "Basic Charakter Spritesheet", 8, 4);
        farmer.walkRightFrames = GetFrames(sprDict, "Basic Charakter Spritesheet", 12, 4);

        EditorUtility.SetDirty(farmer);
        Debug.Log($"[SetupSproutLands] Farmer wired: down={farmer.walkDownFrames?.Length} " +
                  $"up={farmer.walkUpFrames?.Length} left={farmer.walkLeftFrames?.Length} " +
                  $"right={farmer.walkRightFrames?.Length}");

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    }

    static Sprite[] GetFrames(Dictionary<string, Sprite> dict, string baseName, int start, int count)
    {
        var list = new List<Sprite>();
        for (int i = start; i < start + count; i++)
        {
            string key = $"{baseName}_{i}";
            if (dict.TryGetValue(key, out var s)) list.Add(s);
        }
        return list.ToArray();
    }

    // ── Configure Sprout Lands UI Pack sprites as Multiple/Point ─────
    static void ConfigureUIPackSprites()
    {
        const string UI = "Assets/Sprout Lands - UI Pack - Basic pack/Sprite sheets/";
        const string INV = "Assets/Sprout Lands - UI Pack - Basic pack/emojis-free/emoji style ui/";

        var uiSheets = new (string path, int tw, int th)[]
        {
            (UI + "Sprite sheet for Basic Pack.png",   16, 16),
            (UI + "buttons/Square Buttons 26x26.png",  32, 26),
            (UI + "buttons/Small Square Buttons.png",  16, 16),
            (UI + "buttons/Square Buttons 19x26.png",  19, 26),
            (UI + "UI Big Play Button.png",             96, 32),
            (UI + "Setting menu.png",                  128, 144),
            (UI + "Dialouge UI/dialog box big.png",    176, 48),
            (INV + "Inventory_Blocks_Spritesheet.png",  48, 48),
        };

        foreach (var (path, tw, th) in uiSheets)
        {
            var ti = AssetImporter.GetAtPath(path) as TextureImporter;
            if (ti == null) continue;

            ti.textureType         = TextureImporterType.Sprite;
            ti.spriteImportMode    = SpriteImportMode.Multiple;
            ti.filterMode          = FilterMode.Point;
            ti.textureCompression  = TextureImporterCompression.Uncompressed;
            ti.alphaIsTransparency = true;
            ti.mipmapEnabled       = false;
            ti.SaveAndReimport();

            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (tex == null) continue;

            int cols = tex.width  / tw;
            int rows = tex.height / th;
            if (cols == 0 || rows == 0) continue;

            string baseName = System.IO.Path.GetFileNameWithoutExtension(path);
            var metas = new System.Collections.Generic.List<SpriteMetaData>();
            for (int r = 0; r < rows; r++)
            for (int c = 0; c < cols; c++)
            {
                metas.Add(new SpriteMetaData
                {
                    name      = $"{baseName}_{r * cols + c}",
                    rect      = new Rect(c * tw, tex.height - (r+1)*th, tw, th),
                    alignment = 0,
                    pivot     = new Vector2(0.5f, 0.5f)
                });
            }

            ti.spritesheet = metas.ToArray();
            EditorUtility.SetDirty(ti);
            ti.SaveAndReimport();
            Debug.Log($"[SetupSproutLands UI] {System.IO.Path.GetFileName(path)}: {cols}×{rows}");
        }

        // dialog box big: single sprite (full image), configure as 9-slice
        ConfigureSingleSliced(UI + "Dialouge UI/dialog box big.png",    12);
        ConfigureSingleSliced(UI + "Setting menu.png",                  12);
        ConfigureSingleSliced(UI + "Dialouge UI/dialog box medium.png",  12);
    }

    static void ConfigureSingleSliced(string path, int border)
    {
        var ti = AssetImporter.GetAtPath(path) as TextureImporter;
        if (ti == null) return;
        ti.textureType         = TextureImporterType.Sprite;
        ti.spriteImportMode    = SpriteImportMode.Single;
        ti.spriteBorder        = new Vector4(border, border, border, border);
        ti.filterMode          = FilterMode.Point;
        ti.textureCompression  = TextureImporterCompression.Uncompressed;
        ti.alphaIsTransparency = true;
        ti.mipmapEnabled       = false;
        EditorUtility.SetDirty(ti);
        ti.SaveAndReimport();
    }
}
