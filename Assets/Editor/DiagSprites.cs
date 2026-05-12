using UnityEngine;
using UnityEditor;
using System.Linq;

public class DiagSprites
{
    [MenuItem("Tools/Diag Sprites")]
    public static void Execute()
    {
        string[] paths = {
            "Assets/Sprites/FarmBG_Ground.png",
            "Assets/Sprites/FarmBG_Deco.png"
        };
        foreach (var p in paths)
        {
            var all = AssetDatabase.LoadAllAssetsAtPath(p);
            var sprites = all.OfType<Sprite>().ToArray();
            Debug.Log($"[Diag] {p}: total assets={all.Length}, sprites={sprites.Length}");
            if (sprites.Length > 0)
                Debug.Log($"[Diag] First 5 names: {string.Join(", ", sprites.Take(5).Select(s=>s.name))}");
            
            // Check importer settings
            var ti = AssetImporter.GetAtPath(p) as TextureImporter;
            if (ti != null)
                Debug.Log($"[Diag] Importer: type={ti.textureType}, mode={ti.spriteImportMode}, spritesheet count={ti.spritesheet?.Length}");
        }
    }
}
