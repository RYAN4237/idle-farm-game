using UnityEngine;
using UnityEditor;

public class SetupGrassTexture
{
    public static void Execute()
    {
        // 1. 给Main Camera上的FarmMapScroller设置草地贴图
        var cam = GameObject.Find("Main Camera");
        if (cam == null) { Debug.LogError("Main Camera not found!"); return; }

        var scroller = cam.GetComponent<FarmMapScroller>();
        if (scroller == null)
        {
            scroller = cam.AddComponent<FarmMapScroller>();
            Debug.Log("FarmMapScroller added to Main Camera.");
        }

        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Textures/GrassTile.png");
        if (tex != null)
        {
            scroller.grassTexture = tex;
            Debug.Log("[SetupGrassTexture] GrassTile.png assigned to FarmMapScroller!");
        }
        else
        {
            Debug.LogError("[SetupGrassTexture] GrassTile.png not found!");
        }

        // 2. 删除场景里的旧GrassBG（会在运行时重建）
        var oldBG = GameObject.Find("GrassBG");
        if (oldBG != null) Object.DestroyImmediate(oldBG);
        var oldBG2 = GameObject.Find("__GrassBG__");
        if (oldBG2 != null) Object.DestroyImmediate(oldBG2);

        // 3. 设置scroll参数
        scroller.mapMinX       = -20f;
        scroller.mapMaxX       =  20f;
        scroller.scrollSpeed   = 10f;
        scroller.snapSmoothing = 8f;
        scroller.bgHeight      = 8f;
        scroller.bgWidth       = 200f;
        scroller.tilingX       = 40f;

        EditorUtility.SetDirty(cam);

        // 4. 保存场景
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log("[SetupGrassTexture] Done! Play the game to see grass background.");
    }
}
