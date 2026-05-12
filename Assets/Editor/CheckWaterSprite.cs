using UnityEngine;
using UnityEditor;
using System.Linq;

public class CheckWaterSprite
{
    [MenuItem("Tools/Check Water Sprite")]
    public static void Execute()
    {
        // Enable read/write first
        string path = "Assets/Farm Sprite.png";
        var ti = AssetImporter.GetAtPath(path) as TextureImporter;
        if (ti != null && !ti.isReadable)
        {
            ti.isReadable = true;
            ti.SaveAndReimport();
            Debug.Log("Enabled Read/Write on Farm Sprite.png");
        }

        var sprites = AssetDatabase.LoadAllAssetsAtPath(path)
            .OfType<Sprite>().ToArray();

        // Check sprite 177
        var s177 = sprites.FirstOrDefault(s => s.name == "Farm Sprite_177");
        if (s177 != null)
        {
            var tex = s177.texture;
            int cx = (int)s177.rect.x + 32;
            int cy = (int)s177.rect.y + 32;
            Color c = tex.GetPixel(cx, cy);
            Debug.Log($"Farm Sprite_177: rect={s177.rect} center pixel RGBA={c.r:F2},{c.g:F2},{c.b:F2},{c.a:F2}");
        }

        // Scan rows 10-13, cols 0-7 for blue tiles
        Debug.Log("Checking water region (rows 10-13, cols 0-7)...");
        for (int row = 10; row <= 13; row++)
        {
            for (int col = 0; col <= 7; col++)
            {
                int idx = row * 16 + col;
                var s = sprites.FirstOrDefault(sp => sp.name == $"Farm Sprite_{idx}");
                if (s == null) { Debug.Log($"  [{row},{col}] idx={idx}: NOT FOUND"); continue; }
                var tex = s.texture;
                int cx = (int)s.rect.x + 32;
                int cy = (int)s.rect.y + 32;
                Color c = tex.GetPixel(cx, cy);
                Debug.Log($"  [{row},{col}] idx={idx}: RGBA={c.r:F2},{c.g:F2},{c.b:F2},{c.a:F2}");
            }
        }
    }
}
