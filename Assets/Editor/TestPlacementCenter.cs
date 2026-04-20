using UnityEngine;
using UnityEditor;

public class TestPlacementCenter
{
    public static void Execute()
    {
        var grid = Object.FindObjectOfType<FarmGrid>();
        if (grid == null) { Debug.LogError("FarmGrid not found"); return; }

        Debug.Log($"Grid: cellSize={grid.cellSize}, origin=({grid.originX},{grid.originY})");

        // 相机在x=0，ortho=3.5 → 可见x大约-6到+6
        // cellSize=1, originX=-15 → col=(x-(-15))/1 → x=0时col=15
        // 所以col 12~18在中心可见区域
        int[] cols = { 12, 13, 14, 15, 16 };
        int[] rows = { 1, 2 };
        int count  = 0;

        foreach (int col in cols)
        foreach (int row in rows)
        {
            var cell    = new Vector2Int(col, row);
            var worldPos = grid.CellToWorld(cell);
            PlotFactory.Create(worldPos, grid.cellSize);
            // 不调用IsOccupied（Editor线程occupied数组可能未初始化）
            count++;
            Debug.Log($"  cell({col},{row}) → {worldPos}");
        }
        Debug.Log($"Placed {count} test plots");
    }
}
