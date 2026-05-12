using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class FixFarmPlotColors
{
    [MenuItem("Tools/Fix FarmPlot Colors and Sprites")]
    public static void Execute()
    {
        var plots = GameObject.FindObjectsByType<FarmPlot>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        Debug.Log($"Found {plots.Length} FarmPlot(s)");

        string dirtPath = "Assets/Resources/Tilled_Dirt.png";
        var allAssets = AssetDatabase.LoadAllAssetsAtPath(dirtPath);
        Sprite dirtSprite = null;
        int spriteCount = 0;
        foreach (var a in allAssets)
        {
            if (a is Sprite s)
            {
                spriteCount++;
                if (spriteCount <= 5) Debug.Log($"  Sprite: {s.name}");
                if (dirtSprite == null) dirtSprite = s;
                if (s.name == "Tilled_Dirt_0") dirtSprite = s;
            }
        }
        Debug.Log($"Total sprites in Tilled_Dirt.png: {spriteCount}, using: {dirtSprite?.name}");

        // Try to find an unlit material
        Material mat = null;
        foreach (var guid in AssetDatabase.FindAssets("Sprite-Unlit-Default t:Material"))
        {
            mat = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid));
            if (mat != null) { Debug.Log("Mat: " + mat.name); break; }
        }
        if (mat == null)
            mat = new Material(Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default"));

        foreach (var plot in plots)
        {
            plot.readyColor   = Color.white;
            plot.growingColor = Color.white;
            plot.emptyColor   = Color.white;
            EditorUtility.SetDirty(plot);

            var sr = plot.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.color = Color.white;
                if (dirtSprite != null) sr.sprite = dirtSprite;
                sr.sortingOrder = 1;
                if (mat != null) sr.sharedMaterial = mat;
                EditorUtility.SetDirty(sr);
            }
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        Debug.Log($"Fixed {plots.Length} plots.");
    }
}
