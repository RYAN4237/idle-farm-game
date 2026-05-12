using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

/// Phase 1: Configure all Sprout Lands sprite assets with correct import settings.
/// PPU=16, Point filter, No compression, Multiple slice at 16x16.
public class SetupFarmSpriteImport
{
    const int PPU  = 16;
    const int CELL = 16;

    static readonly string[] SpritePaths = new[]
    {
        "Assets/Sprout Lands - Sprites - Basic pack/Tilesets/Grass.png",
        "Assets/Sprout Lands - Sprites - Basic pack/Tilesets/Water.png",
        "Assets/Sprout Lands - Sprites - Basic pack/Tilesets/Tilled Dirt.png",
        "Assets/Sprout Lands - Sprites - Basic pack/Tilesets/Tilled_Dirt.png",
        "Assets/Sprout Lands - Sprites - Basic pack/Tilesets/Fences.png",
        "Assets/Sprout Lands - Sprites - Basic pack/Tilesets/Hills.png",
        "Assets/Sprout Lands - Sprites - Basic pack/Objects/Wood Bridge.png",
        "Assets/Sprout Lands - Sprites - Basic pack/Objects/Wood_Bridge.png",
        "Assets/Sprout Lands - Sprites - Basic pack/Objects/Basic Grass Biom things 1.png",
        "Assets/Sprout Lands - Sprites - Basic pack/Objects/Basic_Grass_Biom_things.png",
        "Assets/Sprout Lands - Sprites - Basic pack/Objects/Basic Plants.png",
        "Assets/Sprout Lands - Sprites - Basic pack/Objects/Basic_Plants.png",
        "Assets/Sprout Lands - Sprites - Basic pack/Objects/Paths.png",
        "Assets/Sprout Lands - Sprites - Basic pack/Characters/Basic Charakter Spritesheet.png",
    };

    [MenuItem("Tools/Setup Farm Sprite Import")]
    public static void Execute()
    {
        int configured = 0;
        foreach (var path in SpritePaths)
        {
            var ti = AssetImporter.GetAtPath(path) as TextureImporter;
            if (ti == null) continue;

            bool changed = false;

            if (ti.textureType != TextureImporterType.Sprite)
            { ti.textureType = TextureImporterType.Sprite; changed = true; }

            if (ti.spriteImportMode != SpriteImportMode.Multiple)
            { ti.spriteImportMode = SpriteImportMode.Multiple; changed = true; }

            if ((int)ti.spritePixelsPerUnit != PPU)
            { ti.spritePixelsPerUnit = PPU; changed = true; }

            if (ti.filterMode != FilterMode.Point)
            { ti.filterMode = FilterMode.Point; changed = true; }

            if (ti.textureCompression != TextureImporterCompression.Uncompressed)
            { ti.textureCompression = TextureImporterCompression.Uncompressed; changed = true; }

            if (!ti.alphaIsTransparency)
            { ti.alphaIsTransparency = true; changed = true; }

            if (ti.mipmapEnabled)
            { ti.mipmapEnabled = false; changed = true; }

            // Auto-slice at 16x16 if no slices yet
            if (ti.spritesheet == null || ti.spritesheet.Length == 0)
            {
                SliceAt16(ti, path);
                changed = true;
            }

            if (changed)
            {
                ti.SaveAndReimport();
                configured++;
                Debug.Log($"[FarmImport] Configured: {path}");
            }
        }

        AssetDatabase.Refresh();
        Debug.Log($"[FarmImport] Done. Configured {configured} sprite sheets.");
    }

    static void SliceAt16(TextureImporter ti, string path)
    {
        // Read texture size via TextureImporter settings
        ti.GetSourceTextureWidthAndHeight(out int w, out int h);
        if (w == 0 || h == 0) return;

        var metas = new List<SpriteMetaData>();
        int idx = 0;
        for (int row = 0; row < h / CELL; row++)
        for (int col = 0; col < w / CELL; col++)
        {
            float x = col * CELL;
            float y = h - (row + 1) * CELL;
            metas.Add(new SpriteMetaData
            {
                name      = $"{System.IO.Path.GetFileNameWithoutExtension(path)}_{idx:000}",
                rect      = new Rect(x, y, CELL, CELL),
                pivot     = new Vector2(0.5f, 0.5f),
                alignment = (int)SpriteAlignment.Center,
            });
            idx++;
        }
        ti.spritesheet = metas.ToArray();
    }
}
