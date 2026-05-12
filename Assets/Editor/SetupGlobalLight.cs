using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering.Universal;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class SetupGlobalLight
{
    [MenuItem("Tools/Setup Global Light 2D")]
    public static void Execute()
    {
        // Remove existing GlobalLight2D if any
        var old = GameObject.Find("GlobalLight2D");
        if (old != null) Object.DestroyImmediate(old);

        var go = new GameObject("GlobalLight2D");
        var light = go.AddComponent<Light2D>();

        // Global light type = 3
        light.lightType = Light2D.LightType.Global;
        light.intensity = 1f;
        light.color = Color.white;

        EditorUtility.SetDirty(go);
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log("[SetupGlobalLight] Global Light 2D added.");
    }
}
