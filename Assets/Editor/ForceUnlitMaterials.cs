using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class ForceUnlitMaterials
{
    [MenuItem("Tools/Force Unlit Materials on All Renderers")]
    public static void Execute()
    {
        // Find URP's Sprite-Unlit-Default material
        Material unlitMat = null;

        // Search in URP package
        var guids = AssetDatabase.FindAssets("Sprite-Unlit-Default t:Material");
        foreach (var g in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(g);
            if (path.Contains("Universal") || path.Contains("URP") || path.Contains("com.unity.render-pipelines"))
            {
                unlitMat = AssetDatabase.LoadAssetAtPath<Material>(path);
                Debug.Log($"[ForceUnlit] Found URP unlit mat at: {path}");
                break;
            }
        }

        if (unlitMat == null && guids.Length > 0)
        {
            var path = AssetDatabase.GUIDToAssetPath(guids[0]);
            unlitMat = AssetDatabase.LoadAssetAtPath<Material>(path);
            Debug.Log($"[ForceUnlit] Using first match: {path}");
        }

        if (unlitMat == null)
        {
            Debug.LogError("[ForceUnlit] Could not find Sprite-Unlit-Default material. Listing all found:");
            foreach (var g in AssetDatabase.FindAssets("Sprite t:Material"))
                Debug.Log("  " + AssetDatabase.GUIDToAssetPath(g));
            return;
        }

        int count = 0;

        var srs = Object.FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var sr in srs)
        {
            if (sr.GetComponentInParent<Canvas>() != null) continue;
            sr.sharedMaterial = unlitMat;
            EditorUtility.SetDirty(sr);
            count++;
        }

        var trs = Object.FindObjectsByType<TilemapRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var tr in trs)
        {
            tr.sharedMaterial = unlitMat;
            EditorUtility.SetDirty(tr);
            count++;
        }

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log($"[ForceUnlit] Applied '{unlitMat.name}' to {count} renderers.");
    }
}
