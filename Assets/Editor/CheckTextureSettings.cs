using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using System.Linq;

/// Check actual texture importer settings for grass and water
public class CheckTextureSettings
{
    [MenuItem("Tools/Check Texture Settings")]
    public static void Execute()
    {
        Check("Assets/Sprout Lands - Sprites - Basic pack/Tilesets/Grass.png");
        Check("Assets/Sprout Lands - Sprites - Basic pack/Tilesets/Water.png");
    }

    static void Check(string path)
    {
        var ti = AssetImporter.GetAtPath(path) as TextureImporter;
        if (ti == null) { Debug.LogError($"Cannot load {path}"); return; }

        var sprites = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToArray();
        float ppu = sprites.Length > 0 ? sprites[0].pixelsPerUnit : -1;

        Debug.Log($"{System.IO.Path.GetFileName(path)}: ImporterPPU={ti.spritePixelsPerUnit}, " +
                  $"RuntimePPU={ppu}, FilterMode={ti.filterMode}, " +
                  $"SpriteCount={sprites.Length}, " +
                  $"TextureSize={ti.maxTextureSize}");

        // Check actual texture size
        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        if (tex != null)
            Debug.Log($"  Actual texture: {tex.width}x{tex.height}");
    }
}
