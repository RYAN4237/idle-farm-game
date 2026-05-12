using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using System.Text;

public class DiagnoseTilemap
{
    [MenuItem("Tools/Diagnose Tilemap Black Holes")]
    public static void Execute()
    {
        var grassGO = GameObject.Find("Tilemap/GrassLayer") ?? GameObject.Find("GrassLayer");
        var waterGO = GameObject.Find("Tilemap/WaterLayer") ?? GameObject.Find("WaterLayer");

        if (grassGO == null) { Debug.LogError("GrassLayer not found"); return; }

        var grassTm = grassGO.GetComponent<Tilemap>();
        var waterTm = waterGO?.GetComponent<Tilemap>();

        var grassRenderer = grassGO.GetComponent<TilemapRenderer>();
        var waterRenderer = waterGO?.GetComponent<TilemapRenderer>();

        Debug.Log($"GrassLayer sortingLayerID={grassRenderer?.sortingLayerID} sortingOrder={grassRenderer?.sortingOrder}");
        Debug.Log($"WaterLayer sortingLayerID={waterRenderer?.sortingLayerID} sortingOrder={waterRenderer?.sortingOrder}");

        // Check pond area for missing grass tiles
        var sb = new StringBuilder();
        sb.AppendLine("Pond area grass tiles (1..5, -2..0):");
        for (int y = 0; y >= -2; y--)
        for (int x = 1; x <= 5; x++)
        {
            var pos = new Vector3Int(x, y, 0);
            var tile = grassTm.GetTile(pos);
            sb.Append($"  ({x},{y})=");
            sb.AppendLine(tile == null ? "NULL" : tile.name ?? tile.GetType().Name);
        }
        Debug.Log(sb.ToString());

        // Count total tiles in GrassLayer
        var bounds = grassTm.cellBounds;
        int nullCount = 0, tileCount = 0;
        for (int x = bounds.xMin; x < bounds.xMax; x++)
        for (int y = bounds.yMin; y < bounds.yMax; y++)
        {
            var t = grassTm.GetTile(new Vector3Int(x, y, 0));
            if (t == null) nullCount++; else tileCount++;
        }
        Debug.Log($"GrassLayer bounds={bounds}, tiles={tileCount}, nulls={nullCount}");
    }
}
