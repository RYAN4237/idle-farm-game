using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using System.Linq;

/// Replace water tiles with a solid-color tile generated from a 1x1 white pixel
/// to eliminate all seam/gap artifacts
public class FixWaterSolid
{
    [MenuItem("Tools/Fix Water Solid Color")]
    public static void Execute()
    {
        string tileDir = "Assets/Tiles";
        if (!AssetDatabase.IsValidFolder(tileDir))
            AssetDatabase.CreateFolder("Assets", "Tiles");

        // Create a 1x1 white texture and save as sprite asset
        string texPath = tileDir + "/WaterSolidTex.png";
        var tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        System.IO.File.WriteAllBytes(
            System.IO.Path.Combine(Application.dataPath, "../" + texPath),
            tex.EncodeToPNG());
        AssetDatabase.ImportAsset(texPath);

        // Set as sprite
        var ti = AssetImporter.GetAtPath(texPath) as TextureImporter;
        ti.textureType        = TextureImporterType.Sprite;
        ti.spriteImportMode   = SpriteImportMode.Single;
        ti.spritePixelsPerUnit = 1;
        ti.filterMode         = FilterMode.Point;
        ti.textureCompression = TextureImporterCompression.Uncompressed;
        EditorUtility.SetDirty(ti);
        ti.SaveAndReimport();
        AssetDatabase.Refresh();

        var spr = AssetDatabase.LoadAssetAtPath<Sprite>(texPath);
        if (spr == null) { Debug.LogError("Failed to load WaterSolidTex sprite"); return; }

        // Create water tile asset
        string tilePath = tileDir + "/WaterSolid.asset";
        AssetDatabase.DeleteAsset(tilePath);
        var tile = ScriptableObject.CreateInstance<Tile>();
        tile.sprite = spr;
        tile.color  = new Color(0.35f, 0.70f, 0.85f, 1f); // water blue
        AssetDatabase.CreateAsset(tile, tilePath);
        AssetDatabase.SaveAssets();

        var savedTile = AssetDatabase.LoadAssetAtPath<Tile>(tilePath);
        if (savedTile == null) { Debug.LogError("Tile save failed"); return; }

        // Fill WaterLayer
        var waterGO = GameObject.Find("Tilemap/WaterLayer") ?? GameObject.Find("WaterLayer");
        if (waterGO == null) { Debug.LogError("WaterLayer not found"); return; }
        var tm = waterGO.GetComponent<Tilemap>();
        tm.ClearAllTiles();
        tm.color = Color.white;

        // River y=2
        for (int x = -3; x < 23; x++)
            tm.SetTile(new Vector3Int(x, 2, 0), savedTile);

        // Pond
        int[,] pond = {
            {1,-2},{2,-2},{3,-2},{4,-2},
            {1,-1},{2,-1},{3,-1},{4,-1},{5,-1},
            {2, 0},{3, 0},{4, 0}
        };
        for (int i = 0; i < pond.GetLength(0); i++)
            tm.SetTile(new Vector3Int(pond[i,0], pond[i,1], 0), savedTile);

        // Upper pool
        int[,] pool = {
            {16,5},{17,5},{18,5},
            {17,6},{18,6},{19,6}
        };
        for (int i = 0; i < pool.GetLength(0); i++)
            tm.SetTile(new Vector3Int(pool[i,0], pool[i,1], 0), savedTile);

        EditorUtility.SetDirty(waterGO);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("[FixWaterSolid] Done — water replaced with solid color tiles");
    }
}
