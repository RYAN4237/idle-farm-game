using UnityEngine;
using UnityEditor;
using System.Linq;

/// Spot-check specific tile indices used for bridge
public class PreviewBridgeTiles
{
    [MenuItem("Tools/Farm Sprite - Preview Bridge Tiles")]
    public static void Execute()
    {
        string path = "Assets/Farm Sprite.png";
        var sprites = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>()
            .OrderByDescending(s => s.rect.y).ThenBy(s => s.rect.x).ToArray();

        var old = GameObject.Find("_TilePreview");
        if (old != null) Object.DestroyImmediate(old);
        var root = new GameObject("_TilePreview");
        var mat = FindUnlitMat();

        // Preview specific indices related to bridge
        int[] idxToTest = { 80, 81, 82, 83, 84, 85, 86, 87, 88, 89, 90, 91, 92, 93, 94, 95,
                            96, 97, 98, 99, 100, 101, 102, 103, 104, 105, 106, 107, 108, 109, 110, 111 };

        float offX = 50f;
        for (int i = 0; i < idxToTest.Length; i++)
        {
            int idx = idxToTest[i];
            if (idx >= sprites.Length) continue;
            float x = offX + (i % 16) * 1.3f;
            float y = -(i / 16) * 1.3f;
            var go = new GameObject($"i{idx}");
            go.transform.SetParent(root.transform, false);
            go.transform.position = new Vector3(x, y, 0f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprites[idx];
            sr.sortingOrder = 20;
            if (mat != null) sr.sharedMaterial = mat;
        }

        var sv = UnityEditor.SceneView.lastActiveSceneView;
        if (sv != null) { sv.pivot = new Vector3(offX + 9.5f, -0.65f, 0f); sv.size = 7f; sv.Repaint(); }
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log($"[PreviewBridgeTiles] Rows 5-6 (idx 80-111). Total sprites: {sprites.Length}");
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
