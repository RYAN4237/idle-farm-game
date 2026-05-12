using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

/// Fix TransitionLayer: grass must be underneath transition tiles
/// TransitionLayer tiles are transparent overlays — they need solid grass behind them.
/// Solution: TransitionLayer stays at sortingOrder=2, but GrassLayer fills ALL cells
/// including transition zones. The overlay tiles' transparent parts show the grass beneath.
public class FixTransitionLayer
{
    const string TILE_DIR = "Assets/Tiles";

    [MenuItem("Tools/Fix Transition Layer")]
    public static void Execute()
    {
        // 1. Ensure GrassLayer covers the full map (already done, just verify sortingOrder=0)
        var grassGO = GameObject.Find("Tilemap/GrassLayer") ?? GameObject.Find("GrassLayer");
        if (grassGO != null)
        {
            var r = grassGO.GetComponent<TilemapRenderer>();
            r.sortingOrder = 0;
            Debug.Log("GrassLayer sortingOrder=0 confirmed");
        }

        // 2. WaterLayer at sortingOrder=1 (above grass, below transition)
        var waterGO = GameObject.Find("Tilemap/WaterLayer") ?? GameObject.Find("WaterLayer");
        if (waterGO != null)
        {
            var r = waterGO.GetComponent<TilemapRenderer>();
            r.sortingOrder = 1;
        }

        // 3. TransitionLayer at sortingOrder=2 (topmost — transparent parts show layer below)
        var transGO = GameObject.Find("Tilemap/TransitionLayer") ?? GameObject.Find("TransitionLayer");
        if (transGO != null)
        {
            var r = transGO.GetComponent<TilemapRenderer>();
            r.sortingOrder = 2;
            Debug.Log("TransitionLayer sortingOrder=2 confirmed");
        }

        // 4. The black holes appear because transition tiles from Grass_22..43
        //    have transparent pixels that show the camera background (black).
        //    Since GrassLayer IS filled underneath, the issue is that TilemapRenderer
        //    uses "Individual" chunk mode which can Z-fight. Force "Chunk" mode.
        if (grassGO != null)
        {
            var r = grassGO.GetComponent<TilemapRenderer>();
            r.mode = TilemapRenderer.Mode.Chunk;
            EditorUtility.SetDirty(grassGO);
        }
        if (waterGO != null)
        {
            var r = waterGO.GetComponent<TilemapRenderer>();
            r.mode = TilemapRenderer.Mode.Chunk;
            EditorUtility.SetDirty(waterGO);
        }
        if (transGO != null)
        {
            var r = transGO.GetComponent<TilemapRenderer>();
            r.mode = TilemapRenderer.Mode.Chunk;
            EditorUtility.SetDirty(transGO);
        }

        // 5. Check if transition tiles are actually showing the wrong thing.
        //    The Sprout Lands grass edge tiles (Grass_22..43) are designed as
        //    STANDALONE tiles that show grass WITH water edge built into the sprite.
        //    They are NOT transparent overlays — they ARE the full tile graphic.
        //    So using them on TransitionLayer means the transparent area of the tile
        //    shows through to the black camera background, not the layer below.
        //
        //    REAL FIX: Move transition tiles back into GrassLayer directly,
        //    replacing the solid grass tile at those positions.
        //    The water (sortingOrder=1) renders on top anyway, so the grass tile
        //    underneath water is irrelevant. Only the grass cells ADJACENT to water
        //    need the edge tile treatment.

        MoveTransitionIntoGrass();

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log("[FixTransitionLayer] Done");
    }

    static void MoveTransitionIntoGrass()
    {
        var transGO = GameObject.Find("Tilemap/TransitionLayer") ?? GameObject.Find("TransitionLayer");
        var grassGO = GameObject.Find("Tilemap/GrassLayer") ?? GameObject.Find("GrassLayer");
        if (transGO == null || grassGO == null) return;

        var transTm = transGO.GetComponent<Tilemap>();
        var grassTm = grassGO.GetComponent<Tilemap>();

        // Copy every tile from TransitionLayer into GrassLayer (overwriting grass there)
        var bounds = transTm.cellBounds;
        int moved = 0;
        for (int x = bounds.xMin; x < bounds.xMax; x++)
        for (int y = bounds.yMin; y < bounds.yMax; y++)
        {
            var pos  = new Vector3Int(x, y, 0);
            var tile = transTm.GetTile(pos);
            if (tile == null) continue;

            grassTm.SetTile(pos, tile);
            moved++;
        }

        // Clear TransitionLayer (it's now empty, grass handles it all)
        transTm.ClearAllTiles();

        EditorUtility.SetDirty(grassGO);
        EditorUtility.SetDirty(transGO);
        Debug.Log($"Moved {moved} transition tiles into GrassLayer. TransitionLayer cleared.");
    }
}
