using UnityEngine;
using UnityEditor;
using System.Linq;

/// Place rows 0-1 only, spread out so we can see each tile clearly
public class PreviewFarmRows01
{
    [MenuItem("Tools/Farm Sprite - Preview Rows 0-1 Zoomed")]
    public static void Execute()
    {
        string path = "Assets/Farm Sprite.png";
        var sprites = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>()
            .OrderByDescending(s => s.rect.y).ThenBy(s => s.rect.x).ToArray();

        var old = GameObject.Find("_TilePreview");
        if (old != null) Object.DestroyImmediate(old);
        var root = new GameObject("_TilePreview");
        var unlitMat = FindUnlitMat();

        // Row 0: idx 0-15 at y=2, spaced 1.5 apart
        // Row 1: idx 16-31 at y=0
        for (int col = 0; col < 16; col++)
        {
            Place(root.transform, unlitMat, sprites, col,       col * 1.5f, 2f,  $"r0c{col}_idx{col}");
            Place(root.transform, unlitMat, sprites, 16 + col,  col * 1.5f, 0f,  $"r1c{col}_idx{16+col}");
        }

        var sv = UnityEditor.SceneView.lastActiveSceneView;
        if (sv != null) { sv.pivot = new Vector3(11f, 1f, 0f); sv.size = 14f; sv.Repaint(); }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("[Preview01] Rows 0-1 placed. idx 0-15 at y=2, idx 16-31 at y=0.");
    }

    static void Place(Transform parent, Material mat, Sprite[] sprites, int idx,
                      float x, float y, string name)
    {
        if (idx >= sprites.Length) return;
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(x, y, 0f);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprites[idx];
        sr.sortingOrder = 20;
        if (mat != null) sr.sharedMaterial = mat;
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
