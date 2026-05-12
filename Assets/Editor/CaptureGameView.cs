using UnityEngine;
using UnityEditor;
using System.IO;

public class CaptureGameView
{
    [MenuItem("Farm/Capture Game View")]
    public static void Execute()
    {
        var cam = Camera.main;
        if (cam == null) { Debug.LogWarning("No main camera"); return; }

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

        var bytes = tex.EncodeToPNG();
        var path = "Assets/Sprites/GameView_Preview.png";
        File.WriteAllBytes(Application.dataPath + "/Sprites/GameView_Preview.png", bytes);
        AssetDatabase.Refresh();
        Debug.Log("Saved to " + path);
    }
}
