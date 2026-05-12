using UnityEngine;
using UnityEditor;
using System.Linq;

/// Preview rows 5-11 to identify sand, water, crop, bridge tiles
public class PreviewFarmRows511
{
    [MenuItem("Tools/Farm Sprite - Preview Rows 5-11")]
    public static void Execute()
    {
        string path = "Assets/Farm Sprite.png";
        var sprites = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>()
            .OrderByDescending(s => s.rect.y).ThenBy(s => s.rect.x).ToArray();

        var old = GameObject.Find("_TilePreview");
        if (old != null) Object.DestroyImmediate(old);
        var root = new GameObject("_TilePreview");
        var mat = FindUnlitMat();

        float offX = 50f, offY = 0f;
        for (int r = 5; r <= 11; r++)
        for (int c = 0; c < 16; c++)
        {
            int idx = r * 16 + c;
            if (idx >= sprites.Length) continue;
            var go = new GameObject($"r{r}c{c}_i{idx}");
            go.transform.SetParent(root.transform, false);
            go.transform.position = new Vector3(offX + c * 1.3f, offY - (r-5) * 1.3f, 0f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprites[idx];
            sr.sortingOrder = 20;
            if (mat != null) sr.sharedMaterial = mat;
        }

        var sv = UnityEditor.SceneView.lastActiveSceneView;
        if (sv != null) { sv.pivot = new Vector3(offX + 9.5f, offY - 4f, 0f); sv.size = 14f; sv.Repaint(); }
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log($"[PreviewRows511] {sprites.Length} total sprites. Rows 5-11 placed at x=50+.");
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
