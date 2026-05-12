using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;

public class DiagnoseRendering
{
    [MenuItem("Tools/Diagnose Rendering")]
    public static void Execute()
    {
        // Check Farmer
        var farmer = GameObject.Find("Farmer");
        if (farmer != null)
        {
            var sr = farmer.GetComponent<SpriteRenderer>();
            Debug.Log($"[Diag] Farmer material: {sr.sharedMaterial?.name} shader={sr.sharedMaterial?.shader?.name}");
        }

        // Check SkyBackground
        var sky = GameObject.Find("SkyBackground");
        if (sky != null)
        {
            var sr = sky.GetComponent<SpriteRenderer>();
            Debug.Log($"[Diag] SkyBackground material: {sr.sharedMaterial?.name} shader={sr.sharedMaterial?.shader?.name} z={sky.transform.position.z}");
        }

        // Check GrassLayer tilemap
        var grass = GameObject.Find("GrassLayer");
        if (grass != null)
        {
            var tr = grass.GetComponent<TilemapRenderer>();
            Debug.Log($"[Diag] GrassLayer material: {tr.sharedMaterial?.name} shader={tr.sharedMaterial?.shader?.name}");
        }

        // Check GlobalLight2D
        var light = GameObject.Find("GlobalLight2D");
        if (light != null)
        {
            var l = light.GetComponent<UnityEngine.Rendering.Universal.Light2D>();
            Debug.Log($"[Diag] GlobalLight2D: type={l.lightType} intensity={l.intensity} blendStyle={l.blendStyleIndex} enabled={l.enabled}");
        }

        // List all camera culling masks and sorting layers
        var cam = Camera.main;
        if (cam != null)
            Debug.Log($"[Diag] Camera cullingMask={cam.cullingMask} clearFlags={cam.clearFlags} bg={cam.backgroundColor}");
    }
}
