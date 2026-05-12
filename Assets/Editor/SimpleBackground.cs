using UnityEngine;
using UnityEditor;
using System.IO;

public class SimpleBackground
{
    [MenuItem("Farm/Setup Simple Background")]
    public static void Execute()
    {
        // Strategy: use Farm_Reference as the actual background sprite
        // It's already 1376x768, PPU=76.8, fills the camera perfectly
        // Just remove alpha and put it at Order -10 as the real background

        // Delete all existing BG_ children and rebuild clean
        var bgRoot = GameObject.Find("Background");
        if (bgRoot != null)
        {
            // Clear all children
            for (int i = bgRoot.transform.childCount - 1; i >= 0; i--)
                Object.DestroyImmediate(bgRoot.transform.GetChild(i).gameObject);
        }
        else
        {
            bgRoot = new GameObject("Background");
        }

        // Also clean up old Reference_Layer
        var oldRef = GameObject.Find("Reference_Layer");
        if (oldRef != null) Object.DestroyImmediate(oldRef);

        // Create BG_Reference: the farm scene as full-screen background (opaque)
        var bgGO = new GameObject("BG_Reference");
        bgGO.transform.SetParent(bgRoot.transform);
        bgGO.transform.position = Vector3.zero;
        bgGO.transform.localScale = Vector3.one;
        var bgSR = bgGO.AddComponent<SpriteRenderer>();
        bgSR.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Farm_Reference.png");
        bgSR.sortingOrder = -10;
        bgSR.color = Color.white;

        // Create placeholder layers on top for interactive elements
        var layerDefs = new (string name, int order)[]
        {
            ("BG_Props",    -5),  // rocks, decorations
            ("BG_Crops",    -4),  // wheat crops
            ("BG_Trees",    -3),  // trees (foreground)
            ("BG_Bridge",   -2),  // bridge
            ("BG_Plants",   -1),  // plants, reeds
        };

        foreach (var (name, order) in layerDefs)
        {
            var go = new GameObject(name);
            go.transform.SetParent(bgRoot.transform);
            go.transform.position = Vector3.zero;
            go.AddComponent<SpriteRenderer>().sortingOrder = order;
        }

        // Set camera background to sky color
        if (Camera.main != null)
            Camera.main.backgroundColor = new Color(0.494f, 0.784f, 0.894f);

        // Render and check
        RenderAndSave();
        Debug.Log("Simple background setup done. BG_Reference is the full-screen farm scene.");
    }

    static void RenderAndSave()
    {
        var cam = Camera.main;
        if (cam == null) return;
        int w = 1376, h = 768;
        var rt = new RenderTexture(w, h, 24);
        cam.targetTexture = rt;
        cam.Render();
        RenderTexture.active = rt;
        var tex = new Texture2D(w, h, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
        tex.Apply();
        cam.targetTexture = null;
        RenderTexture.active = null;
        Object.DestroyImmediate(rt);
        File.WriteAllBytes(Application.dataPath + "/Sprites/GameView_Preview.png", tex.EncodeToPNG());
        AssetDatabase.Refresh();
        Debug.Log("Preview saved.");
    }
}
