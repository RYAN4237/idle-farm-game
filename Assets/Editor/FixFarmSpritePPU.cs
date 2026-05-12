using UnityEngine;
using UnityEditor;
using System.Linq;

/// Fix Farm Sprite.png PPU to 64 so each 64px tile = 1 world unit
public class FixFarmSpritePPU
{
    const string PATH = "Assets/Farm Sprite.png";

    [MenuItem("Tools/Farm Sprite - Fix PPU=64")]
    public static void Execute()
    {
        var ti = AssetImporter.GetAtPath(PATH) as TextureImporter;
        if (ti == null) { Debug.LogError("Not found: " + PATH); return; }

        ti.spritePixelsPerUnit = 64f;
        ti.filterMode = FilterMode.Point;
        ti.textureCompression = TextureImporterCompression.Uncompressed;
        ti.alphaIsTransparency = true;
        ti.mipmapEnabled = false;
        ti.SaveAndReimport();
        AssetDatabase.Refresh();
        Debug.Log("[FarmSprite] PPU set to 64. Each tile = 1 world unit.");
    }

    /// Dump all 256 sprite names with their grid row/col for identification
    [MenuItem("Tools/Farm Sprite - Dump All 256")]
    public static void DumpAll()
    {
        var sprites = AssetDatabase.LoadAllAssetsAtPath(PATH).OfType<Sprite>()
            .OrderByDescending(s => s.rect.y).ThenBy(s => s.rect.x).ToArray();
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"[FarmSprite] {sprites.Length} sprites. Grid=16x16. Row/Col from top-left:");
        for (int i = 0; i < sprites.Length; i++)
        {
            int row = i / 16;
            int col = i % 16;
            var s = sprites[i];
            sb.AppendLine($"  idx={i:000} row={row:00} col={col:00} name={s.name} rect=({s.rect.x},{s.rect.y})");
        }
        Debug.Log(sb.ToString());
    }
}
