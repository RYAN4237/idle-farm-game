using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;

/// Re-imports FarmBG_Ground.png and FarmBG_Deco.png with correct sprite slice settings.
public class ReimportFarmBGSprites
{
    [MenuItem("Tools/Reimport FarmBG Sprites")]
    public static void Execute()
    {
        ConfigureSheet("Assets/Sprites/FarmBG_Ground.png", 32, 8, 6,  "FarmBG_Ground");
        ConfigureSheet("Assets/Sprites/FarmBG_Deco.png",   32, 8, 10, "FarmBG_Deco");
        AssetDatabase.Refresh();
        Debug.Log("[Reimport] Done. Refresh complete.");
    }

    static void ConfigureSheet(string path, int tileSize, int cols, int rows, string prefix)
    {
        var ti = AssetImporter.GetAtPath(path) as TextureImporter;
        if (ti == null) { Debug.LogError($"[Reimport] No importer at {path}"); return; }

        ti.textureType         = TextureImporterType.Sprite;
        ti.spriteImportMode    = SpriteImportMode.Multiple;
        ti.spritePixelsPerUnit = tileSize;
        ti.filterMode          = FilterMode.Point;
        ti.alphaIsTransparency = true;
        ti.mipmapEnabled       = false;
        ti.wrapMode            = TextureWrapMode.Clamp;

        // Force max texture size large enough
        var plat = ti.GetDefaultPlatformTextureSettings();
        plat.maxTextureSize = 2048;
        plat.format = TextureImporterFormat.RGBA32;
        ti.SetPlatformTextureSettings(plat);

        // Build spritesheet metadata manually
        int totalW = cols * tileSize;
        int totalH = rows * tileSize;
        var meta = new List<SpriteMetaData>();
        for (int r = 0; r < rows; r++)
        for (int c = 0; c < cols; c++)
        {
            int idx = r * cols + c;
            // Unity rect: origin bottom-left, so flip row
            float x = c * tileSize;
            float y = (rows - 1 - r) * tileSize; // flip vertically
            meta.Add(new SpriteMetaData
            {
                name   = $"{prefix}_{idx}",
                rect   = new Rect(x, y, tileSize, tileSize),
                pivot  = new Vector2(0.5f, 0.5f),
                alignment = (int)SpriteAlignment.Center,
            });
        }
        ti.spritesheet = meta.ToArray();

        EditorUtility.SetDirty(ti);
        ti.SaveAndReimport();
        Debug.Log($"[Reimport] {path}: configured {meta.Count} sprites ({cols}x{rows})");
    }
}
