using UnityEngine;
using UnityEditor;
using System.Linq;

public class InspectGrassSprite
{
    [MenuItem("Tools/Inspect Grass Sprite Pixels")]
    public static void Execute()
    {
        string grassPath = "Assets/Sprout Lands - Sprites - Basic pack/Tilesets/Grass.png";

        // Make texture readable temporarily
        var ti = AssetImporter.GetAtPath(grassPath) as TextureImporter;
        bool wasReadable = ti.isReadable;
        if (!wasReadable)
        {
            ti.isReadable = true;
            ti.SaveAndReimport();
        }

        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(grassPath);
        if (tex == null) { Debug.LogError("Cannot load Grass.png texture"); return; }

        // Check Grass_66 (x=0, y=0, 16x16) — sample center and corners
        // Unity pixel coords: (0,0) = bottom-left
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"Texture size: {tex.width}x{tex.height}");

        // Grass_66: rect=(0,0,16,16) — sample a few pixels
        sb.AppendLine("Grass_66 (x=0,y=0) pixel samples:");
        sb.AppendLine($"  center (8,8): {tex.GetPixel(8, 8)}");
        sb.AppendLine($"  (4,4): {tex.GetPixel(4, 4)}");
        sb.AppendLine($"  (0,0): {tex.GetPixel(0, 0)}");
        sb.AppendLine($"  (15,15): {tex.GetPixel(15, 15)}");
        sb.AppendLine($"  (0,15): {tex.GetPixel(0, 15)}");
        sb.AppendLine($"  (15,0): {tex.GetPixel(15, 0)}");

        // Grass_0 (x=0, y=96): top row isolated tiles
        sb.AppendLine("Grass_0 (x=0,y=96) pixel samples:");
        sb.AppendLine($"  center (8,104): {tex.GetPixel(8, 104)}");
        sb.AppendLine($"  (0,96): {tex.GetPixel(0, 96)}");
        sb.AppendLine($"  (15,111): {tex.GetPixel(15, 111)}");

        Debug.Log(sb.ToString());

        // Restore
        if (!wasReadable)
        {
            ti.isReadable = false;
            ti.SaveAndReimport();
        }
    }
}
