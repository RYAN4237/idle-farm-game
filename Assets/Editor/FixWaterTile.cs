using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using System.Linq;

/// Reslices Water.png into 4×16x16 frames, then rebuilds water tilemap
public class FixWaterTile
{
    [MenuItem("Tools/Fix Water Tile")]
    public static void Execute()
    {
        // 1. Reslice Water.png as Multiple 16x16
        string waterPath = "Assets/Sprout Lands - Sprites - Basic pack/Tilesets/Water.png";
        var ti = AssetImporter.GetAtPath(waterPath) as TextureImporter;
        if (ti == null) { Debug.LogError("Cannot load Water.png importer"); return; }

        ti.spriteImportMode  = SpriteImportMode.Multiple;
        ti.filterMode        = FilterMode.Point;
        ti.textureCompression = TextureImporterCompression.Uncompressed;

        var meta = new SpriteMetaData[4];
        for (int i = 0; i < 4; i++)
        {
            meta[i] = new SpriteMetaData
            {
                name   = $"Water_{i}",
                rect   = new Rect(i * 16, 0, 16, 16),
                pivot  = new Vector2(0.5f, 0.5f),
                alignment = 9
            };
        }
        ti.spritesheet = meta;
        EditorUtility.SetDirty(ti);
        ti.SaveAndReimport();
        AssetDatabase.Refresh();
        Debug.Log("[FixWaterTile] Water.png resliced into 4×16×16 frames");

        // 2. Rebuild water tilemap using Water_0 (first frame)
        var allWater = AssetDatabase.LoadAllAssetsAtPath(waterPath).OfType<Sprite>().ToArray();
        var waterSpr = allWater.FirstOrDefault(s => s.name == "Water_0");
        if (waterSpr == null) { Debug.LogError("Water_0 not found after reslice"); return; }

        var waterGO = GameObject.Find("Tilemap/WaterLayer") ?? GameObject.Find("WaterLayer");
        if (waterGO == null) { Debug.LogError("WaterLayer not found"); return; }

        var tm = waterGO.GetComponent<Tilemap>();
        tm.ClearAllTiles();

        Tile MakeWater()
        {
            var t   = ScriptableObject.CreateInstance<Tile>();
            t.sprite = waterSpr;
            t.color  = new Color(0.55f, 0.82f, 0.90f, 1f);
            return t;
        }

        // River y=2 full width
        for (int x = -3; x < 23; x++)
            tm.SetTile(new Vector3Int(x, 2, 0), MakeWater());

        // Pond: irregular shape, lower-left
        int[,] pond = {
            {1,-2},{2,-2},{3,-2},{4,-2},
            {1,-1},{2,-1},{3,-1},{4,-1},{5,-1},
            {2, 0},{3, 0},{4, 0}
        };
        for (int i = 0; i < pond.GetLength(0); i++)
            tm.SetTile(new Vector3Int(pond[i,0], pond[i,1], 0), MakeWater());

        // Upper pool: irregular shape, upper-right area
        int[,] pool = {
            {16,5},{17,5},{18,5},
            {17,6},{18,6},{19,6}
        };
        for (int i = 0; i < pool.GetLength(0); i++)
            tm.SetTile(new Vector3Int(pool[i,0], pool[i,1], 0), MakeWater());

        EditorUtility.SetDirty(waterGO);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("[FixWaterTile] WaterLayer rebuilt with correct 16x16 tiles");
    }
}
