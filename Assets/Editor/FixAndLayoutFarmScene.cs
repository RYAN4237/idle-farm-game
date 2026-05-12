using UnityEngine;
using UnityEditor;

public class FixAndLayoutFarmScene
{
    [MenuItem("Farm/Fix Reference + Layout")]
    public static void Execute()
    {
        // Step 1: Fix Farm_Reference PPU to match other sprites
        // Reference image is 1280x720px
        // We want it to fill the camera: ortho size=5 => height=10 world units
        // So PPU = 720/10 = 72
        var refImporter = AssetImporter.GetAtPath("Assets/Sprites/Farm_Reference.png") as TextureImporter;
        if (refImporter != null)
        {
            refImporter.spritePixelsPerUnit = 72;
            refImporter.filterMode = FilterMode.Point;
            refImporter.textureCompression = TextureImporterCompression.Uncompressed;
            refImporter.SaveAndReimport();
            Debug.Log("Fixed Farm_Reference PPU to 72");
        }

        // Step 2: Reset reference layer scale to 1,1,1 and center it
        var refGO = GameObject.Find("Reference_Layer");
        if (refGO != null)
        {
            refGO.transform.position = Vector3.zero;
            refGO.transform.localScale = Vector3.one;
            var sr = refGO.GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = new Color(1, 1, 1, 0.55f);
            Debug.Log("Reference_Layer reset to (0,0,0) scale (1,1,1)");
        }

        // Step 3: Layout all BG layers to match reference image
        // Camera: ortho 5, so visible = 17.78w x 10h, origin = center
        // Reference image 1280x720 => 1px = 1/72 world units
        // Pixel coords (from top-left): convert to world: x_world = (px - 640)/72, y_world = (360 - py)/72

        // BG_Ground: full-screen grass tile, stretch to cover entire view
        SetPosScale("Background/BG_Ground",        0,      -0.5f,  5.0f,  2.8f);

        // BG_River: water strip — in ref image spans full width at y~300-380px from top
        // y_world = (360 - 340)/72 = -1.1 to (360-420)/72 = 0.28 => center y=-0.8
        SetPosScale("Background/BG_River",          0,     -1.1f,  8.5f,  0.75f);

        // BG_Treeline_Back: left trees — center around x=-350px => -4.9, y=200px from top => 2.2
        SetPosScale("Background/BG_Treeline_Back", -4.5f,   1.5f,  2.2f,  2.2f);

        // BG_Trees_Front: right trees — x=450px => 1.0w off center=6.25, y=200px => 2.2
        SetPosScale("Background/BG_Trees_Front",    5.0f,   0.8f,  2.0f,  2.0f);

        // BG_Bridge: center-left over river — x=-80px => -1.1, y=310px => 0.69
        SetPosScale("Background/BG_Bridge",        -1.0f,  -0.7f,  1.0f,  1.0f);

        // BG_Props: rocks right of center — x=200px => 2.8, y=310px => 0.69
        SetPosScale("Background/BG_Props",          2.8f,  -0.5f,  0.8f,  0.8f);

        // BG_Crops: wheat upper right — x=160px right of center=2.2, y=190px from top=2.4
        SetPosScale("Background/BG_Crops",          2.0f,   0.8f,  0.6f,  0.6f);

        // BG_Plants_Front: cattail right bank — x=200px=>2.8, y=290px=>1.0
        SetPosScale("Background/BG_Plants_Front",   2.2f,  -0.6f,  0.6f,  0.6f);

        Debug.Log("Layout complete. Reference_Layer is the bottom guide.");
    }

    static void SetPosScale(string path, float x, float y, float sx, float sy)
    {
        var go = GameObject.Find(path);
        if (go == null) { Debug.LogWarning("Not found: " + path); return; }
        go.transform.position = new Vector3(x, y, 0);
        go.transform.localScale = new Vector3(sx, sy, 1);
    }
}
