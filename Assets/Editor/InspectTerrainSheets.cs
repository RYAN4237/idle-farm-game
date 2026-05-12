using UnityEngine;
using UnityEditor;
using System.Linq;

public class InspectTerrainSheets
{
    [MenuItem("Tools/Inspect Terrain Sheets")]
    public static void Execute()
    {
        string[] paths = {
            "Assets/SERENE_VILLAGE_REVAMPED/RPG_MAKER_MV/Terrains_TILESET_B-C-D-E.png",
            "Assets/Pixel Art Top Down - Basic v1.2.3/Texture/TX Tileset Grass.png",
            "Assets/Pixel Art Top Down - Basic v1.2.3/Texture/TX Tileset Stone Ground.png",
        };

        foreach (var path in paths)
        {
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            var sprites = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToArray();
            if (tex == null) { Debug.Log($"NOT LOADED: {System.IO.Path.GetFileName(path)}"); continue; }
            Debug.Log($"{System.IO.Path.GetFileName(path)}: {tex.width}x{tex.height}px, {sprites.Length} sprites sliced, filterMode={tex.filterMode}, format={tex.format}");
        }
    }
}
