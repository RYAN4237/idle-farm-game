using UnityEngine;
using UnityEditor;

public class LayoutFarmScene
{
    // Reference image = 17.92 x 10 world units (ortho size 5, 1280x720)
    // World origin (0,0) = center of screen
    // Left edge = -8.96, Right edge = 8.96, Top = 5, Bottom = -5

    [MenuItem("Farm/Layout Scene Positions")]
    public static void Execute()
    {
        // --- Reference layer: already correct, just confirm ---
        SetPos("Reference_Layer", 0, 0, 0);

        // --- BG_Ground: full-screen grass, stretch to fill entire view ---
        SetPos("Background/BG_Ground", 0, -0.5f, 0);
        SetScale("Background/BG_Ground", 5.5f, 3.2f);

        // --- BG_River: horizontal water strip across middle ---
        // In reference: river is roughly y=-0.5 to y=-2, centered horizontally
        SetPos("Background/BG_River", 0, -1.4f, 0);
        SetScale("Background/BG_River", 8f, 0.85f);

        // --- BG_Treeline_Back: left cluster of 3 trees (oak, back row) ---
        // In reference: trees on far left, tops at y~3
        SetPos("Background/BG_Treeline_Back", -5.5f, 0.8f, 0);
        SetScale("Background/BG_Treeline_Back", 1.8f, 1.8f);

        // --- BG_Trees_Front: right side trees ---
        // In reference: 2 trees right side, x~4 to 7
        SetPos("Background/BG_Trees_Front", 4.5f, 0.2f, 0);
        SetScale("Background/BG_Trees_Front", 1.8f, 1.8f);

        // --- BG_Bridge: wooden bridge center, over river ---
        // In reference: bridge center-left, x~-1.5, y~-1
        SetPos("Background/BG_Bridge", -1.5f, -0.8f, 0);
        SetScale("Background/BG_Bridge", 0.8f, 0.8f);

        // --- BG_Props: rock cluster right of bridge ---
        // In reference: rocks at x~3, y~-0.5
        SetPos("Background/BG_Props", 3.2f, -0.5f, 0);
        SetScale("Background/BG_Props", 0.7f, 0.7f);

        // --- BG_Crops: wheat field upper right ---
        // In reference: wheat at x~1.5, y~0.5
        SetPos("Background/BG_Crops", 1.5f, 0.5f, 0);
        SetScale("Background/BG_Crops", 0.5f, 0.5f);

        // --- BG_Plants_Front: cattail left riverbank ---
        // In reference: reeds at x~2.5, y~-0.8
        SetPos("Background/BG_Plants_Front", 2.5f, -0.8f, 0);
        SetScale("Background/BG_Plants_Front", 0.5f, 0.5f);

        Debug.Log("Layout applied. Comparing against Reference_Layer at Order -999.");
    }

    static void SetPos(string path, float x, float y, float z)
    {
        var go = GameObject.Find(path);
        if (go == null) { Debug.LogWarning("Not found: " + path); return; }
        go.transform.position = new Vector3(x, y, z);
    }

    static void SetScale(string path, float x, float y)
    {
        var go = GameObject.Find(path);
        if (go == null) return;
        go.transform.localScale = new Vector3(x, y, 1);
    }
}
