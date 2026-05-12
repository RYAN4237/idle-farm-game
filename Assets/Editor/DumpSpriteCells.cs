using UnityEngine;
using UnityEditor;
using System.Linq;
using System.IO;

/// Dumps specific sprite cells as individual PNGs to inspect them
public class DumpSpriteCells
{
    const string SHEET = "Assets/Farm Sprite.png";

    [MenuItem("Tools/Dump Sprite Cells")]
    public static void Execute()
    {
        var ti = AssetImporter.GetAtPath(SHEET) as TextureImporter;
        bool wasReadable = ti != null && ti.isReadable;
        if (!wasReadable && ti != null) { ti.isReadable = true; ti.SaveAndReimport(); }

        var sprites = AssetDatabase.LoadAllAssetsAtPath(SHEET).OfType<Sprite>()
            .ToDictionary(s => {
                var p = s.name.Split('_'); int.TryParse(p[p.Length-1], out int i); return i;
            });
        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(SHEET);

        // Dump a summary: for each row, print average color of first pixel of each cell
        // This helps us find green-dark (tree canopy) vs brown (trunk) vs etc
        Debug.Log("=== ROW SUMMARY (avg center pixel per cell) ===");
        for (int row = 0; row < 8; row++)
        {
            string line = $"Row{row:D2}: ";
            for (int col = 0; col < 16; col++)
            {
                int idx = row * 16 + col;
                // Cell position: sprite sheet rows go from top visually but pixels from bottom
                // Farm Sprite idx 0 = row 0 col 0 visual top = pixel y = (15-0)*64+32 = 992 (near top texture pixels = near high y)
                int px = col * 64 + 32;
                int py = (15 - row) * 64 + 32; // flip: visual row 0 = pixel row 15
                var c = tex.GetPixel(px, py);
                if (c.a < 0.1f) line += "  __  ";
                else
                {
                    string t = "?";
                    if (c.b > c.r + 0.05f && c.b > c.g + 0.05f) t = "BLU";
                    else if (c.r > 0.55f && c.r > c.g * 1.3f) t = "RED";
                    else if (c.r > 0.35f && c.g > 0.2f && c.b < 0.18f && c.r > c.b * 2f) t = "BRN";
                    else if (c.r > 0.65f && c.g > 0.5f && c.b < 0.3f) t = "YEL";
                    else if (c.r > 0.7f && c.g > 0.7f && c.b > 0.6f) t = "WHT";
                    else if (c.g > c.r * 1.05f && c.g > c.b * 1.1f) t = "GRN";
                    else t = $"m{c.r:F1}{c.g:F1}{c.b:F1}";
                    line += $"{t,5} ";
                }
            }
            Debug.Log(line);
        }
        Debug.Log("=== END ===");

        if (!wasReadable && ti != null) { ti.isReadable = false; ti.SaveAndReimport(); }
    }
}
