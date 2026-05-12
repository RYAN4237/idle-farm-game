using UnityEngine;
using UnityEditor;

public class PreciseLayout
{
    // Reference: 1376x768px, PPU=76.8, fills camera exactly (-8.96 to 8.96, -5 to 5)
    // Formula: x_world = (px - 688) / 76.8,  y_world = (384 - py) / 76.8
    // IMPORTANT: ground/tree sprites have BottomCenter pivot
    // So position y = bottom edge of sprite in world space
    // Camera bottom = -5.0

    [MenuItem("Farm/Precise Layout")]
    public static void Execute()
    {
        // BG_Ground: grass tile, pivot=BottomCenter
        // Fill full screen: width=17.92, height=10
        // Sprite is 366x352px @ PPU=32 => 11.44 x 11w
        // Scale: sx=17.92/11.44=1.566, sy=10/11=0.909
        // Position: bottom at y=-5 => pos y=-5
        Set("Background/BG_Ground",        0,      -5.0f,  1.566f, 0.909f);

        // BG_River: water tile, pivot=Center (changed in ground tiles)
        // River in ref: y center ~410px from top => y_world=(384-410)/76.8=-0.34
        // Height ~80px => 1.04w. Sprite 366x352=>11.44x11. sy=1.04/11=0.095
        Set("Background/BG_River",          0,     -0.9f,  1.566f, 0.095f);

        // BG_Treeline_Back: tree_0, pivot=BottomCenter
        // Left trees in ref: base at y~440px => y_world=(384-440)/76.8=-0.73
        // Center x~190px => x_world=(190-688)/76.8=-6.48
        // Tree height ~380px => 4.95w. Sprite 662x364=>20.7x11.4. sy=4.95/11.4=0.434
        // Tree width ~260px => 3.39w. sx=3.39/20.7=0.164
        Set("Background/BG_Treeline_Back", -6.5f,  -0.7f,  0.20f,  0.43f);

        // BG_Trees_Front: tree_1, pivot=BottomCenter
        // Right trees base: y~460px => y_world=(384-460)/76.8=-0.99
        // Center x~1060px => x_world=(1060-688)/76.8=4.84
        Set("Background/BG_Trees_Front",    4.8f,  -1.0f,  0.20f,  0.38f);

        // BG_Bridge: deco_bridge 1365x256px=>42.7x8w, pivot=BottomCenter
        // Bridge bottom: y~450px => y_world=(384-450)/76.8=-0.86
        // Center x~505px => x_world=(505-688)/76.8=-2.38
        // Bridge width ~160px => 2.08w. sx=2.08/42.7=0.049
        // Bridge height ~80px => 1.04w. sy=1.04/8=0.130
        Set("Background/BG_Bridge",        -2.4f,  -1.2f,  0.049f, 0.13f);

        // BG_Props: deco_rock_large 682x256px=>21.3x8w, pivot=BottomCenter
        // Rocks bottom: y~420px => y_world=(384-420)/76.8=-0.47
        // Center x~810px => x_world=(810-688)/76.8=1.59
        // Rock width ~110px => 1.43w. sx=1.43/21.3=0.067
        Set("Background/BG_Props",          1.6f,  -0.8f,  0.067f, 0.067f);

        // BG_Crops: deco_wheat_2 455x256px=>14.2x8w, pivot=BottomCenter
        // Wheat bottom: y~360px => y_world=(384-360)/76.8=0.31
        // Center x~680px => x_world=(680-688)/76.8=-0.10
        // Wheat height ~100px => 1.3w. sy=1.3/8=0.163
        Set("Background/BG_Crops",         -0.1f,  -0.3f,  0.10f,  0.163f);

        // BG_Plants_Front: plant_cattail 273x768px=>8.5x24w, pivot=BottomCenter
        // Cattail base: y~460px => y_world=(384-460)/76.8=-0.99
        // Center x~890px => x_world=(890-688)/76.8=2.63
        // Cattail height ~120px => 1.56w. sy=1.56/24=0.065
        Set("Background/BG_Plants_Front",   2.6f,  -1.0f,  0.065f, 0.065f);

        Debug.Log("Precise layout done.");
    }

    static void Set(string path, float x, float y, float sx, float sy)
    {
        var go = GameObject.Find(path);
        if (go == null) { Debug.LogWarning("Not found: " + path); return; }
        go.transform.position = new Vector3(x, y, 0);
        go.transform.localScale = new Vector3(sx, sy, 1);
    }
}
