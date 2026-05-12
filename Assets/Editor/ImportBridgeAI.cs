using UnityEngine;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using System.Collections.Generic;

public class ImportBridgeAI
{
    [MenuItem("Tools/Import Bridge AI")]
    public static void Run()
    {
        const string path = "Assets/Sprites/Bridge_AI.png";

        // ── importer settings ──────────────────────────────────────────────
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType        = TextureImporterType.Sprite;
        importer.spriteImportMode   = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = 48f;
        importer.filterMode         = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled      = false;

        EditorUtility.SetDirty(importer);
        importer.SaveAndReimport();

        Debug.Log("[BridgeAI] Import done — sprite at " + path);
    }
}
