using UnityEngine;
using UnityEditor;

public class TestUpgradeAutoFarmer
{
    public static void Execute()
    {
        var af = AutoFarmer.Instance;
        var rs = ResourceSystem.Instance;
        if (af == null) { Debug.Log("AutoFarmer.Instance is null"); return; }
        if (rs == null) { Debug.Log("ResourceSystem.Instance is null"); return; }

        float fpBefore = rs.FocusPoints;
        Debug.Log($"FP before={fpBefore}, Level={af.CurrentLevel}, Cost={af.UpgradeCost()}");

        // Directly spend FP and upgrade
        bool ok = af.TryUpgrade();
        Debug.Log($"TryUpgrade={ok}, Level after={af.CurrentLevel}, FP after={rs.FocusPoints}");
    }
}
