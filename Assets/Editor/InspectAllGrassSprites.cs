using UnityEngine;
using UnityEditor;
using System.Linq;

public class InspectAllGrassSprites
{
    [MenuItem("Tools/Inspect All Grass Sprites 66-76")]
    public static void Execute()
    {
        string grassPath = "Assets/Sprout Lands - Sprites - Basic pack/Tilesets/Grass.png";

        var ti = AssetImporter.GetAtPath(grassPath) as TextureImporter;
        bool wasReadable = ti.isReadable;
        if (!wasReadable) { ti.isReadable = true; ti.SaveAndReimport(); }

        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(grassPath);
        if (tex == null) { Debug.LogError("Cannot load texture"); return; }

        // Load all sprites to get their actual rect positions
        var sprites = AssetDatabase.LoadAllAssetsAtPath(grassPath).OfType<Sprite>().ToArray();

        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"Texture size: {tex.width}x{tex.height}");

        for (int i = 66; i <= 76; i++)
        {
            var spr = sprites.FirstOrDefault(s => s.name == $"Grass_{i}");
            if (spr == null) { sb.AppendLine($"Grass_{i}: NOT FOUND"); continue; }

            var rect = spr.textureRect;
            int px = (int)rect.x;
            int py = (int)rect.y;

            // Sample center, all 4 corners
            var center = tex.GetPixel(px + 8, py + 8);
            var tl     = tex.GetPixel(px,     py + 15);
            var tr     = tex.GetPixel(px + 15, py + 15);
            var bl     = tex.GetPixel(px,     py);
            var br     = tex.GetPixel(px + 15, py);

            // Check if any pixel is dark (luminance < 0.3) or transparent (alpha < 1)
            float minAlpha = Mathf.Min(center.a, tl.a, tr.a, bl.a, br.a);
            float minLum = Mathf.Min(
                Lum(center), Lum(tl), Lum(tr), Lum(bl), Lum(br));

            sb.AppendLine($"Grass_{i} rect=({px},{py},16,16)  center={F(center)}  minAlpha={minAlpha:F2}  minLum={minLum:F2}");
        }

        Debug.Log(sb.ToString());

        if (!wasReadable) { ti.isReadable = false; ti.SaveAndReimport(); }
    }

    static float Lum(Color c) => 0.299f*c.r + 0.587f*c.g + 0.114f*c.b;
    static string F(Color c) => $"({c.r:F2},{c.g:F2},{c.b:F2},{c.a:F2})";
}
