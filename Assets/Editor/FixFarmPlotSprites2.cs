using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class FixFarmPlotSprites2
{
    [MenuItem("Tools/Fix FarmPlot Sprites v2")]
    public static void Execute()
    {
        string dirtPath = "Assets/Resources/Tilled_Dirt.png";
        
        // Check importer settings
        var importer = AssetImporter.GetAtPath(dirtPath) as TextureImporter;
        if (importer != null)
        {
            Debug.Log($"PPU={importer.spritePixelsPerUnit}, spriteMode={importer.spriteImportMode}, spritePivot={importer.spritePivot}");
        }
        
        // Load ALL sub-sprites
        var allAssets = AssetDatabase.LoadAllAssetsAtPath(dirtPath);
        Sprite dirtSprite = null;
        foreach (var a in allAssets)
        {
            if (a is Sprite s)
            {
                if (s.name == "Tilled_Dirt_0")
                {
                    dirtSprite = s;
                    Debug.Log($"Found Tilled_Dirt_0: rect={s.rect}, bounds={s.bounds}, ppuX={s.pixelsPerUnit}");
                    break;
                }
            }
        }
        if (dirtSprite == null)
        {
            // Fallback: first sprite
            foreach (var a in allAssets)
            {
                if (a is Sprite s) { dirtSprite = s; Debug.Log($"Fallback sprite: {s.name}"); break; }
            }
        }

        // Find unlit material
        Material mat = null;
        var guids = AssetDatabase.FindAssets("t:Material Sprite-Unlit-Default");
        foreach (var g in guids)
        {
            var m = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(g));
            if (m != null) { mat = m; break; }
        }
        if (mat == null) mat = new Material(Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default"));
        Debug.Log($"Material: {mat?.name ?? "null"}");

        var plots = GameObject.FindObjectsByType<FarmPlot>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var plot in plots)
        {
            var sr = plot.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                SerializedObject so = new SerializedObject(sr);
                var spriteProp = so.FindProperty("m_Sprite");
                spriteProp.objectReferenceValue = dirtSprite;
                so.ApplyModifiedProperties();

                sr.color = Color.white;
                sr.sortingOrder = 5;
                sr.sharedMaterial = mat;
                EditorUtility.SetDirty(sr);
            }
            // Scale up so each tile is 2 unity units
            plot.transform.localScale = Vector3.one * 2.5f;
            EditorUtility.SetDirty(plot.gameObject);
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        Debug.Log($"Fixed {plots.Length} plots with sprite={dirtSprite?.name}, mat={mat?.name}");
    }
}
