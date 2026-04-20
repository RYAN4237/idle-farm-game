using UnityEngine;
using UnityEditor;

public class TestFocusButton
{
    public static void Execute()
    {
        var fs = FocusSystem.Instance;
        if (fs == null) { Debug.LogError("FocusSystem.Instance is null"); return; }

        Debug.Log($"FocusSystem: IsRunning={fs.IsRunning}, IsResting={fs.IsResting}");

        // Toggle timer (same as clicking Start Focus)
        fs.ToggleTimer();

        Debug.Log($"After ToggleTimer: IsRunning={fs.IsRunning}");

        // Check UIManager
        var ui = UnityEngine.Object.FindFirstObjectByType<UIManager>();
        if (ui != null)
        {
            Debug.Log($"UIManager: startPauseButton={ui.startPauseButton?.name}");
            Debug.Log($"UIManager: startPauseButtonText={ui.startPauseButtonText?.text}");
        }

        // Check AutoFarmer
        var af = AutoFarmer.Instance;
        Debug.Log($"AutoFarmer: Level={af?.CurrentLevel}, CanUpgrade={af?.CanUpgrade()}, Cost={af?.UpgradeCost()}");
    }
}
