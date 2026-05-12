using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;

public class SetupFarmSpritePng
{
    const string PATH = "Assets/Farm Sprite.png";
    const int PPU = 16;
    const int CELL = 16;

    [MenuItem("Tools/Farm Sprite - Check Size")]
    public static void CheckSize()
    {
        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(PATH);
        if (tex == null) { Debug.LogError("Cannot load " + PATH); return; }
        var ti = AssetImporter.GetAtPath(PATH) as TextureImporter;
        ti.GetSourceTextureWidthAndHeight(out int w, out int h);
        Debug.Log($"[FarmSpritePng] Texture={tex.width}x{tex.height}  Source={w}x{h}  type={ti.textureType}  mode={ti.spriteImportMode}  PPU={ti.spritePixelsPerUnit}");
        int cols = w / CELL;
        int rows = h / CELL;
        Debug.Log($"[FarmSpritePng] Grid: {cols} cols x {rows} rows = {cols * rows} cells");
        var sprites = AssetDatabase.LoadAllAssetsAtPath(PATH).OfType<Sprite>().ToArray();
        Debug.Log($"[FarmSpritePng] Current slice count: {sprites.Length}");
    }

    [MenuItem("Tools/Farm Sprite - Slice 16x16")]
    public static void SliceAndImport()
    {
        var ti = AssetImporter.GetAtPath(PATH) as TextureImporter;
        if (ti == null) { Debug.LogError("Not found: " + PATH); return; }

        ti.GetSourceTextureWidthAndHeight(out int w, out int h);
        int cols = w / CELL;
        int rows = h / CELL;
        Debug.Log($"[FarmSpritePng] Slicing {cols}x{rows} grid on {w}x{h} texture");

        ti.textureType = TextureImporterType.Sprite;
        ti.spriteImportMode = SpriteImportMode.Multiple;
        ti.spritePixelsPerUnit = PPU;
        ti.filterMode = FilterMode.Point;
        ti.textureCompression = TextureImporterCompression.Uncompressed;
        ti.alphaIsTransparency = true;
        ti.mipmapEnabled = false;

        var metas = new List<SpriteMetaData>();
        for (int r = 0; r < rows; r++)
        for (int c = 0; c < cols; c++)
        {
            float x = c * CELL;
            float y = h - (r + 1) * CELL;
            metas.Add(new SpriteMetaData {
                name      = $"FS_{r:00}_{c:00}",
                rect      = new Rect(x, y, CELL, CELL),
                pivot     = new Vector2(0.5f, 0.5f),
                alignment = (int)SpriteAlignment.Center,
            });
        }
        ti.spritesheet = metas.ToArray();
        ti.SaveAndReimport();
        AssetDatabase.Refresh();
        Debug.Log($"[FarmSpritePng] Done: {metas.Count} sprites as FS_row_col");
    }

    [MenuItem("Tools/Farm Sprite - Dump Sprites")]
    public static void DumpSprites()
    {
        var sprites = AssetDatabase.LoadAllAssetsAtPath(PATH).OfType<Sprite>()
            .OrderByDescending(s => s.rect.y).ThenBy(s => s.rect.x).ToArray();
        if (sprites.Length == 0) { Debug.LogError("[FarmSpritePng] No sprites - run Slice first"); return; }
        Debug.Log($"[FarmSpritePng] {sprites.Length} sprites (top-to-bottom, left-to-right):");
        for (int i = 0; i < sprites.Length; i++)
        {
            var s = sprites[i];
            Debug.Log($"  [{i:000}] {s.name}  rect=({s.rect.x},{s.rect.y})");
        }
    }
}
