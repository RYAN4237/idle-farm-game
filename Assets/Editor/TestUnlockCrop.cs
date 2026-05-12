using UnityEngine;
using UnityEditor;

public class TestUnlockCrop
{
    public static void Execute()
    {
        var shop = CropShop.Instance;
        var rs   = ResourceSystem.Instance;
        if (shop == null || rs == null) { Debug.Log($"shop={shop!=null} rs={rs!=null}"); return; }

        Debug.Log($"FP={rs.FocusPoints}, Crops={shop.allCrops.Count}");
        for (int i = 0; i < shop.allCrops.Count; i++)
        {
            var c = shop.allCrops[i];
            Debug.Log($"  [{i}] {c.cropName}: unlocked={shop.IsCropUnlocked(i)}, cost={c.unlockCost}");
        }

        // Try to unlock Carrot (index 1)
        bool ok = shop.TryUnlockCrop(1);
        Debug.Log($"UnlockCarrot={ok}, FP after={rs.FocusPoints}, unlocked={shop.IsCropUnlocked(1)}");

        // Select Carrot
        if (ok) shop.SelectCrop(1);
        Debug.Log($"SelectedCrop={shop.SelectedCrop?.cropName}");
    }
}
