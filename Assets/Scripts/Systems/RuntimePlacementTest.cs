using UnityEngine;
using System.Collections;

public class RuntimePlacementTest : MonoBehaviour
{
    IEnumerator Start()
    {
        // 等一帧让所有Awake/Start都完成
        yield return null;
        yield return null;

        var grid = FarmGrid.Instance;
        if (grid == null) { Debug.LogError("FarmGrid.Instance still null after 2 frames!"); yield break; }

        int count = 0;
        for (int col = 13; col <= 17; col++)
        for (int row = 1; row <= 2; row++)
        {
            var cell = new Vector2Int(col, row);
            if (!grid.IsValid(cell) || grid.IsOccupied(cell)) continue;
            var wp = grid.CellToWorld(cell);
            PlotFactory.Create(wp, grid.cellSize);
            grid.SetOccupied(cell, true);
            count++;
            Debug.Log($"[RuntimePlacementTest] Placed cell({col},{row}) → {wp}");
        }
        Debug.Log($"[RuntimePlacementTest] Done: {count} plots placed");
        Destroy(this);
    }
}
