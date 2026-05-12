using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class ToggleUI
{
    [MenuItem("Tools/Hide UI Canvas")]
    public static void Execute()
    {
        var ui = GameObject.Find("UICanvas");
        if (ui != null)
        {
            ui.SetActive(false);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("[ToggleUI] UICanvas hidden");
        }
        else Debug.LogWarning("[ToggleUI] UICanvas not found");
    }
}
