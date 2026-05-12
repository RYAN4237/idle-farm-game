using UnityEngine;
using UnityEditor;
using System.Linq;

/// Place tiles in a grid with large spacing so we can zoom in on specific areas
public class PreviewFarmTileBlock
{
    static Material FindUnlitMat()
    {
        foreach (var g in AssetDatabase.FindAssets("Sprite-Unlit-Default t:Material"))
        {
            var p = AssetDatabase.GUIDToAssetPath(g);
            if (p.Contains("com.unity.render-pipelines")) return AssetDatabase.LoadAssetAtPath<Material>(p);
        }
        return null;
    }

    /// Preview a specific rectangular block of tiles, zoomed in
    static void PreviewBlock(int startIdx, int count, int cols, float startX = 0f, float startY = 0f)
    {
        string path = "Assets/Farm Sprite.png";
        var sprites = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>()
            .OrderByDescending(s => s.rect.y).ThenBy(s => s.rect.x).ToArray();

        var old = GameObject.Find("_TilePreview");
        if (old != null) Object.DestroyImmediate(old);
        var root = new GameObject("_TilePreview");
        var mat = FindUnlitMat();

        for (int i = 0; i < count; i++)
        {
            int idx = startIdx + i;
            if (idx >= sprites.Length) break;
            int col = i % cols;
            int row = i / cols;
            var go = new GameObject($"i{idx}_r{idx/16}_c{idx%16}");
            go.transform.SetParent(root.transform, false);
            go.transform.position = new Vector3(startX + col * 2f, startY - row * 2f, 0f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprites[idx];
            sr.sortingOrder = 20;
            if (mat != null) sr.sharedMaterial = mat;
        }

        int midCol = (cols / 2);
        int midRow = ((count / cols) / 2);
        var sv = UnityEditor.SceneView.lastActiveSceneView;
        if (sv != null)
        {
            sv.pivot = new Vector3(startX + midCol * 2f, startY - midRow * 2f, 0f);
            sv.size = Mathf.Max(cols, count / cols) * 1.5f;
            sv.Repaint();
        }
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    }

    [MenuItem("Tools/Farm Sprite - Preview Top-Right (idx 12-15, 28-31)")]
    public static void PreviewTopRight()
    {
        // Row 0 col 12-15 = idx 12,13,14,15 and row 1 col 12-15 = idx 28,29,30,31
        // These are the bright green tiles seen on right side of rows01 preview
        PreviewBlock(12, 4, 4, 0f, 4f);
        // Also add row 1 right side
        string path = "Assets/Farm Sprite.png";
        var sprites = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>()
            .OrderByDescending(s => s.rect.y).ThenBy(s => s.rect.x).ToArray();
        var root = GameObject.Find("_TilePreview").transform;
        var mat = FindUnlitMat();
        for (int i = 0; i < 4; i++)
        {
            int idx = 28 + i;
            var go = new GameObject($"i{idx}_r1c{12+i}");
            go.transform.SetParent(root, false);
            go.transform.position = new Vector3(i * 2f, 2f, 0f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprites[idx];
            sr.sortingOrder = 20;
            if (mat != null) sr.sharedMaterial = mat;
        }
        // Row 2 right side: idx 44-47
        for (int i = 0; i < 4; i++)
        {
            int idx = 44 + i;
            var go = new GameObject($"i{idx}_r2c{12+i}");
            go.transform.SetParent(root, false);
            go.transform.position = new Vector3(i * 2f, 0f, 0f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprites[idx];
            sr.sortingOrder = 20;
            if (mat != null) sr.sharedMaterial = mat;
        }
        var sv = UnityEditor.SceneView.lastActiveSceneView;
        if (sv != null) { sv.pivot = new Vector3(3f, 2f, 0f); sv.size = 7f; sv.Repaint(); }
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("[Preview] Top-right tiles: row0 idx12-15 (y=4), row1 idx28-31 (y=2), row2 idx44-47 (y=0)");
    }

    [MenuItem("Tools/Farm Sprite - Preview All Rows Spaced")]
    public static void PreviewAllSpaced()
    {
        // Each row on its own line, 2 unit spacing between tiles
        string path = "Assets/Farm Sprite.png";
        var sprites = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>()
            .OrderByDescending(s => s.rect.y).ThenBy(s => s.rect.x).ToArray();

        var old = GameObject.Find("_TilePreview");
        if (old != null) Object.DestroyImmediate(old);
        var root = new GameObject("_TilePreview").transform;
        var mat = FindUnlitMat();

        for (int r = 0; r < 16; r++)
        for (int c = 0; c < 16; c++)
        {
            int idx = r * 16 + c;
            if (idx >= sprites.Length) break;
            var go = new GameObject($"r{r:00}c{c:00}_i{idx:000}");
            go.transform.SetParent(root, false);
            go.transform.position = new Vector3(c * 1.5f, -r * 1.5f, 0f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprites[idx];
            sr.sortingOrder = 20;
            if (mat != null) sr.sharedMaterial = mat;
        }
        var sv = UnityEditor.SceneView.lastActiveSceneView;
        if (sv != null) { sv.pivot = new Vector3(11f, -11f, 0f); sv.size = 18f; sv.Repaint(); }
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("[Preview] All tiles at 1.5 unit spacing. Names: r{row}c{col}_i{idx}");
    }
}
