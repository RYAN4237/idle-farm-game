using UnityEngine;
using UnityEditor;

public class DiagRenderer
{
    public static void Execute()
    {
        // Check URP 2D renderer asset
        var rpAsset = UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline;
        Debug.Log($"[Diag] Pipeline asset: {rpAsset?.name ?? "NULL"}");
        
        // Check camera
        var cam = GameObject.FindFirstObjectByType<Camera>();
        if (cam != null)
        {
            var camData = cam.GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
            Debug.Log($"[Diag] Camera: pos={cam.transform.position} orthoSize={cam.orthographicSize} clearFlags={cam.clearFlags} bg={cam.backgroundColor}");
            Debug.Log($"[Diag] CameraData exists: {camData != null}");
        }
        
        // Check BGReference sprite bounds in camera view
        var bg = GameObject.Find("BGReference");
        if (bg != null)
        {
            var sr = bg.GetComponent<SpriteRenderer>();
            Debug.Log($"[Diag] BGRef bounds: {sr.bounds} (cam should see this)");
        }
    }
}
