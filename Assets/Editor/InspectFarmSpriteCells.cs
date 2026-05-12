using UnityEngine;
using UnityEditor;
using System.Linq;

/// Inspect individual Farm Sprite cells to identify grass/water/tree tiles
public class InspectFarmSpriteCells
{
    const string PATH = "Assets/Farm Sprite.png";

    static Sprite[] _all;

    static Sprite[] All() => _all ??= AssetDatabase.LoadAllAssetsAtPath(PATH)
        .OfType<Sprite>().OrderByDescending(s => s.rect.y).ThenBy(s => s.rect.x).ToArray();

    // Render a visual tile map: capture scene screenshot with tile preview grid
    [MenuItem("Tools/Farm Sprite - Preview Row 0")]
    public static void PreviewRow0() => Preview(0, 0, 16);

    [MenuItem("Tools/Farm Sprite - Preview Row 8 (Water)")]
    public static void PreviewRow8() => Preview(8, 128, 16);

    [MenuItem("Tools/Farm Sprite - Preview Rows 0-3")]
    public static void PreviewRows03()
    {
        // Place all tiles from rows 0-3 in a grid in the scene for visual inspection
        var sprites = All();
        var old = GameObject.Find("_TilePreview");
        if (old != null) Object.DestroyImmediate(old);
        var root = new GameObject("_TilePreview");

        int mat = FindUnlitMat() != null ? 1 : 0;
        var unlitMat = FindUnlitMat();

        for (int row = 0; row < 4; row++)
        for (int col = 0; col < 16; col++)
        {
            int idx = row * 16 + col;
            if (idx >= sprites.Length) continue;
            var go = new GameObject($"t_{row}_{col}");
            go.transform.SetParent(root.transform, false);
            go.transform.position = new Vector3(col * 1.1f, -row * 1.1f, 0f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprites[idx];
            sr.sortingOrder = 10;
            if (unlitMat != null) sr.sharedMaterial = unlitMat;
        }
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("[FarmInspect] Preview placed at origin. Rows 0-3, cols 0-15.");
    }

    [MenuItem("Tools/Farm Sprite - Preview All Rows")]
    public static void PreviewAllRows()
    {
        var sprites = All();
        var old = GameObject.Find("_TilePreview");
        if (old != null) Object.DestroyImmediate(old);
        var root = new GameObject("_TilePreview");
        var unlitMat = FindUnlitMat();

        for (int row = 0; row < 16; row++)
        for (int col = 0; col < 16; col++)
        {
            int idx = row * 16 + col;
            if (idx >= sprites.Length) continue;
            var go = new GameObject($"t_{row}_{col}_{idx}");
            go.transform.SetParent(root.transform, false);
            go.transform.position = new Vector3(col * 1.1f, -row * 1.1f, 0f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprites[idx];
            sr.sortingOrder = 10;
            if (unlitMat != null) sr.sharedMaterial = unlitMat;
        }
        // Center camera on preview
        var sv = UnityEditor.SceneView.lastActiveSceneView;
        if (sv != null)
        {
            sv.pivot = new Vector3(7.7f, -7.7f, 0f);
            sv.size = 10f;
            sv.Repaint();
        }
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("[FarmInspect] All 256 tiles placed. 16 cols x 16 rows, 1.1 unit spacing.");
    }

    [MenuItem("Tools/Farm Sprite - Remove Preview")]
    public static void RemovePreview()
    {
        var old = GameObject.Find("_TilePreview");
        if (old != null) Object.DestroyImmediate(old);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    }

    static void Preview(int startRow, int startIdx, int count)
    {
        var sprites = All();
        var old = GameObject.Find("_TilePreview");
        if (old != null) Object.DestroyImmediate(old);
        var root = new GameObject("_TilePreview");
        var unlitMat = FindUnlitMat();
        for (int i = 0; i < count && startIdx + i < sprites.Length; i++)
        {
            var go = new GameObject($"t_{startRow}_{i}_{startIdx+i}");
            go.transform.SetParent(root.transform, false);
            go.transform.position = new Vector3(i * 1.1f, 0f, 0f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprites[startIdx + i];
            sr.sortingOrder = 10;
            if (unlitMat != null) sr.sharedMaterial = unlitMat;
        }
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
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
