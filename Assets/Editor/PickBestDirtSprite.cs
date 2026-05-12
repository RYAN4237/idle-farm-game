using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class PickBestDirtSprite
{
    [MenuItem("Tools/Pick Best Dirt Sprite")]
    public static void Execute()
    {
        string dirtPath = "Assets/Resources/Tilled_Dirt.png";
        var allAssets = AssetDatabase.LoadAllAssetsAtPath(dirtPath);
        
        // List first 20 sprites to understand naming
        int count = 0;
        foreach (var a in allAssets)
        {
            if (a is Sprite s && count < 20)
            {
                Debug.Log($"[{count}] {s.name} rect={s.rect}");
                count++;
            }
        }
        
        // The Tilled_Dirt spritesheet from Sprout Lands:
        // Row 0: tilled dirt tiles (darker brown)
        // We want one that looks like dark tilled soil
        // Try index around 5 or check which ones have darker pixels
        // For now let's use Tilled_Dirt_5 or find a soil tile
        Sprite darkDirt = null;
        foreach (var a in allAssets)
        {
            if (a is Sprite s)
            {
                // Look for variants with "Dirt" in the middle of the name
                // The darker tilled dirt is usually around index 6-14
                if (s.name == "Tilled_Dirt_6") { darkDirt = s; break; }
            }
        }
        if (darkDirt == null)
        {
            foreach (var a in allAssets)
                if (a is Sprite s) { darkDirt = s; break; } // fallback
        }
        
        var plots = GameObject.FindObjectsByType<FarmPlot>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var plot in plots)
        {
            var sr = plot.GetComponent<SpriteRenderer>();
            if (sr != null && darkDirt != null)
            {
                var so = new SerializedObject(sr);
                so.FindProperty("m_Sprite").objectReferenceValue = darkDirt;
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(sr);
            }
        }
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        Debug.Log($"Applied sprite: {darkDirt?.name} to {plots.Length} plots");
    }
}
