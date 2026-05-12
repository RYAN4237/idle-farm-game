using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using System.Linq;

/// Quick test: try different grass fill tile indices in a 5x5 patch 
/// to visually compare which looks best as a tiled ground.
public class TestGrassFill
{
    const string SHEET = "Assets/Farm Sprite.png";

    [MenuItem("Tools/Test Grass Fill Tiles")]
    public static void Execute()
    {
        var sprites = AssetDatabase.LoadAllAssetsAtPath(SHEET)
            .OfType<Sprite>()
            .OrderByDescending(s => s.rect.y).ThenBy(s => s.rect.x)
            .ToArray();

        var mat = FindUnlitMat();

        var old = GameObject.Find("_GrassFillTest");
        if (old != null) Object.DestroyImmediate(old);
        var root = new GameObject("_GrassFillTest");

        // Test these candidate indices as 4x4 tile patches
        int[] candidates = { 0, 1, 2, 3, 16, 17, 18, 19, 32, 33, 48, 64, 146, 149, 162, 201 };

        for (int ci = 0; ci < candidates.Length; ci++)
        {
            int idx = candidates[ci];
            if (idx >= sprites.Length) continue;
            var spr = sprites[idx];

            float ox = (ci % 8) * 5f;
            float oy = -(ci / 8) * 5f;

            // Label
            var label = new GameObject($"lbl_{idx}");
            label.transform.SetParent(root.transform, false);
            label.transform.position = new Vector3(ox + 2f, oy + 4.5f, 0f);

            // 4x4 patch of this tile
            for (int tx = 0; tx < 4; tx++)
            for (int ty = 0; ty < 4; ty++)
            {
                var go = new GameObject($"t{idx}_{tx}_{ty}");
                go.transform.SetParent(root.transform, false);
                go.transform.position = new Vector3(ox + tx, oy + ty, 0f);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = spr;
                sr.sortingOrder = 50;
                if (mat) sr.sharedMaterial = mat;
            }
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
