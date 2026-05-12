#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public static class ListSprites
{
    public static void Execute()
    {
        string[] sheets = {
            "Assets/Sprout Lands - Sprites - Basic pack/Tilesets/Water.png",
            "Assets/Sprout Lands - Sprites - Basic pack/Objects/Basic_Grass_Biom_things.png",
            "Assets/Sprout Lands - Sprites - Basic pack/Objects/Basic_Plants.png",
            "Assets/Sprout Lands - Sprites - Basic pack/Objects/Basic Grass Biom things 1.png",
            "Assets/Sprout Lands - Sprites - Basic pack/Tilesets/Grass.png",
        };

        foreach (var path in sheets)
        {
            var all = AssetDatabase.LoadAllAssetsAtPath(path);
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine($"=== {path} ===");
            foreach (var a in all)
                if (a is Sprite s) sb.AppendLine($"  [{s.name}] rect={s.rect} pivot={s.pivot}");
            Debug.Log(sb.ToString());
        }
    }
}
#endif
