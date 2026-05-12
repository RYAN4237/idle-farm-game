using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class FixGameSystems
{
    [MenuItem("Tools/Fix Game Systems")]
    public static void Execute()
    {
        // 1. Add AutoFarmer to GameManager if missing
        var gm = GameObject.Find("GameManager");
        if (gm == null) { Debug.LogError("GameManager not found"); return; }

        if (gm.GetComponent<AutoFarmer>() == null)
        {
            gm.AddComponent<AutoFarmer>();
            Debug.Log("Added AutoFarmer to GameManager");
        }
        else Debug.Log("AutoFarmer already on GameManager");

        // 2. Move CropShop from SeedsPanel to GameManager
        // Find SeedsPanel CropShop
        var seedsPanel = GameObject.Find("SeedsPanel");
        CropShop existingShop = null;
        if (seedsPanel != null)
            existingShop = seedsPanel.GetComponent<CropShop>();

        var gmShop = gm.GetComponent<CropShop>();
        if (gmShop == null)
        {
            // Copy data from SeedsPanel if available
            var newShop = gm.AddComponent<CropShop>();
            if (existingShop != null)
            {
                newShop.allCrops = new System.Collections.Generic.List<CropData>(existingShop.allCrops);
                newShop.plotUnlockCosts = existingShop.plotUnlockCosts;
                Debug.Log($"Added CropShop to GameManager with {newShop.allCrops.Count} crops");

                // Remove from SeedsPanel to avoid duplicate singleton
                // (Keep it there if CropShopUIController needs it via transform.parent)
                // Actually leave it — CropShop.Awake handles duplicate via Destroy
            }
            else
            {
                // Create default crops
                Debug.Log("Added CropShop to GameManager (no data to copy)");
            }
        }
        else Debug.Log("CropShop already on GameManager");

        // 3. Fix Farmer character walk sprites
        var farmer = GameObject.Find("Farmer");
        if (farmer != null)
        {
            var fc = farmer.GetComponent<FarmerCharacter>();
            if (fc != null)
            {
                string sheetPath = "Assets/Sprout Lands - Sprites - Basic pack/Characters/Basic Charakter Spritesheet.png";
                var allSprites = AssetDatabase.LoadAllAssetsAtPath(sheetPath);
                
                // Log what sprites are available
                int count = 0;
                foreach (var a in allSprites)
                {
                    if (a is Sprite s && count < 20)
                    {
                        Debug.Log($"CharSprite[{count}]: {s.name} rect={s.rect}");
                        count++;
                    }
                }
            }
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        Debug.Log("Fix complete.");
    }
}
