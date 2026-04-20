using UnityEngine;
using UnityEditor;

public class WireSaveSystem
{
    public static void Execute()
    {
        var gm = GameObject.Find("GameManager");
        if (gm == null) { Debug.LogError("GameManager not found"); return; }

        if (gm.GetComponent<SaveSystem>() == null)
        {
            gm.AddComponent<SaveSystem>();
            Debug.Log("SaveSystem added to GameManager.");
        }

        EditorUtility.SetDirty(gm);
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("WireSaveSystem complete + saved!");
    }
}
