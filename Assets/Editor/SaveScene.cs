using UnityEditor;
using UnityEngine.SceneManagement;

public class SaveScene
{
    public static void Execute()
    {
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(
            SceneManager.GetActiveScene());
        UnityEngine.Debug.Log("Scene saved.");
    }
}
