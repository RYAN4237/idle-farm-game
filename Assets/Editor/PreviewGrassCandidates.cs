using UnityEngine;
using UnityEditor;
using System.Linq;

/// Preview rows 0-4 of Farm Sprite.png to visually identify grass tiles.
public class PreviewGrassCandidates
{
    [MenuItem("Tools/Preview Rows 0-4 Tiles")]
    public static void Execute()
    {
        var sprites = AssetDatabase.LoadAllAssetsAtPath("Assets/Farm Sprite.png")
            .OfType<Sprite>()
            .OrderByDescending(s => s.rect.y).ThenBy(s => s.rect.x)
            .ToArray();

        var mat = FindUnlitMat();

        var old = GameObject.Find("_GrassPreview");
        if (old != null) Object.DestroyImmediate(old);
        var root = new GameObject("_GrassPreview");

        // Show rows 5-15 (idx 80-255) in a grid — looking for trees/bridge/decorations
        for (int idx = 80; idx < 256; idx++)
        {
            if (idx >= sprites.Length) break;
            int offset = idx - 80;
            int row = offset / 16;
            int col = offset % 16;
            var go = new GameObject($"tile_{idx:000}");
            go.transform.SetParent(root.transform, false);
            go.transform.position = new Vector3(col * 1.2f - 8f, -row * 1.2f + 4f, 0f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprites[idx];
            sr.sortingOrder = 100;
            if (mat) sr.sharedMaterial = mat;
        }
    }

    static Material FindUnlitMat()
    {
        foreach (var g in AssetDatabase.FindAssets("Sprite-Unlit-Default t:Material"))
        {
            var p = AssetDatabase.GUIDToAssetPath(g);
            if (p.Contains("com.unity.render-pipelines")) return AssetDatabase.LoadAssetAtPath<Material>(p);
        }
        return null;
    }
}
