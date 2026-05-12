using UnityEngine;
using UnityEditor;
using System.Linq;

/// Preview specific row ranges to identify water, sand, dirt tiles
public class PreviewFarmKeyRows
{
    [MenuItem("Tools/Farm Sprite - Preview Rows 5-8 (Sand+Water)")]
    public static void PreviewRows58()
    {
        string path = "Assets/Farm Sprite.png";
        var sprites = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>()
            .OrderByDescending(s => s.rect.y).ThenBy(s => s.rect.x).ToArray();

        var old = GameObject.Find("_TilePreview");
        if (old != null) Object.DestroyImmediate(old);
        var root = new GameObject("_TilePreview");
        var mat = FindUnlitMat();

        // Rows 5,6,7,8 = idx 80-143 (sand + water)
        for (int r = 0; r < 4; r++)
        for (int c = 0; c < 16; c++)
        {
            int idx = (5 + r) * 16 + c;
            if (idx >= sprites.Length) continue;
            var go = new GameObject($"r{5+r}c{c}_i{idx}");
            go.transform.SetParent(root.transform, false);
            go.transform.position = new Vector3(c * 1.2f, -r * 1.2f, 0f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprites[idx];
            sr.sortingOrder = 20;
            if (mat != null) sr.sharedMaterial = mat;
        }

        var sv = UnityEditor.SceneView.lastActiveSceneView;
        if (sv != null) { sv.pivot = new Vector3(9f, -1.5f, 0f); sv.size = 9f; sv.Repaint(); }
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("[PreviewRows58] Rows 5-8 placed. Sand=row5-7, Water=row8.");
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
