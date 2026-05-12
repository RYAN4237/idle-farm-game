using UnityEngine;
using UnityEditor;

public class CleanupPreviews
{
    [MenuItem("Tools/Cleanup Preview Objects")]
    public static void Execute()
    {
        foreach (var n in new[] { "_SpritePreview", "_SpritePreview2", "_SPSpecific" })
        {
            var o = GameObject.Find(n);
            if (o != null) { Object.DestroyImmediate(o); Debug.Log($"Removed {n}"); }
        }
    }
}
