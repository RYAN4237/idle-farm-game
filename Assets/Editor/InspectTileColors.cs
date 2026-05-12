using UnityEngine;
using UnityEditor;
using System.Linq;
using System.IO;
using System.Collections.Generic;

/// Inspect actual pixel colors of Ground and Deco sprite tiles to identify correct row/col mappings
public class InspectTileColors
{
    [MenuItem("Tools/Inspect Tile Colors")]
    public static void Execute()
    {
        InspectSheet("Assets/Sprites/FarmBG_Ground.png", "FarmBG_Ground", 8, 6);
        InspectSheet("Assets/Sprites/FarmBG_Deco.png",   "FarmBG_Deco",   8, 10);
    }

    static void InspectSheet(string path, string prefix, int cols, int rows)
    {
        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        if (tex == null) { Debug.LogError($"Cannot load {path}"); return; }
        
        // Sample center pixel of each tile
        int tileSize = tex.width / cols;
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"=== {prefix} ({tex.width}x{tex.height}, tile={tileSize}) ===");
        
        for (int r = 0; r < rows; r++)
        {
            sb.Append($"Row{r}: ");
            for (int c = 0; c < cols; c++)
            {
                // Rect in texture coords (bottom-left origin): row0 of sprite = visual top = highest y in texture
                int texRow = rows - 1 - r; // flip
                int px = c * tileSize + tileSize/2;
                int py = texRow * tileSize + tileSize/2;
                var col = tex.GetPixel(px, py);
                // Classify color
                string name = ClassifyColor(col, (col.a < 0.1f));
                sb.Append($"[{c}:{name}] ");
            }
            sb.AppendLine();
        }
        Debug.Log(sb.ToString());
    }

    static string ClassifyColor(Color c, bool transparent)
    {
        if (transparent || c.a < 0.15f) return "EMPTY";
        float r = c.r, g = c.g, b = c.b;
        if (g > 0.4f && g > r * 1.3f && g > b * 1.2f) return "GRASS";
        if (b > 0.4f && b > r * 1.2f && b > g * 0.9f && g > 0.3f) return "WATER";
        if (r > 0.4f && g > 0.3f && b < 0.25f && r > b * 2f) return "WOOD";
        if (r > 0.45f && g > 0.35f && b > 0.3f && Mathf.Abs(r-g)<0.15f) return "ROCK";
        if (r > 0.55f && g > 0.45f && b < 0.3f) return "SAND";
        if (r > 0.5f && g < 0.35f && b < 0.2f) return "RED";
        if (r > 0.7f && g > 0.6f && b < 0.2f) return "ORAN";
        return $"rgb({(int)(r*255)},{(int)(g*255)},{(int)(b*255)})";
    }
}
