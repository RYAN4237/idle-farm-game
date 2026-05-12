using UnityEngine;
using UnityEditor;

public static class TestUnlockTree
{
    [MenuItem("Tools/Test Unlock Tree")]
    public static void Execute()
    {
        var tree = UnlockTree.Instance;
        var rs = ResourceSystem.Instance;

        if (tree == null) { Debug.LogError("[Test] UnlockTree.Instance is null"); return; }
        if (rs == null) { Debug.LogError("[Test] ResourceSystem.Instance is null"); return; }

        Debug.Log($"[Test] Balance: {rs.FocusPoints}, Multiplier: {rs.GlobalMultiplier}");
        Debug.Log($"[Test] Nodes loaded: {tree.GetAllNodes().Count}");

        // Test node states
        var state1 = tree.ComputeNodeState("farm_slot_2");
        var state2 = tree.ComputeNodeState("farm_slot_3");
        Debug.Log($"[Test] farm_slot_2 state: {state1} (expect Available if balance >= 1400)");
        Debug.Log($"[Test] farm_slot_3 state: {state2} (expect Locked — prereq not met)");

        // Test unlock
        float before = rs.FocusPoints;
        bool success = tree.TryUnlockNode("farm_slot_2");
        Debug.Log($"[Test] TryUnlock farm_slot_2: {success}, balance {before} -> {rs.FocusPoints}");
        Debug.Log($"[Test] GlobalMultiplier after: {rs.GlobalMultiplier}");

        // After unlocking farm_slot_2, farm_slot_3 should become available
        var state3 = tree.ComputeNodeState("farm_slot_3");
        Debug.Log($"[Test] farm_slot_3 state after farm_slot_2 unlock: {state3}");

        // Test prerequisite gating
        bool badUnlock = tree.TryUnlockNode("multiplier_farm_1");
        Debug.Log($"[Test] TryUnlock multiplier_farm_1 (prereq farm_slot_3 not met): {badUnlock} (expect false)");

        // Unlock farm_slot_3
        bool success2 = tree.TryUnlockNode("farm_slot_3");
        Debug.Log($"[Test] TryUnlock farm_slot_3: {success2}, balance: {rs.FocusPoints}, mult: {rs.GlobalMultiplier}");

        // Verify idempotent
        bool dupe = tree.TryUnlockNode("farm_slot_2");
        Debug.Log($"[Test] Double unlock farm_slot_2: {dupe} (expect false)");

        Debug.Log("[Test] === ALL TESTS COMPLETE ===");
    }
}
