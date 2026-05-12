using UnityEngine;
using UnityEditor;

public class RebuildFarmScene
{
    [MenuItem("Farm/Rebuild Scene From Reference")]
    public static void Execute()
    {
        // Camera: ortho size=5 => visible 10h x 17.78w world units
        // Reference image: 1280x720px
        // To fill screen exactly: PPU = 720px / 10units = 72
        // At PPU=72: sprite world size = 1280/72 = 17.78w, 720/72 = 10h  ✓

        // Fix reference import
        var refImp = AssetImporter.GetAtPath("Assets/Sprites/Farm_Reference.png") as TextureImporter;
        if (refImp != null)
        {
            refImp.spritePixelsPerUnit = 72;
            refImp.filterMode = FilterMode.Point;
            refImp.textureCompression = TextureImporterCompression.Uncompressed;
            refImp.spriteImportMode = SpriteImportMode.Single;
            refImp.SaveAndReimport();
        }

        // Fix reference layer: scale=1, pos=0 => fills screen exactly
        var refGO = GameObject.Find("Reference_Layer");
        if (refGO != null)
        {
            refGO.transform.position = Vector3.zero;
            refGO.transform.localScale = Vector3.one;
            var sr = refGO.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.color = new Color(1, 1, 1, 0.5f);
                sr.sortingOrder = -999;
                // Reassign sprite after reimport
                sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Farm_Reference.png");
            }
        }

        // Now layout BG layers using normalized coords based on reference image
        // Reference image 1280x720, PPU=72
        // World coord formula:
        //   x_world = (pixel_x - 640) / 72
        //   y_world = (360 - pixel_y) / 72
        // All sprites also use PPU=32, so their world size = spritePixels/32

        // BG_Ground: full grass background - stretch to fill 17.78 x 10
        // grass tile is 366x352px at PPU=32 => 11.44 x 11 world units
        // Scale to fill: sx = 17.78/11.44 = 1.55, sy = 10/11 = 0.91
        SetPosScale("Background/BG_Ground",        0,      0f,    1.56f, 0.95f);

        // BG_River: water tile 366x352 at PPU=32 => 11.44 x 11 world units
        // In ref: river spans full width y ~290-380px from top
        // y_center = (360 - 335)/72 = -(-25)/72... let's say y=-1.2
        // Scale: full width sx=1.56, height ~1 unit sy=0.1
        SetPosScale("Background/BG_River",          0,     -1.3f,  1.56f, 0.12f);

        // BG_Treeline_Back: tree sprite ~662x364px at PPU=32 => 20.7 x 11.4 world
        // In ref: left trees centered ~x=200px from left => x_world=(200-640)/72=-6.1
        // y top of tree ~y=80px from top => y_world=(360-200)/72=2.2
        SetPosScale("Background/BG_Treeline_Back", -5.5f,   1.5f,  0.12f, 0.12f);

        // BG_Trees_Front: right trees
        // In ref: right trees x~1050px => (1050-640)/72=5.7
        SetPosScale("Background/BG_Trees_Front",    5.5f,   0.8f,  0.12f, 0.12f);

        // BG_Bridge: bridge ~1365x256px at PPU=32 => 42.7 x 8 world
        // In ref: bridge x~530px=>(530-640)/72=-1.5, y~340px=>(360-340)/72=0.28
        SetPosScale("Background/BG_Bridge",        -1.5f,  -1.2f,  0.06f, 0.06f);

        // BG_Props: rocks ~682x256px at PPU=32 => 21.3 x 8 world
        // In ref: rocks x~820px=>(820-640)/72=2.5, y~330px=>(360-330)/72=0.42
        SetPosScale("Background/BG_Props",          2.5f,  -0.8f,  0.08f, 0.08f);

        // BG_Crops: wheat ~455x256px at PPU=32 => 14.2 x 8 world
        // In ref: wheat x~780px=(780-640)/72=1.9, y~250px=(360-250)/72=1.5
        SetPosScale("Background/BG_Crops",          1.9f,   0.5f,  0.07f, 0.07f);

        // BG_Plants_Front: cattail ~273x768px at PPU=32 => 8.5 x 24 world
        // In ref: reeds x~820px=2.5, y~300px=(360-300)/72=0.83
        SetPosScale("Background/BG_Plants_Front",   2.5f,  -0.9f,  0.05f, 0.05f);

        Debug.Log("Rebuild complete. Reference_Layer (alpha=0.5) fills screen as guide.");
    }

    static void SetPosScale(string path, float x, float y, float sx, float sy)
    {
        var go = GameObject.Find(path);
        if (go == null) { Debug.LogWarning("Not found: " + path); return; }
        go.transform.position = new Vector3(x, y, 0);
        go.transform.localScale = new Vector3(sx, sy, 1);
    }
}
