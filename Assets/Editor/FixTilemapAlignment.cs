using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;

public class FixTilemapAlignment
{
    public static void Execute()
    {
        var root = GameObject.Find("FarmTilemap");
        if (root == null) { Debug.LogError("FarmTilemap not found"); return; }

        // FarmGrid: originX=-19.2, originY=-2.56, cellSize=1.28
        // gridWidth=30, gridHeight=4
        // 总宽=38.4, 总高=5.12
        // 中心: x=0, y=-2.56+2.56=0

        // 1. 设置Grid cell size = 1.28
        var grid = root.GetComponent<Grid>();
        if (grid != null)
        {
            grid.cellSize = new Vector3(1.28f, 1.28f, 0f);
            EditorUtility.SetDirty(root);
            Debug.Log("Grid cell size set to 1.28");
        }

        // 2. 设置Tilemap位置从FarmGrid原点开始
        // FarmGrid originX=-19.2, originY=-2.56
        // 但Tilemap的tile(0,0)对应左下角
        root.transform.position = new Vector3(-19.2f, -2.56f, 1f);
        EditorUtility.SetDirty(root);

        // 3. 设置TilemapRenderer sorting order
        var tilemap = root.GetComponentInChildren<Tilemap>();
        if (tilemap != null)
        {
            var tr = tilemap.GetComponent<TilemapRenderer>();
            if (tr != null)
            {
                tr.sortingOrder = -10;
                EditorUtility.SetDirty(tr.gameObject);
            }
        }

        // 4. Camera位置归0
        var cam = GameObject.Find("Main Camera");
        if (cam != null)
        {
            cam.transform.position = new Vector3(0, 0, -10);
            // orthographic size: 要看到4行1.28高的格子 = 5.12高
            // 加上下边距设为4.0
            cam.GetComponent<Camera>().orthographicSize = 4.0f;
            EditorUtility.SetDirty(cam);
            Debug.Log("Camera reset to (0,0,-10), ortho size=4.0");
        }

        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("FixTilemapAlignment done!");
    }
}
