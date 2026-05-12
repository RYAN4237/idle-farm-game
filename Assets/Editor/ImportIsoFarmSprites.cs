using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;

/// Imports IsoFarm_Ground.png (8x6, 48px tiles) and IsoFarm_Deco.png (8x12, 48px tiles)
public class ImportIsoFarmSprites
{
    const int T = 48;

    [MenuItem("Tools/Import Iso Farm Sprites")]
    public static void Execute()
    {
        ConfigureSheet("Assets/Sprites/IsoFarm_Ground.png", T, 8, 6,  "IsoGnd");
        ConfigureSheet("Assets/Sprites/IsoFarm_Deco.png",   T, 8, 12, "IsoDco");
        AssetDatabase.Refresh();
        Debug.Log("[ImportIso] Done.");
    }

    static void ConfigureSheet(string path, int tileSize, int cols, int rows, string prefix)
    {
        var ti = AssetImporter.GetAtPath(path) as TextureImporter;
        if (ti == null) { Debug.LogError($"No importer: {path}"); return; }

        ti.textureType         = TextureImporterType.Sprite;
        ti.spriteImportMode    = SpriteImportMode.Multiple;
        ti.spritePixelsPerUnit = tileSize;
        ti.filterMode          = FilterMode.Point;
        ti.alphaIsTransparency = true;
        ti.mipmapEnabled       = false;
        ti.wrapMode            = TextureWrapMode.Clamp;

        var plat = ti.GetDefaultPlatformTextureSettings();
        plat.maxTextureSize = 4096;
        plat.format = TextureImporterFormat.RGBA32;
        ti.SetPlatformTextureSettings(plat);

        var meta = new List<SpriteMetaData>();
        for (int r = 0; r < rows; r++)
        for (int c = 0; c < cols; c++)
        {
            int idx = r * cols + c;
            float x = c * tileSize;
            float y = (rows - 1 - r) * tileSize;
            meta.Add(new SpriteMetaData {
                name      = $"{prefix}_{idx}",
                rect      = new Rect(x, y, tileSize, tileSize),
                pivot     = new Vector2(0.5f, 0.5f),
                alignment = (int)SpriteAlignment.Center,
            });
        }
        ti.spritesheet = meta.ToArray();
        EditorUtility.SetDirty(ti);
        ti.SaveAndReimport();
        Debug.Log($"[ImportIso] {path}: {meta.Count} sprites ({cols}x{rows})");
    }
}
