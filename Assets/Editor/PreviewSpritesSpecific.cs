using UnityEngine;
using UnityEditor;
using System.Linq;

// Preview specific sprite rows to identify crop and trunk tiles
public class PreviewSpritesSpecific
{
    const string SHEET = "Assets/Farm Sprite.png";

    [MenuItem("Tools/Preview Specific Sprites")]
    public static void Execute()
    {
        var sprites = AssetDatabase.LoadAllAssetsAtPath(SHEET).OfType<Sprite>()
            .ToDictionary(s => { var p = s.name.Split('_'); int.TryParse(p[p.Length-1], out int i); return i; });

        var matGuid = AssetDatabase.FindAssets("Sprite-Unlit-Default t:Material")
            .Select(g => AssetDatabase.GUIDToAssetPath(g))
            .FirstOrDefault(p => p.Contains("render-pipelines"));
        var mat = matGuid != null ? AssetDatabase.LoadAssetAtPath<Material>(matGuid) : null;

        var old = GameObject.Find("_SPSpecific");
        if (old != null) Object.DestroyImmediate(old);
        var root = new GameObject("_SPSpecific");
        root.transform.position = new Vector3(-5, 25, 0);

        // Show in labeled groups: crops (48-63), plants (160-175), and idx 160-185
        int[] toShow = {
            // Row 3 (idx 48-63): check these "RED" tiles
            48,49,50,51,52,53,54,55,56,57,58,59,60,61,62,63,
            // gap
            // Row 10 (idx 160-175): YEL/warm - possible crops
            160,161,162,163,164,165,166,167,168,169,170,171,172,173,174,175
        };

        for (int i = 0; i < toShow.Length; i++)
        {
            int idx = toShow[i];
            int row = i / 16;
            int col = i % 16;
            if (!sprites.ContainsKey(idx)) continue;
            var go = new GameObject($"s{idx}");
            go.transform.SetParent(root.transform, false);
            go.transform.localPosition = new Vector3(col * 1.3f, -row * 1.3f, 0f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprites[idx];
            sr.sortingOrder = 50;
            if (mat) sr.sharedMaterial = mat;
        }
        Debug.Log("[SPSpecific] Done");
    }
}
