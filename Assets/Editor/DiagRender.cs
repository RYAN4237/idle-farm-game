using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;

public class DiagRender
{
    [MenuItem("Tools/Diag Render")]
    public static void Execute()
    {
        // Check Map_Ground tilemap renderer
        var tmGo = GameObject.Find("Tilemap/Map_Ground");
        if (tmGo != null)
        {
            var tr = tmGo.GetComponent<TilemapRenderer>();
            Debug.Log($"Map_Ground TilemapRenderer: mat={tr?.sharedMaterial?.name} sortOrder={tr?.sortingOrder}");
            var tm = tmGo.GetComponent<Tilemap>();
            Debug.Log($"Map_Ground color={tm?.color}");
            // Force refresh
            tm?.RefreshAllTiles();
            Debug.Log("RefreshAllTiles called");
        }
        else Debug.LogWarning("Map_Ground not found");

        // Check SkyBG
        var sky = GameObject.Find("SkyBG");
        if (sky != null)
        {
            var sr = sky.GetComponent<SpriteRenderer>();
            Debug.Log($"SkyBG pos={sky.transform.position} scale={sky.transform.localScale} " +
                      $"sortOrder={sr?.sortingOrder} mat={sr?.sharedMaterial?.name} sprite={sr?.sprite?.name}");
            // SkyBG at y=10, scale=(26,10) → bounds y=5..15 → overlaps grass y=4..12!
            // Fix: move up so bottom edge is above y=4 (trees reach ~y=11, sky should start ~y=9)
            sky.transform.position = new Vector3(10f, 14f, 1f);
            sky.transform.localScale = new Vector3(26f, 12f, 1f);
            Debug.Log("SkyBG repositioned: pos=(10,14,1) scale=(26,12,1) → bounds y=8..20");
        }
        else Debug.LogWarning("SkyBG not found");

        // Check all TilemapRenderers
        foreach (var tr in Object.FindObjectsByType<TilemapRenderer>(FindObjectsInactive.Include))
            Debug.Log($"TmR {tr.gameObject.name}: mat={tr.sharedMaterial?.name} order={tr.sortingOrder}");
    }
}
