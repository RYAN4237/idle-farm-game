using UnityEngine;
using UnityEditor;

public class CheckBGMaterial
{
    public static void Execute()
    {
        var go = GameObject.Find("BGReference");
        if (go == null) { Debug.LogError("BGReference not found"); return; }
        var sr = go.GetComponent<SpriteRenderer>();
        if (sr == null) { Debug.LogError("No SpriteRenderer"); return; }
        
        Debug.Log($"[BG] sprite={sr.sprite?.name ?? "NULL"}");
        Debug.Log($"[BG] material={sr.sharedMaterial?.name ?? "NULL"}");
        Debug.Log($"[BG] sortingLayer={sr.sortingLayerName} order={sr.sortingOrder}");
        Debug.Log($"[BG] enabled={sr.enabled} color={sr.color} alpha={sr.color.a}");
        Debug.Log($"[BG] z={go.transform.position.z}");
        
        // Also check if sprite import is correct
        if (sr.sprite != null)
        {
            var ti = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(sr.sprite)) as TextureImporter;
            if (ti != null)
                Debug.Log($"[BG] texture type={ti.textureType} PPU={ti.spritePixelsPerUnit}");
        }
    }
}
