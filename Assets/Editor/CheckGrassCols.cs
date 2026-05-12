using UnityEngine;
using UnityEditor;
using System.Linq;
using System.IO;

/// Check which exact grass cols have sparkle/glitter vs plain green
public class CheckGrassCols
{
    [MenuItem("Tools/Check Grass Cols")]
    public static void Execute()
    {
        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Sprites/FarmBG_Ground.png");
        if (tex == null) { Debug.LogError("No texture"); return; }
        // Sample multiple pixels per tile in row0 to detect sparkle
        for (int c = 0; c < 8; c++)
        {
            int x = c * 32;
            // Sample 9 points in the tile
            var colors = new System.Collections.Generic.List<string>();
            for (int dx = 4; dx <= 28; dx += 8)
            for (int dy = 4; dy <= 28; dy += 8)
            {
                // row0 visual = bottom-most tile in texture (rows inverted: row0 = visual top = texture bottom)
                // Actually: reimport set row0 = texture top after flip. Let's check both tex row positions.
                // Our reimport: y = (rows-1-r)*tileSize+center = (5)*32+16 = 176 for row0
                var col = tex.GetPixel(x+dx, 5*32+dy);
                colors.Add($"({(int)(col.r*255)},{(int)(col.g*255)},{(int)(col.b*255)},a={col.a:F1})");
            }
            Debug.Log($"Row0 col{c}: {string.Join(" | ", colors)}");
        }
    }
}
