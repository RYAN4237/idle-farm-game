using UnityEditor;
using UnityEngine;

public class ImportBGReference
{
    [MenuItem("Tools/Import BG Reference")]
    public static void Run()
    {
        const string spritePath = "Assets/Sprites/sample_BG_only.png";

        var importer = AssetImporter.GetAtPath(spritePath) as TextureImporter;
        if (importer == null) { Debug.LogError("Importer not found: " + spritePath); return; }

        importer.textureType         = TextureImporterType.Sprite;
        importer.spriteImportMode    = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = 48f;
        importer.filterMode          = FilterMode.Point;
        importer.textureCompression  = TextureImporterCompression.Uncompressed;
        importer.mipmapEnabled       = false;

        var settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        settings.spriteMeshType  = SpriteMeshType.FullRect;
        settings.spriteAlignment = (int)SpriteAlignment.Center;
        importer.SetTextureSettings(settings);

        AssetDatabase.ImportAsset(spritePath, ImportAssetOptions.ForceUpdate);

        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
        if (sprite == null) { Debug.LogError("Sprite load failed: " + spritePath); return; }

        var old = GameObject.Find("BGReference");
        if (old != null) Object.DestroyImmediate(old);

        var go = new GameObject("BGReference");
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite       = sprite;
        sr.sortingOrder = -100;

        var mat = AssetDatabase.LoadAssetAtPath<Material>(
            "Packages/com.unity.render-pipelines.universal/Runtime/Materials/Sprite-Unlit-Default.mat");
        if (mat != null) sr.sharedMaterial = mat;

        // Center over camera visible area (camera pos x=10, y=2)
        go.transform.position = new Vector3(10f, 2f, 1f);

        EditorUtility.SetDirty(go);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        float wu = sprite.rect.width / 48f;
        float wh = sprite.rect.height / 48f;
        Debug.Log($"BGReference placed at (10,2,1), sortingOrder=-100, world size={wu:F1}x{wh:F1}");
    }
}
