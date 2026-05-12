using UnityEngine;
using UnityEditor;
using System.Linq;

public class SampleFarmWaterTiles
{
    [MenuItem("Tools/Farm Sprite - Sample Water Row Colors")]
    public static void Execute()
    {
        string PATH = "Assets/Farm Sprite.png";
        var ti = AssetImporter.GetAtPath(PATH) as TextureImporter;
        bool wasReadable = ti.isReadable;
        if (!wasReadable) { ti.isReadable = true; ti.SaveAndReimport(); }

        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(PATH);
        var sprites = AssetDatabase.LoadAllAssetsAtPath(PATH).OfType<Sprite>()
            .OrderByDescending(s => s.rect.y).ThenBy(s => s.rect.x).ToArray();

        // Sample rows 7-12 (idx 112-191) to find blue water tiles
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("[WaterSample] Rows 7-12:");
        for (int i = 112; i < Mathf.Min(192, sprites.Length); i++)
        {
            var s = sprites[i];
            int cx = (int)(s.rect.x + s.rect.width  * 0.5f);
            int cy = (int)(s.rect.y + s.rect.height * 0.5f);
            var c = tex.GetPixel(cx, cy);
            bool isBlue  = c.b > 0.5f && c.b > c.r && c.b > c.g;
            bool isGreen = c.g > 0.4f && c.g > c.r * 1.15f;
            bool isEmpty = c.a < 0.1f;
            string tag = isEmpty ? "EMPTY" : isBlue ? "BLUE" : isGreen ? "GREEN" : "OTHER";
            if (isBlue || (i >= 128 && i <= 143) || (i >= 160 && i <= 179))
                sb.AppendLine($"  [{i:000}] r{i/16}c{i%16} ({(int)(c.r*255)},{(int)(c.g*255)},{(int)(c.b*255)}) a={c.a:F1} [{tag}]");
        }
        Debug.Log(sb.ToString());

        if (!wasReadable) { ti.isReadable = false; ti.SaveAndReimport(); }
    }
}
