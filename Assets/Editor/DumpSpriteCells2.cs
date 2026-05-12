using UnityEngine;
using UnityEditor;
using System.Linq;

public class DumpSpriteCells2
{
    const string SHEET = "Assets/Farm Sprite.png";

    [MenuItem("Tools/Dump Sprite Cells 2")]
    public static void Execute()
    {
        var ti = AssetImporter.GetAtPath(SHEET) as TextureImporter;
        bool wasReadable = ti != null && ti.isReadable;
        if (!wasReadable && ti != null) { ti.isReadable = true; ti.SaveAndReimport(); }

        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(SHEET);

        Debug.Log("=== ROWS 8-15 ===");
        for (int row = 8; row < 16; row++)
        {
            string line = $"Row{row:D2}: ";
            for (int col = 0; col < 16; col++)
            {
                int px = col * 64 + 32;
                int py = (15 - row) * 64 + 32;
                var c = tex.GetPixel(px, py);
                if (c.a < 0.1f) line += "  __  ";
                else
                {
                    string t;
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
