using UnityEngine;
using UnityEditor;

public class CreateCropAssets
{
    public static void Execute()
    {
        string folder = "Assets/Data/Crops";
        System.IO.Directory.CreateDirectory(folder.Replace("/", System.IO.Path.DirectorySeparatorChar.ToString()));
        AssetDatabase.Refresh();

        // ── Wheat: starter crop ──
        var wheat = ScriptableObject.CreateInstance<CropData>();
        wheat.cropName     = "Wheat";
        wheat.icon         = "W";
        wheat.plantCost    = 10f;
        wheat.harvestReward= 25f;
        wheat.growthTime   = 10f;
        wheat.emptyColor   = new Color(0.45f, 0.32f, 0.16f, 1f);
        wheat.growingColor = new Color(0.55f, 0.72f, 0.20f, 1f);
        wheat.readyColor   = new Color(0.95f, 0.85f, 0.10f, 1f); // golden
        wheat.unlockCost   = 0f;
        wheat.unlockOrder  = 0;
        AssetDatabase.CreateAsset(wheat, $"{folder}/Wheat.asset");

        // ── Carrot: mid-tier ──
        var carrot = ScriptableObject.CreateInstance<CropData>();
        carrot.cropName      = "Carrot";
        carrot.icon          = "C";
        carrot.plantCost     = 30f;
        carrot.harvestReward = 75f;
        carrot.growthTime    = 30f;
        carrot.emptyColor    = new Color(0.45f, 0.32f, 0.16f, 1f);
        carrot.growingColor  = new Color(0.20f, 0.58f, 0.20f, 1f);
        carrot.readyColor    = new Color(0.95f, 0.50f, 0.05f, 1f); // orange
        carrot.unlockCost    = 100f;
        carrot.unlockOrder   = 1;
        AssetDatabase.CreateAsset(carrot, $"{folder}/Carrot.asset");

        // ── Corn: high-tier ──
        var corn = ScriptableObject.CreateInstance<CropData>();
        corn.cropName      = "Corn";
        corn.icon          = "!";
        corn.plantCost     = 60f;
        corn.harvestReward = 160f;
        corn.growthTime    = 60f;
        corn.emptyColor    = new Color(0.45f, 0.32f, 0.16f, 1f);
        corn.growingColor  = new Color(0.20f, 0.65f, 0.25f, 1f);
        corn.readyColor    = new Color(0.98f, 0.92f, 0.10f, 1f); // bright yellow
        corn.unlockCost    = 300f;
        corn.unlockOrder   = 2;
        AssetDatabase.CreateAsset(corn, $"{folder}/Corn.asset");

        // ── Strawberry: premium ──
        var berry = ScriptableObject.CreateInstance<CropData>();
        berry.cropName      = "Strawberry";
        berry.icon          = "S";
        berry.plantCost     = 120f;
        berry.harvestReward = 350f;
        berry.growthTime    = 120f;
        berry.emptyColor    = new Color(0.45f, 0.32f, 0.16f, 1f);
        berry.growingColor  = new Color(0.20f, 0.55f, 0.20f, 1f);
        berry.readyColor    = new Color(0.95f, 0.15f, 0.15f, 1f); // red
        berry.unlockCost    = 800f;
        berry.unlockOrder   = 3;
        AssetDatabase.CreateAsset(berry, $"{folder}/Strawberry.asset");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // ── Add CropShop to GameManager ──
        var gm = GameObject.Find("GameManager");
        if (gm != null)
        {
            var shop = gm.GetComponent<CropShop>() ?? gm.AddComponent<CropShop>();
            shop.allCrops.Clear();
            shop.allCrops.Add(wheat);
            shop.allCrops.Add(carrot);
            shop.allCrops.Add(corn);
            shop.allCrops.Add(berry);
            shop.plotUnlockCosts = new float[] { 0f, 0f, 0f, 80f, 150f, 300f };
            EditorUtility.SetDirty(gm);
        }

        // ── Lock plots 4,5,6 ──
        for (int i = 4; i <= 6; i++)
        {
            var go   = GameObject.Find("FarmPlot_" + i);
            var plot = go?.GetComponent<FarmPlot>();
            if (plot == null) continue;
            plot.isLocked = true;
            var sr = go.GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = new Color(0.20f, 0.20f, 0.20f, 1f);
            EditorUtility.SetDirty(go);
        }

        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("CreateCropAssets complete! 4 crops created + CropShop wired + plots 4-6 locked.");
    }
}
