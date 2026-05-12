using UnityEngine;
using UnityEditor;

public class AssignFarmSprites
{
    [MenuItem("Farm/Assign Sprites to Layers")]
    public static void Execute()
    {
        // Reference layer - center and scale to fill camera (ortho size 5 = 10 units tall)
        Assign("Reference_Layer", "Assets/Sprites/Farm_Reference.png", null);
        var refGO = GameObject.Find("Reference_Layer");
        if (refGO != null)
        {
            var sr = refGO.GetComponent<SpriteRenderer>();
            if (sr?.sprite != null)
            {
                float scale = 10f / sr.sprite.bounds.size.y;
                refGO.transform.localScale = new Vector3(scale, scale, 1);
                refGO.transform.position = Vector3.zero;
            }
        }

        // Ground - use grass tile as base BG_Ground
        AssignSliced("Background/BG_Ground",        "Assets/Sprites/Farm_Ground.png", "ground_grass");
        AssignSliced("Background/BG_River",         "Assets/Sprites/Farm_Ground.png", "ground_water");

        // Trees
        AssignSliced("Background/BG_Treeline_Back", "Assets/Sprites/Farm_Trees.png",  "tree_0");
        AssignSliced("Background/BG_Trees_Front",   "Assets/Sprites/Farm_Trees.png",  "tree_1");

        // Deco
        AssignSliced("Background/BG_Bridge",        "Assets/Sprites/Farm_Deco.png",   "deco_bridge");
        AssignSliced("Background/BG_Props",         "Assets/Sprites/Farm_Deco.png",   "deco_rock_large");
        AssignSliced("Background/BG_Crops",         "Assets/Sprites/Farm_Deco.png",   "deco_wheat_2");

        // Plants
        AssignSliced("Background/BG_Plants_Front",  "Assets/Sprites/Farm_Plants.png", "plant_cattail");

        Debug.Log("Sprites assigned. Fine-tune positions in Scene view using Reference_Layer as guide.");
    }

    static void Assign(string goPath, string assetPath, string spriteName)
    {
        var go = GameObject.Find(goPath);
        if (go == null) { Debug.LogWarning("GameObject not found: " + goPath); return; }
        var sr = go.GetComponent<SpriteRenderer>();
        if (sr == null) sr = go.AddComponent<SpriteRenderer>();
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        if (sprite != null) sr.sprite = sprite;
    }

    static void AssignSliced(string goPath, string assetPath, string spriteName)
    {
        var go = GameObject.Find(goPath);
        if (go == null) { Debug.LogWarning("GameObject not found: " + goPath); return; }
        var sr = go.GetComponent<SpriteRenderer>();
        if (sr == null) sr = go.AddComponent<SpriteRenderer>();

        var all = AssetDatabase.LoadAllAssetsAtPath(assetPath);
        foreach (var obj in all)
        {
            if (obj is Sprite s && s.name == spriteName)
            {
                sr.sprite = s;
                Debug.Log($"Assigned {spriteName} -> {goPath}");
                return;
            }
        }
        Debug.LogWarning($"Sprite not found: {spriteName} in {assetPath}");
    }
}
