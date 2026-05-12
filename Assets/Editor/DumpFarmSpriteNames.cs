using UnityEngine;
using UnityEditor;
using System.Linq;

/// Dump all sprite names from Farm Sprite.png and show any with rect info
public class DumpFarmSpriteNames
{
    [MenuItem("Tools/Farm Sprite - Dump Sprite Names to Console")]
    public static void Execute()
    {
        string path = "Assets/Farm Sprite.png";
        var sprites = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToArray();
        Debug.Log($"[DumpNames] Total sprites: {sprites.Length}");

        // Show all sprites with their rect info to find the brown wood ones
        var sortedByRect = sprites.OrderByDescending(s => s.rect.y).ThenBy(s => s.rect.x).ToArray();
        for (int i = 0; i < sortedByRect.Length; i++)
        {
            var s = sortedByRect[i];
            Debug.Log($"idx={i:000} name={s.name} rect=({s.rect.x},{s.rect.y},{s.rect.width}x{s.rect.height})");
        }
    }
}
