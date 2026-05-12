using UnityEngine;
using UnityEditor;

public class SetupFarmScene
{
    [MenuItem("Farm/Setup Background Scene")]
    public static void Execute()
    {
        // Fix import settings
        string[] sprites = new[]
        {
            "Assets/Sprites/Farm_Reference.png",
            "Assets/Sprites/Farm_Trees.png",
            "Assets/Sprites/Farm_Ground.png",
            "Assets/Sprites/Farm_Plants.png",
            "Assets/Sprites/Farm_Deco.png",
        };

        foreach (var path in sprites)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) { Debug.LogWarning("Not found: " + path); continue; }

            importer.textureType = TextureImporterType.Sprite;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.maxTextureSize = 2048;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = path.Contains("Farm_Reference") ? 100 : 32;
            importer.SaveAndReimport();
            Debug.Log("Imported: " + path);
        }

        // Reference layer
        var refSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Farm_Reference.png");
        var refObj = new GameObject("Reference_Layer");
        var refSR = refObj.AddComponent<SpriteRenderer>();
        refSR.sprite = refSprite;
        refSR.sortingOrder = -999;
        refSR.color = new Color(1, 1, 1, 0.6f);
        refObj.tag = "EditorOnly";
        refObj.transform.position = Vector3.zero;
        if (refSprite != null)
        {
            float scale = 10f / refSprite.bounds.size.y;
            refObj.transform.localScale = new Vector3(scale, scale, 1);
        }

        // Layer hierarchy
        var bgRoot = new GameObject("Background");
        var layers = new (string name, int order)[]
        {
            ("BG_Sky",           -10),
            ("BG_Treeline_Back",  -8),
            ("BG_Ground",         -6),
            ("BG_River",          -4),
            ("BG_Bridge",         -3),
            ("BG_Props",          -2),
            ("BG_Crops",          -1),
            ("BG_Trees_Front",     0),
            ("BG_Plants_Front",    1),
        };
        foreach (var (name, order) in layers)
        {
            var go = new GameObject(name);
            go.transform.SetParent(bgRoot.transform);
            go.AddComponent<SpriteRenderer>().sortingOrder = order;
        }

        // Camera
        if (Camera.main != null)
        {
            Camera.main.orthographic = true;
            Camera.main.orthographicSize = 5;
            Camera.main.backgroundColor = new Color(0.494f, 0.784f, 0.894f);
            Camera.main.transform.position = new Vector3(0, 0, -10);
        }

        Debug.Log("Done. Assign sprites to each BG_ layer in Inspector.");
    }
}
