using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;

public class DiagnoseCorners
{
    [MenuItem("Tools/Diagnose Corners")]
    public static void Execute()
    {
        var grassGO = GameObject.Find("Tilemap/GrassLayer") ?? GameObject.Find("GrassLayer");
        var grassTm = grassGO.GetComponent<Tilemap>();

        // Check the exact corner positions that appear black in the screenshot
        var testPositions = new[] {
            new Vector3Int(-3,-3,0), new Vector3Int(-2,-3,0), new Vector3Int(-1,-3,0),
            new Vector3Int(22,-3,0), new Vector3Int(21,-3,0), new Vector3Int(20,-3,0),
            new Vector3Int(-3,-2,0), new Vector3Int(-3,-1,0), new Vector3Int(-3, 7,0),
            new Vector3Int(22, 7,0), new Vector3Int(22, 0,0), new Vector3Int(22,-1,0),
        };

        var sb = new System.Text.StringBuilder("Corner tile check:\n");
        foreach (var pos in testPositions)
        {
            var tile = grassTm.GetTile(pos);
            var t2 = grassTm.GetTile<UnityEngine.Tilemaps.Tile>(pos);
            sb.AppendLine($"  {pos}: tile={tile?.name ?? "NULL"}, sprite={t2?.sprite?.name ?? "NONE"}");
        }
        Debug.Log(sb.ToString());

        // Also check DirtLayer sortingOrder
        var dirtGO = GameObject.Find("Tilemap/DirtLayer");
        if (dirtGO != null)
        {
            var r = dirtGO.GetComponent<TilemapRenderer>();
            Debug.Log($"DirtLayer sortingOrder={r.sortingOrder}");
        }
    }
}
