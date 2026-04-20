using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;

public class FixRenderOrder
{
    public static void Execute()
    {
        // Tilemap移到z=2（更远，在FarmPlot后面）
        // FarmPlot在z=0
        // 相机在z=-10，看向正z方向
        // 所以z越大越靠后（越先被遮挡）
        // 正确顺序：相机(-10) → FarmPlot(z=0) → Tilemap(z=2)

        var tilemapRoot = GameObject.Find("FarmTilemap");
        if (tilemapRoot != null)
        {
            var pos = tilemapRoot.transform.position;
            tilemapRoot.transform.position = new Vector3(pos.x, pos.y, 2f);
            EditorUtility.SetDirty(tilemapRoot);
            Debug.Log($"FarmTilemap z set to 2");
        }

        // 确保TilemapRenderer sorting order足够低
        var tilemap = tilemapRoot?.GetComponentInChildren<Tilemap>();
        if (tilemap != null)
        {
            var tr = tilemap.GetComponent<TilemapRenderer>();
            if (tr != null)
            {
                tr.sortingOrder = -20;  // 最底层
                tr.sortingLayerName = "Default";
                EditorUtility.SetDirty(tr.gameObject);
                Debug.Log("TilemapRenderer sortingOrder=-20");
            }
        }

        // GridLines也要在FarmPlot后面
        // GridLines的LineRenderer sortingOrder=5，FarmPlot的SpriteRenderer sortingOrder=1
        // 这样网格线会显示在FarmPlot上面（好的）

        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("FixRenderOrder done!");
    }
}
