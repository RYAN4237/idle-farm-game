using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;

public class CheckMaterials
{
    [MenuItem("Tools/Check Materials")]
    public static void Execute()
    {
        foreach (var tr in Object.FindObjectsByType<TilemapRenderer>(FindObjectsInactive.Include))
            Debug.Log($"TilemapRenderer [{tr.gameObject.name}] mat={tr.sharedMaterial?.name ?? "NULL"}  order={tr.sortingOrder}");
        foreach (var sr in Object.FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Include))
        {
            if (sr.GetComponentInParent<Canvas>()) continue;
            Debug.Log($"SR [{sr.gameObject.name}] mat={sr.sharedMaterial?.name ?? "NULL"} sprite={sr.sprite?.name ?? "NULL"}");
        }
        // Also check camera
        var cam = Object.FindAnyObjectByType<Camera>();
        if (cam) Debug.Log($"Camera bg={cam.backgroundColor}  orthoSize={cam.orthographicSize}  pos={cam.transform.position}");
    }
}
