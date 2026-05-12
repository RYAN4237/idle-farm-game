using UnityEngine;
using UnityEditor;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class FixEventSystem
{
    [MenuItem("Tools/Fix EventSystem Input Module")]
    public static void Execute()
    {
        var es = Object.FindAnyObjectByType<EventSystem>();
        if (es == null) { Debug.LogError("[FixEventSystem] No EventSystem found"); return; }

        var old = es.GetComponent<StandaloneInputModule>();
        if (old != null)
        {
            Object.DestroyImmediate(old);
            Debug.Log("[FixEventSystem] Removed StandaloneInputModule");
        }

        if (es.GetComponent<InputSystemUIInputModule>() == null)
        {
            es.gameObject.AddComponent<InputSystemUIInputModule>();
            Debug.Log("[FixEventSystem] Added InputSystemUIInputModule");
        }

        EditorUtility.SetDirty(es.gameObject);
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log("[FixEventSystem] Done.");
    }
}
