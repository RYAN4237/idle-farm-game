using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

/// Assigns a tilled-dirt sprite to all FarmPlot SpriteRenderers
/// and makes the FarmPlot colors use alpha=0 (sprite-based visuals only)
public class AssignFarmPlotSprites
{
    public static void Execute()
    {
        // Load tilled dirt sprite from Sprout Lands
        // "Tilled_Dirt_0" = first sprite = plain soil (row0 col0)
        string dirtPath = "Assets/Resources/Tilled_Dirt.png";
        var allSprites = AssetDatabase.LoadAllAssetsAtPath(dirtPath);
        
        Sprite dirtSprite = null;
        foreach (var a in allSprites)
        {
            if (a is Sprite s && s.name.EndsWith("_0"))
            {
                dirtSprite = s;
                break;
            }
        }
        
        if (dirtSprite == null)
        {
            // Try loading any sprite from the sheet
            foreach (var a in allSprites)
            {
                if (a is Sprite s) { dirtSprite = s; break; }
            }
        }
        
        Debug.Log($"[AssignFarmPlotSprites] dirtSprite={dirtSprite?.name ?? "NULL"} (total assets in sheet: {allSprites.Length})");
        if (dirtSprite == null) return;

        // Apply to all FarmPlot SpriteRenderers
        var container = GameObject.Find("FarmPlots");
        if (container == null) { Debug.LogError("FarmPlots not found"); return; }

        int count = 0;
        foreach (Transform t in container.transform)
        {
            var sr = t.GetComponent<SpriteRenderer>();
            if (sr == null) continue;
            
            sr.sprite       = dirtSprite;
            sr.color        = Color.white;
            sr.sortingOrder = 1;   // above BGReference (-100)
            
            // Also set unlit material
            var mat = AssetDatabase.LoadAssetAtPath<Material>(
                "Packages/com.unity.render-pipelines.universal/Runtime/Materials/Sprite-Unlit-Default.mat");
            if (mat != null) sr.sharedMaterial = mat;
            
            EditorUtility.SetDirty(t.gameObject);
            count++;
        }
        
        // Update FarmPlot colors to transparent (visual handled by sprite + plant)
        foreach (Transform t in container.transform)
        {
            var fp = t.GetComponent<FarmPlot>();
            if (fp == null) continue;
            fp.emptyColor   = Color.white;
            fp.growingColor = Color.white;
            fp.readyColor   = new Color(0.8f, 1f, 0.6f); // slight green tint when ready
            EditorUtility.SetDirty(fp);
        }
        
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log($"[AssignFarmPlotSprites] Applied dirt sprite to {count} FarmPlots.");
    }
}
