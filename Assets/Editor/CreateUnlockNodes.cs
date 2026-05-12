using UnityEngine;
using UnityEditor;

public static class CreateUnlockNodes
{
    [MenuItem("Tools/Create Unlock Node Assets")]
    public static void Execute()
    {
        string folder = "Assets/Data/UnlockTree";
        if (!AssetDatabase.IsValidFolder(folder))
        {
            AssetDatabase.CreateFolder("Assets/Data", "UnlockTree");
        }

        CreateNode(folder, "farm_slot_2",        "Farm Slot 2",     1400f,   0.20f, new string[0]);
        CreateNode(folder, "farm_slot_3",        "Farm Slot 3",     3200f,   0.20f, new[] { "farm_slot_2" });
        CreateNode(folder, "multiplier_farm_1",  "Farm Multiplier I", 7500f, 0.25f, new[] { "farm_slot_3" });
        CreateNode(folder, "fish_species_2",     "Fish Species 2",  17000f,  0.25f, new string[0]);
        CreateNode(folder, "fish_species_3",     "Fish Species 3",  36000f,  0.30f, new[] { "fish_species_2" });
        CreateNode(folder, "multiplier_fish_1",  "Fish Multiplier I", 110000f, 0.35f, new[] { "fish_species_3" });
        CreateNode(folder, "farm_slot_4",        "Farm Slot 4",     68000f,  0.30f, new[] { "farm_slot_3", "multiplier_farm_1" });
        CreateNode(folder, "multiplier_farm_2",  "Farm Multiplier II", 160000f, 0.35f, new[] { "multiplier_farm_1", "multiplier_fish_1" });

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[UnlockTree] Created 8 MVP node assets in " + folder);
    }

    static void CreateNode(string folder, string nodeId, string displayName, float cost, float mult, string[] prereqs)
    {
        var node = ScriptableObject.CreateInstance<UnlockNodeData>();
        node.NodeId = nodeId;
        node.DisplayName = displayName;
        node.PointCost = cost;
        node.MultiplierGranted = mult;
        node.PrerequisiteNodeIds = prereqs;
        AssetDatabase.CreateAsset(node, $"{folder}/{nodeId}.asset");
    }
}
