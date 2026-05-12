using UnityEngine;
using UnityEditor;
using System.Linq;

public class FindGrassTiles
{
    [MenuItem("Tools/Farm Sprite - Find All Green Tiles")]
    public static void Execute()
    {
        string PATH = "Assets/Farm Sprite.png";
        var ti = AssetImporter.GetAtPath(PATH) as TextureImporter;
        bool wasReadable = ti.isReadable;
        if (!wasReadable) { ti.isReadable = true; ti.SaveAndReimport(); }

        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(PATH);
        var sprites = AssetDatabase.LoadAllAssetsAtPath(PATH).OfType<Sprite>()
            .OrderByDescending(s => s.rect.y).ThenBy(s => s.rect.x).ToArray();

        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"[GrassFinder] {sprites.Length} sprites. Listing all non-empty, non-white tiles with dominant color:");

        for (int i = 0; i < sprites.Length; i++)
        {
            var s = sprites[i];
            // Sample 4 points across the tile center
            float cx = s.rect.x + s.rect.width  * 0.5f;
            float cy = s.rect.y + s.rect.height * 0.5f;

            var c = tex.GetPixel((int)cx, (int)cy);
            if (c.a < 0.1f) continue; // skip transparent
            if (c.r > 0.95f && c.g > 0.95f) continue; // skip white/empty

            float r = c.r * 255f, g = c.g * 255f, b = c.b * 255f;
            // Categorize by dominant channel and ratios
            string cat;
            if (g > 100 && g > r * 1.05f && g > b * 1.5f) cat = "GREEN";
            else if (b > 100 && b > r && b > g * 0.95f) cat = "BLUE";
            else if (r > 180 && g > 150 && b < 120) cat = "SAND";
            else if (r < 120 && g < 120 && b < 120) cat = "DARK";
            else cat = $"MIXED(r={r:F0},g={g:F0},b={b:F0})";

            if (cat == "GREEN" || cat == "BLUE")
                sb.AppendLine($"  [{i:000}] r{i/16}c{i%16} rect=({s.rect.x},{s.rect.y}) rgb=({r:F0},{g:F0},{b:F0}) → {cat}");
        }
        Debug.Log(sb.ToString());

        if (!wasReadable) { ti.isReadable = false; ti.SaveAndReimport(); }
    }
}
