using UnityEngine;
using UnityEditor;
using System.IO;

public class FixAndRender
{
    [MenuItem("Farm/Fix All And Render")]
    public static void Execute()
    {
        // 1. Remove EditorOnly tag from Reference_Layer so it shows in Game View
        var refGO = GameObject.Find("Reference_Layer");
        if (refGO != null)
        {
            refGO.tag = "Untagged"; // show in Game View too
            var sr = refGO.GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = new Color(1, 1, 1, 0.45f);
            Debug.Log("Reference_Layer: removed EditorOnly tag");
        }

        // 2. Fix all BG layer scales
        // Camera: ortho 5 = 10h x 17.92w world units
        // Reference image: 1376x768px, PPU=76.8
        // x_world = (px_from_left - 688) / 76.8
        // y_world = (384 - px_from_top) / 76.8
        //
        // Sprite sizes at PPU=32:
        //   ground tile (grass):  366x352px => 11.44 x 11.0 world
        //   tree sprite (662x364px per cell): 20.69 x 11.375 world
        //   bridge (1365x256px):  42.66 x 8.0 world
        //   rock_large (682x256): 21.31 x 8.0 world
        //   wheat_2 (455x256):    14.22 x 8.0 world
        //   cattail (273x768):    8.53 x 24.0 world

        // BG_Ground: fill full screen
        // Need 17.92w x 10h. Sprite=11.44x11. Scale=(17.92/11.44, 10/11)=(1.567, 0.909)
        // Pivot=BottomCenter => pos y=-5 to sit at bottom of camera
        SetGO("Background/BG_Ground",        0,     -5.0f,  1.567f, 0.909f);

        // BG_River: water strip across middle
        // In ref: river y=360~450px, center y=405 => world y=(384-405)/76.8=-0.27
        // Height=90px => 1.17w. sy=1.17/11=0.106. Full width sx=1.567
        // Pivot=Center (ground tiles)
        SetGO("Background/BG_River",          0,    -0.6f,  1.567f, 0.106f);

        // BG_Treeline_Back: left tree cluster
        // In ref: left tree x=50~320px, center=185. y base=~450px
        // x_world=(185-688)/76.8=-6.55, y_world=(384-450)/76.8=-0.86 (base)
        // Tree height in ref ~300px => 3.9w. sy=3.9/11.375=0.343
        // Tree width in ref ~220px => 2.86w. sx=2.86/20.69=0.138
        SetGO("Background/BG_Treeline_Back", -6.5f, -0.9f,  0.138f, 0.343f);

        // BG_Trees_Front: right trees
        // In ref: right tree x=980~1180, center=1080, y base=~455px
        // x_world=(1080-688)/76.8=5.1, y_world=(384-455)/76.8=-0.92
        SetGO("Background/BG_Trees_Front",    5.1f, -0.9f,  0.138f, 0.343f);

        // BG_Bridge: wooden bridge
        // In ref: bridge x=430~570, center=500, y base=~450px
        // x_world=(500-688)/76.8=-2.45, y_world=(384-450)/76.8=-0.86
        // Bridge width in ref ~130px => 1.69w. sx=1.69/42.66=0.040
        // Bridge height in ref ~70px => 0.91w. sy=0.91/8=0.114
        SetGO("Background/BG_Bridge",        -2.4f, -1.2f,  0.040f, 0.114f);

        // BG_Props: rock cluster
        // In ref: rocks x=750~870, center=810, y base=~430px
        // x_world=(810-688)/76.8=1.59, y_world=(384-430)/76.8=-0.60
        // Rock width ~100px => 1.3w. sx=1.3/21.31=0.061
        SetGO("Background/BG_Props",          1.6f, -0.9f,  0.061f, 0.061f);

        // BG_Crops: wheat field
        // In ref: wheat x=630~740, center=685, y base=~355px
        // x_world=(685-688)/76.8=-0.04, y_world=(384-355)/76.8=0.38
        // Wheat height ~100px => 1.3w. sy=1.3/8=0.163. width~90px sx=0.090/14.22=0.090
        SetGO("Background/BG_Crops",         -0.04f, 0.1f,  0.090f, 0.163f);

        // BG_Plants_Front: cattail/reeds
        // In ref: reeds x=840~880, center=860, y base=~445px
        // x_world=(860-688)/76.8=2.24, y_world=(384-445)/76.8=-0.79
        // Cattail height ~100px => 1.3w. sy=1.3/24=0.054
        SetGO("Background/BG_Plants_Front",   2.2f, -1.0f,  0.054f, 0.054f);

        // 3. Render Game View and save
        RenderAndSave();
    }

    static void SetGO(string path, float x, float y, float sx, float sy)
    {
        var go = GameObject.Find(path);
        if (go == null) { Debug.LogWarning("Not found: " + path); return; }
        go.transform.position = new Vector3(x, y, 0);
        go.transform.localScale = new Vector3(sx, sy, 1);
    }

    static void RenderAndSave()
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
        File.WriteAllBytes(Application.dataPath + "/Sprites/GameView_Preview.png", bytes);
        AssetDatabase.Refresh();
        Debug.Log("Game View saved to Assets/Sprites/GameView_Preview.png");
    }
}
