using UnityEngine;
using UnityEditor;

public class ListFarmSprites
{
    [MenuItem("Farm/List Sprite Names")]
    public static void Execute()
    {
        string[] paths = {
            "Assets/Sprites/Farm_Trees.png",
            "Assets/Sprites/Farm_Ground.png",
            "Assets/Sprites/Farm_Plants.png",
            "Assets/Sprites/Farm_Deco.png",
        };
        foreach (var path in paths)
        {
            var all = AssetDatabase.LoadAllAssetsAtPath(path);
            var names = new System.Collections.Generic.List<string>();
            foreach (var obj in all)
                if (obj is Sprite s) names.Add(s.name);
            Debug.Log(path + " => [" + string.Join(", ", names) + "]");
        }
    }
}
