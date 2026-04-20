using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;

public class AdjustTilemapCamera
{
    public static void Execute()
    {
        // ── 1. 调整 Camera orthographic size ──────────────────
        var cam = GameObject.Find("Main Camera");
        if (cam != null)
        {
            var camera = cam.GetComponent<Camera>();
            if (camera != null)
            {
                // FarmGrid height = 4 * 1.28 = 5.12 world units
                // 想让草地占满屏幕高度，加一点上下边距
                camera.orthographicSize = 3.5f;
                EditorUtility.SetDirty(cam);
                Debug.Log("Camera orthographic size set to 3.5");
            }
        }

        // ── 2. 调整 FarmTilemap 位置 ───────────────────────────
        // Tilemap cell=1x1, FarmGrid cell=1.28
        // 我们把 Tilemap 的每个tile对应1.28世界单位
        // 方法：设置 Grid cell size 为 (1.28, 1.28)
        var farmTilemap = GameObject.Find("FarmTilemap");
        if (farmTilemap != null)
        {
            var grid = farmTilemap.GetComponent<Grid>();
            if (grid != null)
            {
                grid.cellSize = new Vector3(1.28f, 1.28f, 0f);
                EditorUtility.SetDirty(farmTilemap);
                Debug.Log("Set Grid cell size to 1.28");
            }

            // 位置: FarmGrid originX=-19.2, originY=-2.56
            farmTilemap.transform.position = new Vector3(-19.2f, -2.56f, 1f);
            EditorUtility.SetDirty(farmTilemap);
        }

        // ── 3. 调整 FarmGrid 的 gridColor 更明显 ──────────────
        var camGO = GameObject.Find("Main Camera");
        if (camGO != null)
        {
            var farmGrid = camGO.GetComponent<FarmGrid>();
            if (farmGrid != null)
            {
                farmGrid.gridColor = new Color(0f, 0f, 0f, 0.35f);
                farmGrid.lineWidth = 0.05f;
                EditorUtility.SetDirty(camGO);
            }
        }

        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("AdjustTilemapCamera done!");
    }
}
