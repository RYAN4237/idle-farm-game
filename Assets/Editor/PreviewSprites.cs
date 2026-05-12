using UnityEngine;
using UnityEditor;
using System.Linq;

/// Preview a range of sprites placed in a grid for visual identification
public class PreviewSprites
{
    const string SHEET = "Assets/Farm Sprite.png";
    const string MAT = "Sprite-Unlit-Default";

    [MenuItem("Tools/Preview Sprites Grid")]
    public static void Execute()
    {
        var sprites = AssetDatabase.LoadAllAssetsAtPath(SHEET).OfType<Sprite>()
            .ToDictionary(s => { var p = s.name.Split('_'); int.TryParse(p[p.Length-1], out int i); return i; });

        var matGuid = AssetDatabase.FindAssets("Sprite-Unlit-Default t:Material")
            .Select(g => AssetDatabase.GUIDToAssetPath(g))
            .FirstOrDefault(p => p.Contains("render-pipelines"));
        var mat = matGuid != null ? AssetDatabase.LoadAssetAtPath<Material>(matGuid) : null;

        // Remove old preview
        var old = GameObject.Find("_SpritePreview");
        if (old != null) Object.DestroyImmediate(old);

        var root = new GameObject("_SpritePreview");
        root.transform.position = new Vector3(0, 20, 0); // above scene

        // Show sprites 60-180 in a grid (rows 4-11, likely to contain trees/plants/water)
        int startIdx = 60, endIdx = 180, cols = 16;
        for (int idx = startIdx; idx <= endIdx; idx++)
        {
            if (!sprites.ContainsKey(idx)) continue;
            int row = (idx - startIdx) / cols;
            int col = (idx - startIdx) % cols;
            var go = new GameObject($"s{idx}");
            go.transform.SetParent(root.transform, false);
            go.transform.localPosition = new Vector3(col * 1.2f, -row * 1.2f, 0f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprites[idx];
            sr.sortingOrder = 50;
            if (mat) sr.sharedMaterial = mat;
        }

        Debug.Log($"[Preview] Placed sprites {startIdx}-{endIdx} at world y=20. Camera row: idx starts at row {startIdx/16}.");
    }
}
