using UnityEngine;
using UnityEditor;

/// Configure FarmBG_Ground.png and FarmBG_Deco.png as sprite sheets (32x32 grid)
public class ImportFarmBGSprites
{
    [MenuItem("Tools/Import FarmBG Sprites")]
    public static void Execute()
    {
        ConfigureSheet("Assets/Sprites/FarmBG_Ground.png", 32);
        ConfigureSheet("Assets/Sprites/FarmBG_Deco.png", 32);
        AssetDatabase.Refresh();
        Debug.Log("[Import] FarmBG sprite sheets configured.");
    }

    static void ConfigureSheet(string path, int tileSize)
    {
        var ti = AssetImporter.GetAtPath(path) as TextureImporter;
        if (ti == null) { Debug.LogError($"Not found: {path}"); return; }

        ti.textureType         = TextureImporterType.Sprite;
        ti.spriteImportMode    = SpriteImportMode.Multiple;
        ti.spritePixelsPerUnit = tileSize;
        ti.filterMode          = FilterMode.Point;
        ti.textureCompression  = TextureImporterCompression.Uncompressed;
        ti.alphaIsTransparency = true;
        ti.isReadable          = true;
        ti.maxTextureSize      = 2048;

        // Auto-slice into tileSize×tileSize grid
        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        // Need to reimport first to get texture dimensions
        ti.SaveAndReimport();

        tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        if (tex == null) { Debug.LogError($"Texture null after reimport: {path}"); return; }

        int cols = tex.width  / tileSize;
        int rows = tex.height / tileSize;

        var metas = new System.Collections.Generic.List<SpriteMetaData>();
        for (int row = 0; row < rows; row++)
        for (int col = 0; col < cols; col++)
        {
            var sm = new SpriteMetaData();
            sm.name   = $"{System.IO.Path.GetFileNameWithoutExtension(path)}_{row * cols + col}";
            sm.rect   = new Rect(col * tileSize, tex.height - (row + 1) * tileSize, tileSize, tileSize);
            sm.pivot  = new Vector2(0.5f, 0.5f);
            sm.alignment = (int)SpriteAlignment.Center;
            metas.Add(sm);
        }

        ti.spritesheet = metas.ToArray();
        ti.SaveAndReimport();
        Debug.Log($"[Import] {path}: {cols}×{rows} = {metas.Count} sprites");
    }
}
