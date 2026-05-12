using UnityEngine;
using UnityEditor;
using System.Linq;

/// Preview rows 0-4 far from main scene to identify actual tree tiles
public class PreviewFarmRows04
{
    [MenuItem("Tools/Farm Sprite - Preview Rows 0-4 (Trees+Decor)")]
    public static void Execute()
    {
        string path = "Assets/Farm Sprite.png";
        var sprites = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>()
            .OrderByDescending(s => s.rect.y).ThenBy(s => s.rect.x).ToArray();

        var old = GameObject.Find("_TilePreview");
        if (old != null) Object.DestroyImmediate(old);
        var root = new GameObject("_TilePreview");
        var mat = FindUnlitMat();

        // Place WAY to the left (x=50+) so it doesn't overlap the main scene
        float offX = 50f, offY = 0f;
        for (int r = 0; r < 5; r++)
        for (int c = 0; c < 16; c++)
        {
            int idx = r * 16 + c;
            if (idx >= sprites.Length) continue;
            var go = new GameObject($"r{r}c{c}_i{idx}");
            go.transform.SetParent(root.transform, false);
            go.transform.position = new Vector3(offX + c * 1.3f, offY - r * 1.3f, 0f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprites[idx];
            sr.sortingOrder = 20;
            if (mat != null) sr.sharedMaterial = mat;
        }

        // Focus scene view on preview area
        var sv = UnityEditor.SceneView.lastActiveSceneView;
        if (sv != null) { sv.pivot = new Vector3(offX + 9.5f, offY - 2.5f, 0f); sv.size = 11f; sv.Repaint(); }
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("[PreviewRows04] Rows 0-4 placed at x=50+. Identify tree sprites by position.");
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
