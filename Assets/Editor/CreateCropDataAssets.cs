using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class CreateCropDataAssets
{
    [MenuItem("Tools/Create Crop Data Assets")]
    public static void Execute()
    {
        string folder = "Assets/Data/Crops";
        if (!AssetDatabase.IsValidFolder("Assets/Data"))
            AssetDatabase.CreateFolder("Assets", "Data");
        if (!AssetDatabase.IsValidFolder(folder))
            AssetDatabase.CreateFolder("Assets/Data", "Crops");

        // Define crops
        var cropDefs = new[]
        {
            new { name="Wheat",   icon="🌾", plant=10f, harvest=30f, growth=20f, unlock=0f,   order=0, ec=new Color(0.55f,0.42f,0.20f,1f), gc=new Color(0.40f,0.65f,0.20f,1f), rc=new Color(0.95f,0.85f,0.20f,1f) },
            new { name="Carrot",  icon="🥕", plant=20f, harvest=55f, growth=30f, unlock=50f,  order=1, ec=new Color(0.55f,0.42f,0.20f,1f), gc=new Color(0.30f,0.70f,0.25f,1f), rc=new Color(0.95f,0.55f,0.10f,1f) },
            new { name="Tomato",  icon="🍅", plant=35f, harvest=90f, growth=45f, unlock=120f, order=2, ec=new Color(0.55f,0.42f,0.20f,1f), gc=new Color(0.20f,0.60f,0.20f,1f), rc=new Color(0.90f,0.20f,0.15f,1f) },
            new { name="Pumpkin", icon="🎃", plant=60f, harvest=150f,growth=60f, unlock=250f, order=3, ec=new Color(0.55f,0.42f,0.20f,1f), gc=new Color(0.25f,0.55f,0.20f,1f), rc=new Color(0.95f,0.55f,0.10f,1f) },
        };

        var createdCrops = new System.Collections.Generic.List<CropData>();

        foreach (var def in cropDefs)
        {
            string path = $"{folder}/{def.name}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<CropData>(path);
            if (existing != null)
            {
                Debug.Log($"Crop {def.name} already exists");
                createdCrops.Add(existing);
                continue;
            }

            var crop = ScriptableObject.CreateInstance<CropData>();
            crop.cropName     = def.name;
            crop.icon         = def.icon;
            crop.plantCost    = def.plant;
            crop.harvestReward= def.harvest;
            crop.growthTime   = def.growth;
            crop.unlockCost   = def.unlock;
            crop.unlockOrder  = def.order;
            crop.emptyColor   = def.ec;
            crop.growingColor = def.gc;
            crop.readyColor   = def.rc;

            AssetDatabase.CreateAsset(crop, path);
            createdCrops.Add(crop);
            Debug.Log($"Created CropData: {def.name}");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // Assign to CropShop on GameManager
        var gm = GameObject.Find("GameManager");
        if (gm == null) { Debug.LogError("GameManager not found"); return; }
        var shop = gm.GetComponent<CropShop>();
        if (shop == null) { Debug.LogError("CropShop not on GameManager"); return; }

        var so = new SerializedObject(shop);
        var cropsProp = so.FindProperty("allCrops");
        cropsProp.arraySize = createdCrops.Count;
        for (int i = 0; i < createdCrops.Count; i++)
            cropsProp.GetArrayElementAtIndex(i).objectReferenceValue = createdCrops[i];
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(shop);

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        Debug.Log($"Assigned {createdCrops.Count} crops to CropShop on GameManager");
    }
}
