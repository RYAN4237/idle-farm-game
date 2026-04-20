using UnityEngine;
using UnityEditor;

public class TestFocusReward
{
    public static void Execute()
    {
        // 模拟专注完成 → 检查EventBus是否触发
        Debug.Log("[Test] Simulating focus session complete...");
        GameEventBus.PublishFocusComplete(1);
        GameEventBus.PublishBoost(2f, 10f); // x2 boost for 10 seconds

        // 检查FarmBoostReceiver
        var fbr = Object.FindObjectOfType<FarmBoostReceiver>();
        if (fbr != null)
            Debug.Log($"[Test] FarmBoostReceiver: multiplier={fbr.GrowthMultiplier}");
        else
            Debug.LogWarning("[Test] FarmBoostReceiver not found (only available at runtime)");
    }
}
