using UnityEngine;
using UnityEditor;
using System.Linq;

public class ImportFromRef
{
    const string ROOT = "Assets/Sprites/FromRef";

    [MenuItem("Tools/Import FromRef Assets")]
    public static void Run()
    {
        string[] paths = {
            ROOT + "/props/bridge.png",
            ROOT + "/props/crop_field.png",
            ROOT + "/background/sky_strip.png",
            ROOT + "/background/cloud_1.png",
            ROOT + "/background/cloud_2.png",
        };

        foreach (var path in paths)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) { Debug.LogWarning("[FromRef] Not found: " + path); continue; }

            importer.textureType         = TextureImporterType.Sprite;
            importer.spriteImportMode    = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 48f;
            importer.filterMode          = FilterMode.Point;
            importer.textureCompression  = TextureImporterCompression.Uncompressed;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled       = false;
            EditorUtility.SetDirty(importer);
            importer.SaveAndReimport();
            Debug.Log("[FromRef] Imported: " + path);
        }

        // Now swap assets in scene
        SwapSceneAssets();
        Debug.Log("[FromRef] Done.");
    }

    static void SwapSceneAssets()
    {
        var mat = FindUnlitMat();

        // ── Sky ──────────────────────────────────────────────────────────────
        var skySprite = AssetDatabase.LoadAssetAtPath<Sprite>(ROOT + "/background/sky_strip.png");
        var skyGO = GameObject.Find("SkyBG");
        if (skyGO != null && skySprite != null)
        {
            var sr = skyGO.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sprite = skySprite;
                // scale to fill full visible width; sky_strip is 1365x104px at PPU=48
                // world width = 1365/48 = 28.4, height = 104/48 = 2.17
                // We want it to fill x=0..20, so scale uniformly to cover width
                // At scale=1, sprite is 28.4 wide. Camera shows ~20 wide. Use scale=1 centered at x=10
                skyGO.transform.position   = new Vector3(10f, 14f, 1f);
                skyGO.transform.localScale  = Vector3.one;
                sr.sortingOrder = -10;
                if (mat) sr.sharedMaterial = mat;
                Debug.Log("[FromRef] SkyBG updated.");
            }
        }

        // ── Bridge ───────────────────────────────────────────────────────────
        var bridgeSprite = AssetDatabase.LoadAssetAtPath<Sprite>(ROOT + "/props/bridge.png");
        var bridgeGO = GameObject.Find("Bridge");
        if (bridgeGO != null && bridgeSprite != null)
        {
            // Remove old children, place single SpriteRenderer
            foreach (Transform child in bridgeGO.transform)
                Object.DestroyImmediate(child.gameObject);

            var sr_go = new GameObject("BridgeSprite");
            sr_go.transform.SetParent(bridgeGO.transform, false);
            // bridge.png: 140x180px at PPU=48 → world size 2.9 x 3.75
            // Place centered on river crossing: x=10.5, y=1.5
            sr_go.transform.position = new Vector3(10.5f, 1.5f, 0f);
            var sr = sr_go.AddComponent<SpriteRenderer>();
            sr.sprite = bridgeSprite;
            sr.sortingOrder = 12;
            if (mat) sr.sharedMaterial = mat;
            Debug.Log("[FromRef] Bridge updated.");
        }

        // ── Crops ─────────────────────────────────────────────────────────
        var cropSprite = AssetDatabase.LoadAssetAtPath<Sprite>(ROOT + "/props/crop_field.png");
        var cropsGO = GameObject.Find("Crops");
        if (cropsGO != null && cropSprite != null)
        {
            foreach (Transform child in cropsGO.transform)
                Object.DestroyImmediate(child.gameObject);

            var sr_go = new GameObject("CropSprite");
            sr_go.transform.SetParent(cropsGO.transform, false);
            // crop_field.png: 197x170px → world 4.1 x 3.5 at PPU=48
            // Place center at x=13, y=6
            sr_go.transform.position = new Vector3(13f, 6.2f, 0f);
            var sr = sr_go.AddComponent<SpriteRenderer>();
            sr.sprite = cropSprite;
            sr.sortingOrder = 6;
            if (mat) sr.sharedMaterial = mat;
            Debug.Log("[FromRef] Crops updated.");
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    }

    static Material FindUnlitMat()
    {
        foreach (var guid in AssetDatabase.FindAssets("Sprite-Unlit-Default t:Material"))
        {
            var p = AssetDatabase.GUIDToAssetPath(guid);
            if (p.Contains("com.unity.render-pipelines")) return AssetDatabase.LoadAssetAtPath<Material>(p);
        }
        return null;
    }
}
