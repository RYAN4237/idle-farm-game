using UnityEngine;
using UnityEditor;
using System.Linq;

public class PreviewSprites2
{
    const string SHEET = "Assets/Farm Sprite.png";

    [MenuItem("Tools/Preview Sprites Grid 2")]
    public static void Execute()
    {
        var sprites = AssetDatabase.LoadAllAssetsAtPath(SHEET).OfType<Sprite>()
            .ToDictionary(s => { var p = s.name.Split('_'); int.TryParse(p[p.Length-1], out int i); return i; });

        var matGuid = AssetDatabase.FindAssets("Sprite-Unlit-Default t:Material")
            .Select(g => AssetDatabase.GUIDToAssetPath(g))
            .FirstOrDefault(p => p.Contains("render-pipelines"));
        var mat = matGuid != null ? AssetDatabase.LoadAssetAtPath<Material>(matGuid) : null;

        var old = GameObject.Find("_SpritePreview2");
        if (old != null) Object.DestroyImmediate(old);

        var root = new GameObject("_SpritePreview2");
        root.transform.position = new Vector3(30, 20, 0); // to the right

        // Show sprites 0-59 (rows 0-3: grass, green areas, etc.)
        // and also first row again for reference
        for (int idx = 0; idx <= 59; idx++)
        {
            if (!sprites.ContainsKey(idx)) continue;
            int row = idx / 16;
            int col = idx % 16;
            var go = new GameObject($"s{idx}");
            go.transform.SetParent(root.transform, false);
            go.transform.localPosition = new Vector3(col * 1.2f, -row * 1.2f, 0f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprites[idx];
            sr.sortingOrder = 50;
            if (mat) sr.sharedMaterial = mat;
        }

        Debug.Log("[Preview2] Showing sprites 0-59 (rows 0-3)");
    }
}
