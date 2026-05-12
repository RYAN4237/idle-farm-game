using UnityEngine;
using UnityEditor;
using System.Linq;

/// Sample the dominant color of each Farm Sprite tile to identify grass/water/etc
public class SampleFarmTileColors
{
    const string PATH = "Assets/Farm Sprite.png";

    [MenuItem("Tools/Farm Sprite - Sample Colors Row 0-2")]
    public static void Execute()
    {
        // Make texture readable
        var ti = AssetImporter.GetAtPath(PATH) as TextureImporter;
        bool wasReadable = ti.isReadable;
        if (!wasReadable) { ti.isReadable = true; ti.SaveAndReimport(); }

        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(PATH);
        var sprites = AssetDatabase.LoadAllAssetsAtPath(PATH).OfType<Sprite>()
            .OrderByDescending(s => s.rect.y).ThenBy(s => s.rect.x).ToArray();

        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"[ColorSample] {sprites.Length} sprites. Sampling center pixel of each (rows 0-2 = idx 0-47):");

        for (int i = 0; i < Mathf.Min(48, sprites.Length); i++)
        {
            var s = sprites[i];
            int cx = (int)(s.rect.x + s.rect.width  * 0.5f);
            int cy = (int)(s.rect.y + s.rect.height * 0.5f);
            var c = tex.GetPixel(cx, cy);
            string colorName = ClassifyColor(c);
            sb.AppendLine($"  [{i:00}] rect=({s.rect.x},{s.rect.y}) center=({cx},{cy}) rgb=({(int)(c.r*255)},{(int)(c.g*255)},{(int)(c.b*255)}) a={c.a:F2} → {colorName}");
        }
        Debug.Log(sb.ToString());

        if (!wasReadable) { ti.isReadable = false; ti.SaveAndReimport(); }
    }

    static string ClassifyColor(Color c)
    {
        if (c.a < 0.1f) return "TRANSPARENT";
        float r = c.r, g = c.g, b = c.b;
        if (g > 0.35f && g > r * 1.2f && g > b * 1.3f) return "GREEN";
        if (g > 0.25f && r > 0.35f && b < 0.2f) return "OLIVE/DARK_GREEN";
        if (r > 0.7f && g > 0.6f && b > 0.3f) return "SAND/YELLOW";
        if (b > 0.45f && b > r && b > g) return "BLUE/WATER";
        if (r > 0.5f && g > 0.5f && b > 0.5f) return "LIGHT/SNOW";
        if (r > 0.4f && g > 0.3f && b > 0.2f) return "BROWN/DIRT";
        if (r < 0.35f && g < 0.35f && b < 0.35f) return "DARK";
        return $"OTHER(r={r:F2},g={g:F2},b={b:F2})";
    }
}
