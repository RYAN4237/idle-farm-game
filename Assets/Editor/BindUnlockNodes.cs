using UnityEngine;
using UnityEditor;
using System.Linq;

public static class BindUnlockNodes
{
    [MenuItem("Tools/Bind Unlock Nodes to Scene")]
    public static void Execute()
    {
        var tree = Object.FindAnyObjectByType<UnlockTree>();
        if (tree == null) { Debug.LogError("No UnlockTree in scene"); return; }

        string folder = "Assets/Data/UnlockTree";
        var guids = AssetDatabase.FindAssets("t:UnlockNodeData", new[] { folder });
        var nodes = guids
            .Select(g => AssetDatabase.LoadAssetAtPath<UnlockNodeData>(AssetDatabase.GUIDToAssetPath(g)))
            .Where(n => n != null)
            .OrderBy(n => n.PointCost)
            .ToArray();

        var so = new SerializedObject(tree);
        var prop = so.FindProperty("_nodes");
        prop.arraySize = nodes.Length;
        for (int i = 0; i < nodes.Length; i++)
            prop.GetArrayElementAtIndex(i).objectReferenceValue = nodes[i];
        so.ApplyModifiedProperties();

        EditorUtility.SetDirty(tree);
        Debug.Log($"[BindUnlockNodes] Assigned {nodes.Length} nodes to UnlockTree.");
    }
}
