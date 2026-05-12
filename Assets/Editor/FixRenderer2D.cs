using UnityEditor;
using UnityEngine;

public class FixRenderer2D
{
    [MenuItem("Tools/Fix Renderer2D Unlit")]
    public static void Execute()
    {
        string path = "Assets/Settings/Renderer2D.asset";
        var renderer = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
        if (renderer == null)
        {
            Debug.LogError("Renderer2D.asset not found at " + path);
            return;
        }

        var so = new SerializedObject(renderer);
        var prop = so.FindProperty("m_DefaultMaterialType");
        if (prop != null)
        {
            Debug.Log($"Current m_DefaultMaterialType: {prop.intValue}");
            prop.intValue = 1; // 1 = Unlit
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(renderer);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Renderer2D set to Unlit (m_DefaultMaterialType=1)");
        }
        else
        {
            Debug.LogWarning("m_DefaultMaterialType property not found, listing all properties:");
            var iter = so.GetIterator();
            iter.Next(true);
            int count = 0;
            while (iter.Next(false) && count < 30)
            {
                Debug.Log($"  Property: {iter.propertyPath} = {iter.intValue}");
                count++;
            }
        }
    }
}
