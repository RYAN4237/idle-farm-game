using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class FixSceneBackground
{
    const string WHITE_TEX_PATH = "Assets/Textures/WhiteFill.png";
    const string SKY_TEX_PATH   = "Assets/Brackeys/2D Mega Pack/Backgrounds/SkyBackground.png";

    [MenuItem("Tools/Fix Scene Background")]
    public static void Execute()
    {
        // ── Camera clear color ───────────────────────────────────────────────
        var cam = Camera.main;
        if (cam != null)
        {
            cam.backgroundColor = new Color(0.55f, 0.82f, 0.98f, 1f);
            EditorUtility.SetDirty(cam.gameObject);
        }

        EnsureWhiteTexture();
        EnsureSkyTexture();

        var whiteSprite = AssetDatabase.LoadAssetAtPath<Sprite>(WHITE_TEX_PATH);
        if (whiteSprite == null) { Debug.LogError("[FixBG] Could not load WhiteFill sprite"); return; }

        var skySprite = AssetDatabase.LoadAssetAtPath<Sprite>(SKY_TEX_PATH);
        if (skySprite == null) { Debug.LogError("[FixBG] Could not load SkyBackground sprite"); return; }

        // Camera: ortho=5, y=2 → sees y: -3..7, aspect 16:9 → x: -6.9..26.9
        // SkyBackground.png = 400×225px, PPU=100 → native 4×2.25 world units
        // scale to 40×10 → (10, 4.44, 1), center y=4 → covers y:-1..9

        // ── SkyBackground ────────────────────────────────────────────────────
        var skyGO = GameObject.Find("SkyBackground");
        if (skyGO == null)
        {
            skyGO = new GameObject("SkyBackground");
            skyGO.AddComponent<SpriteRenderer>();
        }
        {
            skyGO.transform.position   = new Vector3(10f, 4f, 5f);
            skyGO.transform.localScale = new Vector3(40f / 4f, 10f / 2.25f, 1f);
            var sr = skyGO.GetComponent<SpriteRenderer>();
            sr.sprite       = skySprite;
            sr.sortingOrder = -9;
            EditorUtility.SetDirty(skyGO);
        }

        // ── GroundFill ───────────────────────────────────────────────────────
        var old = GameObject.Find("GroundFill");
        if (old != null) Object.DestroyImmediate(old);

        var gfGO = new GameObject("GroundFill");
        // PPU=4, sprite 4px → native = 1×1 world unit
        // scale (40, 16, 1) → 40×16 world units, center (10,2) → y:-6..10
        gfGO.transform.position   = new Vector3(10f, 2f, 5f);
        gfGO.transform.localScale = new Vector3(40f, 16f, 1f);

        var sr2 = gfGO.AddComponent<SpriteRenderer>();
        sr2.sprite       = whiteSprite;
        sr2.color        = new Color(0.53f, 0.72f, 0.33f, 1f);
        sr2.sortingOrder = -10;

        EditorUtility.SetDirty(gfGO);
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log("[FixSceneBackground] Done");
    }

    static void EnsureWhiteTexture()
    {
        if (!System.IO.Directory.Exists("Assets/Textures"))
            AssetDatabase.CreateFolder("Assets", "Textures");

        bool existed = AssetDatabase.LoadAssetAtPath<Texture2D>(WHITE_TEX_PATH) != null;

        if (!existed)
        {
            var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
            var pixels = new Color[16];
            for (int i = 0; i < 16; i++) pixels[i] = Color.white;
            tex.SetPixels(pixels);
            tex.Apply();
            System.IO.File.WriteAllBytes(WHITE_TEX_PATH, tex.EncodeToPNG());
            AssetDatabase.ImportAsset(WHITE_TEX_PATH);
        }

        // Always ensure correct import settings: Sprite, PPU=4, point, uncompressed
        var ti = AssetImporter.GetAtPath(WHITE_TEX_PATH) as TextureImporter;
        if (ti != null)
        {
            ti.textureType        = TextureImporterType.Sprite;
            ti.spritePixelsPerUnit = 4f;   // 4px texture → 1×1 world unit native
            ti.filterMode         = FilterMode.Point;
            ti.textureCompression = TextureImporterCompression.Uncompressed;
            ti.SaveAndReimport();
        }

        AssetDatabase.Refresh();
    }

    static void EnsureSkyTexture()
    {
        var ti = AssetImporter.GetAtPath(SKY_TEX_PATH) as TextureImporter;
        if (ti == null) return;
        if (ti.textureType != TextureImporterType.Sprite ||
            (int)ti.spritePixelsPerUnit != 100)
        {
            ti.textureType         = TextureImporterType.Sprite;
            ti.spritePixelsPerUnit = 100f;
            ti.filterMode          = FilterMode.Point;
            ti.textureCompression  = TextureImporterCompression.Uncompressed;
            ti.SaveAndReimport();
        }
    }
}
