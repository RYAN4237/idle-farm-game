using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class ActivateCanvas
{
    [MenuItem("Tools/Activate Canvas")]
    public static void Execute()
    {
        var canvas = GameObject.Find("UICanvas");
        if (canvas == null)
        {
            // Try finding inactive
            var all = Resources.FindObjectsOfTypeAll<Canvas>();
            foreach (var c in all)
            {
                if (c.name == "UICanvas")
                {
                    canvas = c.gameObject;
                    break;
                }
            }
        }
        if (canvas == null) { Debug.LogError("UICanvas not found"); return; }
        canvas.SetActive(true);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        Debug.Log("UICanvas activated: " + canvas.activeSelf);
    }
}
