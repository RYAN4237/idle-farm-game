using UnityEngine;
using UnityEditor;
using System.Linq;

// Samples exact pixel coords from Terrains_TILESET_B-C-D-E.png
// RPG Maker MV tile size = 48px
// We want to identify the water autotile block and its neighbor tiles
public class InspectRPGMakerTerrain
{
    [MenuItem("Tools/Inspect RPGMaker Terrain Pixels")]
    public static void Execute()
    {
        string path = "Assets/SERENE_VILLAGE_REVAMPED/RPG_MAKER_MV/Terrains_TILESET_B-C-D-E.png";

        var ti = AssetImporter.GetAtPath(path) as TextureImporter;
        bool wasReadable = ti.isReadable;
        if (!wasReadable) { ti.isReadable = true; ti.SaveAndReimport(); }

        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        int w = tex.width, h = tex.height;
        Debug.Log($"Terrain sheet: {w}x{h}  tileSize=48 → {w/48}x{h/48} grid");

        // Sample center pixel of each 48x48 cell to identify tile regions
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("Grid sampling (center pixel of each 48x48 cell):");
        for (int row = 0; row < h/48; row++)
        for (int col = 0; col < w/48; col++)
        {
            int px = col * 48 + 24;
            int py = h - row * 48 - 24; // Unity flips Y
            var c = tex.GetPixel(px, py);
            // Classify: blue=water, green=grass, sand=tan, empty=transparent
            string type = c.a < 0.1f ? "EMPTY" :
                          c.b > 0.5f && c.b > c.r ? "WATER" :
                          c.g > 0.4f && c.g > c.r ? "GRASS" :
                          c.r > 0.5f && c.g > 0.3f ? "SAND " : "OTHER";
            sb.AppendLine($"  [{row,2},{col,2}] px=({px},{py}) {type} rgb=({c.r:F2},{c.g:F2},{c.b:F2},{c.a:F2})");
        }
        Debug.Log(sb.ToString());

        if (!wasReadable) { ti.isReadable = false; ti.SaveAndReimport(); }
    }
}
