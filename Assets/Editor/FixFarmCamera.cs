using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;

public class FixFarmCamera
{
    public static void Execute()
    {
        var cam = GameObject.Find("Main Camera");
        if (cam == null) { Debug.LogError("Main Camera not found"); return; }

        var camera = cam.GetComponent<Camera>();
        if (camera == null) { Debug.LogError("Camera component not found"); return; }

        // 游戏窗口是横幅式（宽>>高）
        // 农田格子 cellSize=1, 4行 → 高度=4单位
        // orthographic size = 半高 = 2 + 小边距
        camera.orthographicSize = 2.5f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.08f, 0.12f, 0.08f);

        // Camera位置居中
        var pos = cam.transform.position;
        cam.transform.position = new Vector3(0, 0, pos.z);

        EditorUtility.SetDirty(cam);

        // FarmGrid originY调整让格子垂直居中
        var grid = cam.GetComponent<FarmGrid>();
        if (grid != null)
        {
            // cellSize=1, 4行, 垂直中心在y=0
            // origin = -height/2 = -2
            grid.originY = -2f;
            grid.originX = -15f; // 30列从-15到+15
            EditorUtility.SetDirty(cam);
        }

        // FarmMapScroller范围
        var scroller = cam.GetComponent<FarmMapScroller>();
        if (scroller != null)
        {
            scroller.mapMinX = -15f;
            scroller.mapMaxX = 15f;
            EditorUtility.SetDirty(cam);
        }

        // Tilemap位置匹配FarmGrid
        var tilemapRoot = GameObject.Find("FarmTilemap");
        if (tilemapRoot != null)
        {
            tilemapRoot.transform.position = new Vector3(-15f, -2f, 2f);
            var g = tilemapRoot.GetComponent<Grid>();
            if (g != null) g.cellSize = new Vector3(1f, 1f, 0f);
            EditorUtility.SetDirty(tilemapRoot);
        }

        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("[FixFarmCamera] Camera ortho=2.5, farm centered.");
    }
}
