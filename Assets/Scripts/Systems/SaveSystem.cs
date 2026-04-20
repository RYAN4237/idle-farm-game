using UnityEngine;
using System.Collections.Generic;

public class SaveSystem : MonoBehaviour
{
    public static SaveSystem Instance { get; private set; }

    const string KEY_FP             = "save_fp";
    const string KEY_SESSIONS       = "save_sessions";
    const string KEY_UNLOCKED_CROPS = "save_unlocked_crops";
    const string KEY_SELECTED_CROP  = "save_selected_crop";
    const string KEY_AUTOFARMER_LVL = "save_autofarmer_level";

    float autoSaveTimer = 30f;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()                           => LoadGame();
    void OnApplicationQuit()              => SaveGame();
    void OnApplicationPause(bool pause)   { if (pause) SaveGame(); }

    void Update()
    {
        autoSaveTimer -= Time.deltaTime;
        if (autoSaveTimer <= 0f) { SaveGame(); autoSaveTimer = 30f; }
    }

    // ── SAVE ──────────────────────────────────────────────────────────
    public void SaveGame()
    {
        var rs = ResourceSystem.Instance;
        if (rs != null)
        {
            PlayerPrefs.SetFloat(KEY_FP,      rs.FocusPoints);
            PlayerPrefs.SetInt(KEY_SESSIONS,  rs.TotalSessionsCompleted);
        }

        var plots = FindObjectsByType<FarmPlot>(FindObjectsSortMode.None);
        foreach (var plot in plots)
        {
            string k = "plot_" + plot.gameObject.name;
            PlayerPrefs.SetInt   (k + "_state",  (int)plot.State);
            PlayerPrefs.SetInt   (k + "_locked", plot.isLocked ? 1 : 0);
            if (plot.State == FarmPlot.PlotState.Growing)
            {
                // GetActiveCrop() returns string directly
                PlayerPrefs.SetString(k + "_crop",  plot.GetActiveCrop() ?? "");
                PlayerPrefs.SetFloat (k + "_timer", plot.GetGrowTimerRemaining());
            }
            else
            {
                PlayerPrefs.SetString(k + "_crop", "");
                PlayerPrefs.SetFloat (k + "_timer", 0f);
            }
        }

        var shop = CropShop.Instance;
        if (shop != null)
        {
            var names = new List<string>();
            for (int i = 0; i < shop.allCrops.Count; i++)
                if (shop.IsCropUnlocked(i)) names.Add(shop.allCrops[i].cropName);
            PlayerPrefs.SetString(KEY_UNLOCKED_CROPS, string.Join(",", names));
            PlayerPrefs.SetInt   (KEY_SELECTED_CROP,
                Mathf.Max(shop.allCrops.IndexOf(shop.SelectedCrop), 0));
        }

        if (AutoFarmer.Instance != null)
            PlayerPrefs.SetInt(KEY_AUTOFARMER_LVL, AutoFarmer.Instance.CurrentLevel);

        PlayerPrefs.Save();
        Debug.Log($"[Save] Saved. FP={rs?.FocusPoints:F0}");
    }

    // ── LOAD ──────────────────────────────────────────────────────────
    public void LoadGame()
    {
        if (!PlayerPrefs.HasKey(KEY_FP)) { Debug.Log("[Save] No save, starting fresh."); return; }

        var rs = ResourceSystem.Instance;
        if (rs != null)
        {
            rs.SetFocusPoints(PlayerPrefs.GetFloat(KEY_FP, 0f));
            rs.SetSessions   (PlayerPrefs.GetInt(KEY_SESSIONS, 0));
        }

        var shop = CropShop.Instance;
        if (shop != null)
        {
            string saved = PlayerPrefs.GetString(KEY_UNLOCKED_CROPS, "");
            if (!string.IsNullOrEmpty(saved))
                foreach (var n in saved.Split(','))
                    for (int i = 0; i < shop.allCrops.Count; i++)
                        if (shop.allCrops[i].cropName == n) shop.ForceUnlock(i);
            shop.SelectCrop(PlayerPrefs.GetInt(KEY_SELECTED_CROP, 0));
        }

        var plots = FindObjectsByType<FarmPlot>(FindObjectsSortMode.None);
        foreach (var plot in plots)
        {
            string k = "plot_" + plot.gameObject.name;
            if (!PlayerPrefs.HasKey(k + "_state")) continue;

            plot.isLocked = PlayerPrefs.GetInt(k + "_locked", 0) == 1;
            var    state  = (FarmPlot.PlotState)PlayerPrefs.GetInt(k + "_state", 1);
            string cn     = PlayerPrefs.GetString(k + "_crop", "");
            float  timer  = PlayerPrefs.GetFloat (k + "_timer", 0f);

            if (state == FarmPlot.PlotState.Growing && !string.IsNullOrEmpty(cn) && shop != null)
            {
                var crop = shop.allCrops.Find(c => c.cropName == cn);
                // RestoreGrowingState takes a string crop name
                if (crop != null) { plot.RestoreGrowingState(crop.cropName, timer); continue; }
            }
            plot.SetState(state);
        }

        int afLevel = PlayerPrefs.GetInt(KEY_AUTOFARMER_LVL, 0);
        if (afLevel > 0) AutoFarmer.Instance?.RestoreLevel(afLevel);

        Debug.Log("[Save] Loaded.");
    }

    public void DeleteSave() { PlayerPrefs.DeleteAll(); Debug.Log("[Save] Deleted."); }
}
