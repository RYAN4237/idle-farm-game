using UnityEditor;
using UnityEngine;
using System.Linq;

/// Dumps all sprite names and rects from a sheet so we can identify correct indices.
public class DumpSpriteSheet
{
    [MenuItem("Tools/Dump Biom Sprites")]
    public static void DumpBiom()
        => Dump("Assets/Sprout Lands - Sprites - Basic pack/Objects/Basic Grass Biom things 1.png");

    [MenuItem("Tools/Dump Bridge Sprites")]
    public static void DumpBridge()
        => Dump("Assets/Sprout Lands - Sprites - Basic pack/Objects/Wood Bridge.png");

    [MenuItem("Tools/Dump Grass Sprites")]
    public static void DumpGrass()
        => Dump("Assets/Sprout Lands - Sprites - Basic pack/Tilesets/Grass.png");

    [MenuItem("Tools/Dump Plants Sprites")]
    public static void DumpPlants()
        => Dump("Assets/Sprout Lands - Sprites - Basic pack/Objects/Basic Plants.png");

    static void Dump(string path)
    {
        var sprites = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToArray();
        // Sort by rect position (top-to-bottom, left-to-right)
        var sorted = sprites.OrderBy(s => -s.rect.y).ThenBy(s => s.rect.x).ToArray();
        Debug.Log($"[Dump] {path} — {sorted.Length} sprites:");
        for (int i = 0; i < sorted.Length; i++)
        {
            var s = sorted[i];
            Debug.Log($"  [{i:00}] name={s.name} rect=({s.rect.x},{s.rect.y},{s.rect.width}x{s.rect.height})");
        }
    }
}
