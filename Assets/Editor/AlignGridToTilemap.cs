using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;

public class AlignGridToTilemap
{
    public static void Execute()
    {
        // Tilemap的Grid cell size=1x1
        // 把FarmGrid改为cellSize=1.0，重新计算origin让地图居中
        // Tilemap position=(-19.2, -2.56, 2)
        // 铺了43列8行（from FillTilemapGround log）
        // 所以Tilemap覆盖: x=-19.2 to -19.2+43=23.8, y=-2.56 to -2.56+8=5.44
        // 我们要FarmGrid覆盖可见的中间区域：30列x4行
        // 用 cellSize=1, 30列=30宽, 4行=4高
        // 以(0,0)为中心: originX=-15, originY=-2

        var cam = GameObject.Find("Main Camera");
        if (cam == null) { Debug.LogError("Main Camera not found"); return; }

        var grid = cam.GetComponent<FarmGrid>();
        if (grid == null) { Debug.LogError("FarmGrid not found"); return; }

        // 更新FarmGrid参数，对齐Tilemap（cellSize=1）
        grid.cellSize   = 1.0f;
        grid.gridWidth  = 30;
        grid.gridHeight = 4;
        grid.originX    = -15f;   // 中心x=0时，30格从-15到+15
        grid.originY    = -2f;    // 中心y=0时，4格从-2到+2
        EditorUtility.SetDirty(cam);

        // 把Tilemap也重新定位，从FarmGrid原点开始
        var tilemapRoot = GameObject.Find("FarmTilemap");
        if (tilemapRoot != null)
        {
            tilemapRoot.transform.position = new Vector3(-15f, -2f, 2f);
            // Grid cell size 也改为1x1（已经是默认值）
            var g = tilemapRoot.GetComponent<Grid>();
            if (g != null) g.cellSize = new Vector3(1f, 1f, 0f);
            EditorUtility.SetDirty(tilemapRoot);
        }

        // Camera orthographic size调整到能看到4行: 4/2=2 + 边距=3
        var camera = cam.GetComponent<Camera>();
        if (camera != null)
        {
            camera.orthographicSize = 3.5f;
            EditorUtility.SetDirty(cam);
        }

        // 删除RuntimePlacementTest组件
        var test = cam.GetComponent<RuntimePlacementTest>();
        if (test != null) Object.DestroyImmediate(test);

        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("AlignGridToTilemap done! cellSize=1.0, origin=(-15,-2), camera ortho=3.5");
    }
}
