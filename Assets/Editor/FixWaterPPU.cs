using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using System.Linq;

/// Fix Water.png PPU to 16 so tiles fill their 1x1 cell with no gaps
public class FixWaterPPU
{
    [MenuItem("Tools/Fix Water PPU")]
    public static void Execute()
    {
        string waterPath = "Assets/Sprout Lands - Sprites - Basic pack/Tilesets/Water.png";
        string grassPath = "Assets/Sprout Lands - Sprites - Basic pack/Tilesets/Grass.png";

        // Fix Water PPU
        FixPPU(waterPath, 16);
        // Fix Grass PPU too
        FixPPU(grassPath, 16);

        AssetDatabase.Refresh();

        // Verify
        var waterSprites = AssetDatabase.LoadAllAssetsAtPath(waterPath).OfType<Sprite>().ToArray();
        var grassSprites = AssetDatabase.LoadAllAssetsAtPath(grassPath).OfType<Sprite>().ToArray();

        if (waterSprites.Length > 0)
            Debug.Log($"Water_0 PPU={waterSprites[0].pixelsPerUnit}, rect={waterSprites[0].rect}");
        if (grassSprites.Length > 0)
            Debug.Log($"Grass_0 PPU={grassSprites[0].pixelsPerUnit}, rect={grassSprites[0].rect}");

        Debug.Log("[FixWaterPPU] Done");
    }

    static void FixPPU(string path, float ppu)
    {
        var ti = AssetImporter.GetAtPath(path) as TextureImporter;
        if (ti == null) { Debug.LogError($"Cannot load importer for {path}"); return; }

        ti.spritePixelsPerUnit = ppu;
        ti.filterMode = FilterMode.Point;
        ti.textureCompression = TextureImporterCompression.Uncompressed;
        EditorUtility.SetDirty(ti);
        ti.SaveAndReimport();
        Debug.Log($"[FixWaterPPU] {path} PPU set to {ppu}");
    }
}
