using UnityEngine;
using UnityEditor;

public class TestPlacement
{
    public static void Execute()
    {
        var grid = FarmGrid.Instance;
        if (grid == null) { Debug.LogError("FarmGrid.Instance is null!"); return; }
        if (PlacementManager.Instance == null) { Debug.LogError("PlacementManager.Instance is null!"); return; }

        // 放置几个测试格子
        var testCells = new []
        {
            new UnityEngine.Vector2Int(0, 0),
            new UnityEngine.Vector2Int(1, 0),
            new UnityEngine.Vector2Int(2, 0),
            new UnityEngine.Vector2Int(0, 1),
        };

        int count = 0;
        foreach (var cell in testCells)
        {
            if (grid.IsValid(cell) && !grid.IsOccupied(cell))
            {
                var worldPos = grid.CellToWorld(cell);
                PlotFactory.Create(worldPos, grid.cellSize);
                grid.SetOccupied(cell, true);
                count++;
                Debug.Log($"Placed plot at cell {cell} → world {worldPos}");
            }
        }
        Debug.Log($"TestPlacement: placed {count} plots. cellSize={grid.cellSize}");
    }
}
