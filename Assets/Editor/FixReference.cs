using UnityEngine;
using UnityEditor;

public class FixReference
{
    [MenuItem("Farm/Fix Reference Scale")]
    public static void Execute()
    {
        // Image: 1376x768px, camera ortho=5 => height=10 world units
        // PPU = 768 / 10 = 76.8 => use 76.8 exactly via spritePixelsPerUnit (float)
        // World size at PPU=76.8: 1376/76.8 = 17.92w, 768/76.8 = 10h ✓

        var imp = AssetImporter.GetAtPath("Assets/Sprites/Farm_Reference.png") as TextureImporter;
        if (imp != null)
        {
            imp.spritePixelsPerUnit = 76.8f;
            imp.filterMode = FilterMode.Point;
            imp.textureCompression = TextureImporterCompression.Uncompressed;
            imp.spriteImportMode = SpriteImportMode.Single;
            imp.SaveAndReimport();
        }

        var go = GameObject.Find("Reference_Layer");
        if (go != null)
        {
            go.transform.position = Vector3.zero;
            go.transform.localScale = Vector3.one;
            var sr = go.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Farm_Reference.png");
                sr.color = new Color(1, 1, 1, 0.5f);
                sr.sortingOrder = -999;
            }
        }

        Debug.Log("Reference fixed. Bounds should now be ~17.92 x 10.");
    }
}
