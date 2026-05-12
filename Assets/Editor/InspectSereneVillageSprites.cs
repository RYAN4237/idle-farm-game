using UnityEngine;
using UnityEditor;
using System.Linq;

public class InspectSereneVillageSprites
{
    [MenuItem("Tools/Inspect Serene Village Sprites Detail")]
    public static void Execute()
    {
        string path = "Assets/SERENE_VILLAGE_REVAMPED/Serene_Village_16x16.png";

        var ti = AssetImporter.GetAtPath(path) as TextureImporter;
        bool wasReadable = ti.isReadable;
        if (!wasReadable) { ti.isReadable = true; ti.SaveAndReimport(); }

        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        var sprites = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>()
                        .OrderBy(s => s.textureRect.y).ThenBy(s => s.textureRect.x)
                        .ToArray();

        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"=== Serene_Village_16x16.png === {tex.width}x{tex.height}px, {sprites.Length} sprites");

        // Only show sprites in top portion (y > tex.height - 128) = water/terrain area
        foreach (var s in sprites)
        {
            var r = s.textureRect;
            // Sample center pixel for color hint
            var c = tex.GetPixel((int)r.x + 8, (int)r.y + 8);
            sb.AppendLine($"  {s.name,-40} rect=({(int)r.x,3},{(int)r.y,3},{(int)r.width},{(int)r.height})  center=({c.r:F2},{c.g:F2},{c.b:F2},{c.a:F2})");
        }

        Debug.Log(sb.ToString());

        if (!wasReadable) { ti.isReadable = false; ti.SaveAndReimport(); }
    }
}
