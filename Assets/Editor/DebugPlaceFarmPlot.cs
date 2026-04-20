using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;

public class DebugPlaceFarmPlot
{
    public static void Execute()
    {
        // 直接在Editor模式创建一个FarmPlot，放在场景中心
        // 不依赖FarmGrid.Instance
        Vector3 worldPos = new Vector3(0, 0, 0);
        float cellSize = 1.28f;

        var go = new GameObject("DebugFarmPlot");
        go.transform.position = worldPos;

        // 创建一个简单的颜色方块
        var sr = go.AddComponent<SpriteRenderer>();

        // 生成纯色纹理
        var tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, new Color(0.52f, 0.34f, 0.14f, 1f));
        tex.Apply();
        sr.sprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        sr.sortingOrder = 50;  // 超高，确保在最上层
        sr.color = new Color(0.52f, 0.34f, 0.14f, 1f);

        go.transform.localScale = new Vector3(cellSize, cellSize, 1f);

        Debug.Log($"DebugFarmPlot created at {worldPos}, scale={cellSize}, sortingOrder=50");
        EditorUtility.SetDirty(go);

        // 检查TilemapRenderer的实际sortingOrder
        var tilemap = Object.FindObjectOfType<TilemapRenderer>();
        if (tilemap != null)
            Debug.Log($"TilemapRenderer: sortingOrder={tilemap.sortingOrder}, layer={tilemap.sortingLayerName}");

        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    }
}
