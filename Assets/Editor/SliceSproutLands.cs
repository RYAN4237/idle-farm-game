using UnityEngine;
using UnityEditor;

/// Slices Sprout Lands sprite sheets into individual sprites
public class SliceSproutLands
{
    static readonly string BASE = "Assets/Sprout Lands - Sprites - Basic pack/Objects/";

    public static void Execute()
    {
        // Each sheet: path, tile size in pixels
        var sheets = new (string path, int tileW, int tileH)[]
        {
            (BASE + "Basic Plants.png",           16, 16),
            (BASE + "Basic tools and meterials.png", 16, 16),
            (BASE + "Basic Grass Biom things 1.png",16, 16),
            (BASE + "Basic Furniture.png",         16, 16),
        };

        foreach (var (path, tw, th) in sheets)
        {
            var ti = AssetImporter.GetAtPath(path) as TextureImporter;
            if (ti == null) { Debug.LogWarning($"Not found: {path}"); continue; }

            ti.textureType        = TextureImporterType.Sprite;
            ti.spriteImportMode   = SpriteImportMode.Multiple;
            ti.filterMode         = FilterMode.Point;   // pixel-art
            ti.textureCompression = TextureImporterCompression.Uncompressed;
            ti.maxTextureSize     = 2048;

            // Auto-slice by cell size
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (tex == null) { Debug.LogWarning($"Texture null: {path}"); continue; }

            int cols = tex.width  / tw;
            int rows = tex.height / th;
            var metas = new System.Collections.Generic.List<SpriteMetaData>();

            string baseName = System.IO.Path.GetFileNameWithoutExtension(path);
            for (int r = 0; r < rows; r++)
            for (int c = 0; c < cols; c++)
            {
                // Skip fully-transparent tiles
                var smd = new SpriteMetaData
                {
                    name      = $"{baseName}_{r}_{c}",
                    rect      = new Rect(c * tw, tex.height - (r+1)*th, tw, th),
                    alignment = 0,
                    pivot     = new Vector2(0.5f, 0.5f)
                };
                metas.Add(smd);
            }

            ti.spritesheet = metas.ToArray();
            EditorUtility.SetDirty(ti);
            ti.SaveAndReimport();
            Debug.Log($"Sliced {path}: {cols}x{rows} = {metas.Count} sprites");
        }

        AssetDatabase.Refresh();
        Debug.Log("[SliceSproutLands] All sheets sliced!");
    }
}
