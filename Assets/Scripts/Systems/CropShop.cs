using UnityEngine;
using System.Collections.Generic;

public class CropShop : MonoBehaviour
{
    public static CropShop Instance { get; private set; }

    [Header("All Crop Types")]
    public List<CropData> allCrops = new List<CropData>();

    [Header("Plot Unlock Costs")]
    public float[] plotUnlockCosts = { 0f, 0f, 0f, 80f, 150f, 300f };

    public CropData SelectedCrop { get; private set; }

    private HashSet<string> unlockedCropNames = new HashSet<string>();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        // First crop always free
        if (allCrops.Count > 0)
        {
            SelectedCrop = allCrops[0];
            unlockedCropNames.Add(allCrops[0].cropName);
        }
    }

    public float UnlockCost(FarmPlot plot)
    {
        if (int.TryParse(plot.gameObject.name.Replace("FarmPlot_", ""), out int idx))
        {
            int i = idx - 1;
            if (i >= 0 && i < plotUnlockCosts.Length) return plotUnlockCosts[i];
        }
        return 80f;
    }

    public void SelectCrop(int index)
    {
        if (index < 0 || index >= allCrops.Count) return;
        if (!unlockedCropNames.Contains(allCrops[index].cropName)) return;
        SelectedCrop = allCrops[index];
        RefreshPlotLabels();
    }

    public bool TryUnlockCrop(int index)
    {
        if (index < 0 || index >= allCrops.Count) return false;
        var crop = allCrops[index];
        if (unlockedCropNames.Contains(crop.cropName)) return true;
        if (!ResourceSystem.Instance.SpendFocusPoints(crop.unlockCost)) return false;
        unlockedCropNames.Add(crop.cropName);
        Debug.Log($"CropShop: Unlocked {crop.cropName}!");
        return true;
    }

    // Called by SaveSystem on load — no FP cost
    public void ForceUnlock(int index)
    {
        if (index < 0 || index >= allCrops.Count) return;
        unlockedCropNames.Add(allCrops[index].cropName);
    }

    public bool IsCropUnlocked(int index)
    {
        if (index < 0 || index >= allCrops.Count) return false;
        return unlockedCropNames.Contains(allCrops[index].cropName);
    }

    void RefreshPlotLabels()
    {
        var plots = FindObjectsByType<FarmPlot>(FindObjectsSortMode.None);
        foreach (var p in plots)
            if (p.State == FarmPlot.PlotState.Empty) p.SetState(FarmPlot.PlotState.Empty);
    }
}
