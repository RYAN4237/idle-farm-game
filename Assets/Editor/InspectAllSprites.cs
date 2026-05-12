using UnityEngine;
using UnityEditor;
using System.Linq;

public class InspectAllSprites
{
    [MenuItem("Tools/Inspect Water and Grass Biom Sprites")]
    public static void Execute()
    {
        InspectSheet("Assets/Sprout Lands - Sprites - Basic pack/Tilesets/Water.png");
        InspectSheet("Assets/Sprout Lands - Sprites - Basic pack/Objects/Basic_Grass_Biom_things.png");
    }

    static void InspectSheet(string path)
    {
        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        var sprites = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToArray();
        if (tex == null) { Debug.LogError($"Cannot load {path}"); return; }

        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"=== {System.IO.Path.GetFileName(path)} === {tex.width}x{tex.height}px, {sprites.Length} sprites");
        foreach (var s in sprites.OrderBy(x => x.name))
        {
            var r = s.textureRect;
            sb.AppendLine($"  {s.name}  rect=({(int)r.x},{(int)r.y},{(int)r.width},{(int)r.height})");
        }
        Debug.Log(sb.ToString());
    }
}
