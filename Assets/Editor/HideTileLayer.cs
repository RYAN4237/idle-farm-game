using UnityEngine;
using UnityEditor;

public class HideTileLayer
{
    [MenuItem("Farm/Hide Tile Layer")]
    public static void Execute()
    {
        var tilemap = GameObject.Find("Tilemap");
        if (tilemap != null) tilemap.SetActive(false);

        var deco = GameObject.Find("Decorations");
        if (deco != null) deco.SetActive(false);

        var skyBG = GameObject.Find("SkyBG");
        if (skyBG != null) skyBG.SetActive(false);

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("[HideTileLayer] Done.");
    }
}
