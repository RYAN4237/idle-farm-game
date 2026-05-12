using UnityEngine;
using UnityEditor;
using System.Linq;

public class InspectSereneVillage
{
    [MenuItem("Tools/Inspect Serene Village Assets")]
    public static void Execute()
    {
        string[] paths = {
            "Assets/SERENE_VILLAGE_REVAMPED/Serene_Village_16x16.png",
            "Assets/SERENE_VILLAGE_REVAMPED/Construct 3/Autotiles_no_inner_corners_16x16.png",
            "Assets/SERENE_VILLAGE_REVAMPED/Animated stuff/water_waves_16x16.png",
        };

        foreach (var path in paths)
        {
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            var sprites = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToArray();
            if (tex == null) { Debug.Log($"{System.IO.Path.GetFileName(path)}: NOT LOADED (needs import settings)"); continue; }
            Debug.Log($"{System.IO.Path.GetFileName(path)}: {tex.width}x{tex.height}px, {sprites.Length} sprites sliced");
        }
    }
}
