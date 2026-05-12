using UnityEngine;
using UnityEditor;
using System.Linq;

public class DumpFarmSpriteRects
{
    [MenuItem("Tools/Dump Farm Sprite Rects")]
    public static void Execute()
    {
        string path = "Assets/Farm Sprite.png";
        var sprites = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>()
            .OrderByDescending(s => s.rect.y).ThenBy(s => s.rect.x).ToArray();

        // Print first 50 sorted (top-row, left-to-right)
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"[FarmRects] Total: {sprites.Length}");
        for (int i = 0; i < Mathf.Min(50, sprites.Length); i++)
        {
            var s = sprites[i];
            sb.AppendLine($"  [{i:00}] {s.name}  x={s.rect.x} y={s.rect.y} w={s.rect.width} h={s.rect.height}");
        }
        Debug.Log(sb.ToString());
    }
}
